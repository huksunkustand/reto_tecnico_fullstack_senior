USE [AtlanticPedidos];
GO
SELECT Id, NumeroPedido, Cliente, Fecha, Total, Estado, Version
FROM dbo.Pedidos WHERE Eliminado = 0 ORDER BY Id DESC;
GO
DECLARE @Id int = 1;
SELECT Id, NumeroPedido, Cliente, Fecha, Total, Estado
FROM dbo.Pedidos WHERE Id = @Id AND Eliminado = 0;
GO
DECLARE @Busqueda nvarchar(150) = N'Juan';
SELECT Id, NumeroPedido, Cliente, Fecha, Total, Estado
FROM dbo.Pedidos WHERE Eliminado = 0 AND (Cliente LIKE N'%' + @Busqueda + N'%' OR NumeroPedido LIKE N'%' + @Busqueda + N'%');
GO
SELECT Estado, COUNT(*) AS Cantidad, SUM(Total) AS Total
FROM dbo.Pedidos WHERE Eliminado = 0 GROUP BY Estado;
GO
SELECT Id, Email, Rol FROM dbo.Usuarios;
SELECT MigrationId, ProductVersion FROM dbo.__EFMigrationsHistory ORDER BY MigrationId;
GO
BEGIN TRANSACTION;
DECLARE @Numero nvarchar(50) = N'QUERY-' + LEFT(CONVERT(nvarchar(36), NEWID()), 8);
INSERT INTO dbo.Pedidos (NumeroPedido, Cliente, Fecha, Total, Estado, Eliminado)
VALUES (@Numero, N'Cliente de prueba', CAST(GETDATE() AS date), 250.75, N'Registrado', 0);
DECLARE @NuevoId int = SCOPE_IDENTITY();
UPDATE dbo.Pedidos SET Estado = N'En proceso', Total = 300.00 WHERE Id = @NuevoId;
SELECT * FROM dbo.Pedidos WHERE Id = @NuevoId;
UPDATE dbo.Pedidos SET Eliminado = 1 WHERE Id = @NuevoId;
ROLLBACK;
GO