using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pedidos.Infrastructure;

namespace Pedidos.Api;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PedidosDbContext>
{
    public PedidosDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__SqlServer")
            ?? "Server=.\\SQLEXPRESS;Database=AtlanticPedidos;Integrated Security=True;TrustServerCertificate=True";
        return new(new DbContextOptionsBuilder<PedidosDbContext>()
            .UseSqlServer(connection, sql => sql.MigrationsAssembly("Pedidos.Infrastructure")).Options);
    }
}