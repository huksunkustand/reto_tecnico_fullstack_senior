USE [master];
GO
IF DB_ID(N'AtlanticPedidos') IS NULL
BEGIN
    CREATE DATABASE [AtlanticPedidos];
END;
GO