using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Pedidos.Application;
using Pedidos.Domain;

namespace Pedidos.Infrastructure;

public sealed class PedidoRepository(PedidosDbContext database) : IPedidoRepository
{
    public async Task<Page<PedidoDto>> ListAsync(string? search, string? estado, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = database.Pedidos.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(pedido => pedido.NumeroPedido.Contains(search) || pedido.Cliente.Contains(search));
        if (!string.IsNullOrWhiteSpace(estado)) query = query.Where(pedido => pedido.Estado == estado);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(pedido => pedido.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new(items.Select(PedidoDto.From).ToList(), total, page, pageSize);
    }

    public Task<Pedido?> FindAsync(int id, CancellationToken cancellationToken)
        => database.Pedidos.SingleOrDefaultAsync(pedido => pedido.Id == id, cancellationToken);

    public Task<bool> NumberExistsAsync(string number, int? exceptId, CancellationToken cancellationToken)
        => database.Pedidos.IgnoreQueryFilters().AnyAsync(pedido => pedido.NumeroPedido == number && pedido.Id != exceptId, cancellationToken);

    public void Add(Pedido pedido) => database.Pedidos.Add(pedido);
    public void SetVersion(Pedido pedido, byte[] version)
        => database.Entry(pedido).Property(entity => entity.Version).OriginalValue = version;

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        try { await database.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException)
        { throw new ConflictException("El pedido cambio. Actualiza el listado e intenta nuevamente."); }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        { throw new ConflictException("El numero de pedido ya existe."); }
    }
}