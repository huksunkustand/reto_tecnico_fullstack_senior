USE [AtlanticPedidos];
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WITH (UPDLOCK, HOLDLOCK) WHERE Email = N'kleiver@gmail.com')
BEGIN
    INSERT INTO dbo.Usuarios (Email, PasswordHash, Rol)
    VALUES (
        N'kleiver@gmail.com',
        N'AQAAAAIAAYagAAAAED+Iikko9p86Bh/NHhvoAGp3BCVb7Cdfe/3QVCNVirbwfkbtGwlAneo/dT9uURMEUw==',
        N'Admin'
    );
END;

COMMIT;

SELECT Id, Email, Rol
FROM dbo.Usuarios
WHERE Email = N'kleiver@gmail.com';
GO