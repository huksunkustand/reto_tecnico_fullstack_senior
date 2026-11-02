using System.ComponentModel.DataAnnotations;
using Pedidos.Domain;

namespace Pedidos.Application;

public sealed record PedidoInput(
    [Required, MaxLength(50)] string NumeroPedido,
    [Required, MaxLength(150)] string Cliente,
    DateOnly Fecha,
    decimal Total,
    [Required] string Estado,
    string? Version = null);
public sealed record PedidoDto(int Id, string NumeroPedido, string Cliente, DateOnly Fecha, decimal Total, string Estado, string Version)
{
    public static PedidoDto From(Pedido pedido) => new(pedido.Id, pedido.NumeroPedido, pedido.Cliente,
        pedido.Fecha, pedido.Total, pedido.Estado, Convert.ToBase64String(pedido.Version));
}
public sealed record Page<T>(IReadOnlyList<T> Items, int Total, int PageNumber, int PageSize);
public sealed record LoginInput([Required, EmailAddress, MaxLength(200)] string Email, [Required, MaxLength(200)] string Password);
public sealed record LoginResult(string Token, int ExpiresIn, string Email, string Rol);
public sealed class NotFoundException() : Exception("Pedido no encontrado.");
public sealed class ConflictException(string message) : Exception(message);

public interface IPedidoRepository
{
    Task<Page<PedidoDto>> ListAsync(string? search, string? estado, int page, int pageSize, CancellationToken cancellationToken);
    Task<Pedido?> FindAsync(int id, CancellationToken cancellationToken);
    Task<bool> NumberExistsAsync(string number, int? exceptId, CancellationToken cancellationToken);
    void Add(Pedido pedido);
    void SetVersion(Pedido pedido, byte[] version);
    Task SaveAsync(CancellationToken cancellationToken);
}

public interface IAuthService
{
    Task<LoginResult?> LoginAsync(LoginInput input, CancellationToken cancellationToken);
}

public sealed class PedidoService(IPedidoRepository repository)
{
    public Task<Page<PedidoDto>> ListAsync(string? search, string? estado, int page, int pageSize, CancellationToken cancellationToken)
    {
        if (page < 1 || pageSize is < 1 or > 100 || search?.Length > 150)
            throw new BusinessException("Paginacion o busqueda no valida. El tamano de pagina debe estar entre 1 y 100.");
        return repository.ListAsync(search?.Trim(), estado, page, pageSize, cancellationToken);
    }

    public async Task<PedidoDto> GetAsync(int id, CancellationToken cancellationToken)
        => PedidoDto.From(await repository.FindAsync(id, cancellationToken) ?? throw new NotFoundException());

    public async Task<PedidoDto> CreateAsync(PedidoInput input, CancellationToken cancellationToken)
    {
        var pedido = new Pedido(input.NumeroPedido, input.Cliente, input.Fecha, input.Total, input.Estado);
        if (await repository.NumberExistsAsync(pedido.NumeroPedido, null, cancellationToken))
            throw new ConflictException("El numero de pedido ya existe, incluso si fue eliminado.");
        repository.Add(pedido);
        await repository.SaveAsync(cancellationToken);
        return PedidoDto.From(pedido);
    }

    public async Task<PedidoDto> UpdateAsync(int id, PedidoInput input, CancellationToken cancellationToken)
    {
        var pedido = await repository.FindAsync(id, cancellationToken) ?? throw new NotFoundException();
        if (string.IsNullOrWhiteSpace(input.Version))
            throw new BusinessException("La version del pedido es obligatoria para editar.");
        byte[] version;
        try { version = Convert.FromBase64String(input.Version); }
        catch (FormatException) { throw new BusinessException("La version no es valida."); }
        if (version.Length != 8) throw new BusinessException("La version no es valida.");
        pedido.Actualizar(input.NumeroPedido, input.Cliente, input.Fecha, input.Total, input.Estado);
        if (await repository.NumberExistsAsync(pedido.NumeroPedido, id, cancellationToken))
            throw new ConflictException("El numero de pedido ya existe, incluso si fue eliminado.");
        repository.SetVersion(pedido, version);
        await repository.SaveAsync(cancellationToken);
        return PedidoDto.From(pedido);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var pedido = await repository.FindAsync(id, cancellationToken) ?? throw new NotFoundException();
        pedido.Eliminar();
        await repository.SaveAsync(cancellationToken);
    }
}