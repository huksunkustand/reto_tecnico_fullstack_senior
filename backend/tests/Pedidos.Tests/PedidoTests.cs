using Moq;
using Pedidos.Application;
using Pedidos.Domain;
using Xunit;

namespace Pedidos.Tests;

public sealed class PedidoTests
{
    private static readonly DateOnly Fecha = new(2026, 11, 1);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100000000)]
    [InlineData(1.001)]
    public void RechazaTotalesInvalidos(decimal total)
        => Assert.Throws<BusinessException>(() => new Pedido("PED-001", "Cliente", Fecha, total, "Registrado"));

    [Theory]
    [InlineData("", "Cliente", "Registrado")]
    [InlineData("PED-001", " ", "Registrado")]
    [InlineData("PED-001", "Cliente", "Otro")]
    public void RechazaCamposInvalidos(string numero, string cliente, string estado)
        => Assert.Throws<BusinessException>(() => new Pedido(numero, cliente, Fecha, 20, estado));

    [Fact]
    public void NormalizaNumeroYCliente()
    {
        var pedido = new Pedido(" ped-001 ", " Ana Perez ", Fecha, 250.75m, "Registrado");
        Assert.Equal("PED-001", pedido.NumeroPedido);
        Assert.Equal("Ana Perez", pedido.Cliente);
    }

    [Fact]
    public void RechazaFechaVacia()
        => Assert.Throws<BusinessException>(() => new Pedido("PED-001", "Cliente", default, 10, "Registrado"));

    [Fact]
    public void RechazaTextosLargos()
    {
        Assert.Throws<BusinessException>(() => new Pedido(new string('A', 51), "Cliente", Fecha, 10, "Registrado"));
        Assert.Throws<BusinessException>(() => new Pedido("PED-001", new string('A', 151), Fecha, 10, "Registrado"));
    }

    [Fact]
    public void EliminacionEsLogica()
    {
        var pedido = new Pedido("PED-001", "Cliente", Fecha, 10, "Registrado");
        pedido.Eliminar();
        Assert.True(pedido.Eliminado);
        Assert.Equal("PED-001", pedido.NumeroPedido);
    }

    [Fact]
    public async Task RechazaNumeroDuplicado()
    {
        var repository = new Mock<IPedidoRepository>();
        repository.Setup(instance => instance.NumberExistsAsync("PED-001", null, default)).ReturnsAsync(true);
        var service = new PedidoService(repository.Object);
        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(new("ped-001", "Cliente", Fecha, 10, "Registrado"), default));
        repository.Verify(instance => instance.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PedidoInexistenteDevuelveNotFound()
    {
        var service = new PedidoService(new Mock<IPedidoRepository>().Object);
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetAsync(1, default));
        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(1, default));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("no-base64")]
    [InlineData("YWJj")]
    public async Task EdicionRequiereVersionValida(string? version)
    {
        var repository = new Mock<IPedidoRepository>();
        repository.Setup(instance => instance.FindAsync(1, default)).ReturnsAsync(new Pedido("PED-001", "Cliente", Fecha, 10, "Registrado"));
        var service = new PedidoService(repository.Object);
        await Assert.ThrowsAsync<BusinessException>(() => service.UpdateAsync(1, new("PED-001", "Cliente", Fecha, 20, "Registrado", version), default));
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task RechazaPaginacionInvalida(int page, int pageSize)
    {
        var service = new PedidoService(new Mock<IPedidoRepository>().Object);
        await Assert.ThrowsAsync<BusinessException>(() => service.ListAsync(null, null, page, pageSize, default));
    }
}