USE [AtlanticPedidos];
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;
INSERT INTO dbo.Pedidos (NumeroPedido, Cliente, Fecha, Total, Estado, Eliminado)
SELECT source.NumeroPedido, source.Cliente, source.Fecha, source.Total, source.Estado, 0
FROM (VALUES
    (N'PED-001', N'Juan Perez', CAST('2026-10-28' AS date), 250.75, N'Registrado'),
    (N'PED-002', N'Maria Rodriguez', CAST('2026-10-29' AS date), 1280.00, N'En proceso'),
    (N'PED-003', N'Comercial Pacifico SAC', CAST('2026-10-29' AS date), 3450.50, N'Completado'),
    (N'PED-004', N'Carlos Mendoza', CAST('2026-10-30' AS date), 89.90, N'Registrado'),
    (N'PED-005', N'Inversiones del Sur', CAST('2026-10-30' AS date), 760.00, N'En proceso'),
    (N'PED-006', N'Lucia Torres', CAST('2026-10-31' AS date), 420.00, N'Cancelado'),
    (N'PED-007', N'Grupo Horizonte', CAST('2026-10-31' AS date), 2150.25, N'Completado'),
    (N'PED-008', N'Andrea Flores', CAST('2026-11-01' AS date), 185.00, N'Registrado')
) AS source(NumeroPedido, Cliente, Fecha, Total, Estado)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Pedidos WITH (UPDLOCK, HOLDLOCK) WHERE NumeroPedido = source.NumeroPedido);
COMMIT;
GO