using Microsoft.EntityFrameworkCore;
using Pedidos.Domain;

namespace Pedidos.Infrastructure;

public sealed class PedidosDbContext(DbContextOptions<PedidosDbContext> options) : DbContext(options)
{
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var pedido = modelBuilder.Entity<Pedido>();
        pedido.ToTable("Pedidos", table =>
        {
            table.HasCheckConstraint("CK_Pedidos_Total", "[Total] > 0");
            table.HasCheckConstraint("CK_Pedidos_Estado", "[Estado] IN ('Registrado','En proceso','Completado','Cancelado')");
        });
        pedido.HasKey(entity => entity.Id);
        pedido.Property(entity => entity.NumeroPedido).HasMaxLength(50).IsRequired();
        pedido.HasIndex(entity => entity.NumeroPedido).IsUnique();
        pedido.Property(entity => entity.Cliente).HasMaxLength(150).IsRequired();
        pedido.Property(entity => entity.Total).HasPrecision(10, 2);
        pedido.Property(entity => entity.Estado).HasMaxLength(50).IsRequired();
        pedido.Property(entity => entity.Version).IsRowVersion();
        pedido.HasQueryFilter(entity => !entity.Eliminado);
        var usuario = modelBuilder.Entity<Usuario>();
        usuario.Property(entity => entity.Email).HasMaxLength(200).IsRequired();
        usuario.HasIndex(entity => entity.Email).IsUnique();
        usuario.Property(entity => entity.PasswordHash).HasMaxLength(500).IsRequired();
        usuario.Property(entity => entity.Rol).HasMaxLength(20).IsRequired();
    }
}