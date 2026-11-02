namespace Pedidos.Domain;

public sealed class BusinessException(string message) : Exception(message);

public sealed class Pedido
{
    public int Id { get; private set; }
    public string NumeroPedido { get; private set; } = "";
    public string Cliente { get; private set; } = "";
    public DateOnly Fecha { get; private set; }
    public decimal Total { get; private set; }
    public string Estado { get; private set; } = "Registrado";
    public bool Eliminado { get; private set; }
    public byte[] Version { get; private set; } = [];

    private Pedido() { }

    public Pedido(string numeroPedido, string cliente, DateOnly fecha, decimal total, string estado)
        => Actualizar(numeroPedido, cliente, fecha, total, estado);

    public void Actualizar(string numeroPedido, string cliente, DateOnly fecha, decimal total, string estado)
    {
        if (string.IsNullOrWhiteSpace(numeroPedido) || numeroPedido.Trim().Length > 50)
            throw new BusinessException("El numero de pedido es obligatorio y admite hasta 50 caracteres.");
        if (string.IsNullOrWhiteSpace(cliente) || cliente.Trim().Length > 150)
            throw new BusinessException("El cliente es obligatorio y admite hasta 150 caracteres.");
        if (total <= 0 || total > 99999999.99m || decimal.Round(total, 2) != total)
            throw new BusinessException("El total debe ser positivo, tener hasta 2 decimales y no superar 99999999.99.");
        if (fecha == default)
            throw new BusinessException("La fecha es obligatoria.");
        if (estado is not ("Registrado" or "En proceso" or "Completado" or "Cancelado"))
            throw new BusinessException("El estado no es valido.");
        NumeroPedido = numeroPedido.Trim().ToUpperInvariant();
        Cliente = cliente.Trim();
        Fecha = fecha;
        Total = total;
        Estado = estado;
    }

    public void Eliminar() => Eliminado = true;
}

public sealed class Usuario
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Rol { get; set; } = "User";
}