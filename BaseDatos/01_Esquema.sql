USE master;
GO
IF DB_ID(N'CatalogoRopaCalzado') IS NULL
    CREATE DATABASE CatalogoRopaCalzado;
GO
USE CatalogoRopaCalzado;
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.Categorias', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categorias (
        CategoriaId INT IDENTITY(1,1) CONSTRAINT PK_Categorias PRIMARY KEY,
        Nombre NVARCHAR(60) NOT NULL CONSTRAINT UQ_Categorias_Nombre UNIQUE,
        CONSTRAINT CK_Categorias_Nombre CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0)
    );
END;

IF OBJECT_ID(N'dbo.Productos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Productos (
        ProductoId INT IDENTITY(1,1) CONSTRAINT PK_Productos PRIMARY KEY,
        Nombre NVARCHAR(120) NOT NULL,
        Talla NVARCHAR(30) NULL,
        CategoriaId INT NOT NULL,
        Precio DECIMAL(12,2) NOT NULL,
        Stock INT NOT NULL,
        CONSTRAINT FK_Productos_Categorias FOREIGN KEY (CategoriaId) REFERENCES dbo.Categorias(CategoriaId),
        CONSTRAINT CK_Productos_Nombre CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0),
        CONSTRAINT CK_Productos_Precio CHECK (Precio >= 0),
        CONSTRAINT CK_Productos_Stock CHECK (Stock >= 0)
    );
    CREATE INDEX IX_Productos_CategoriaId ON dbo.Productos(CategoriaId);
END;

COMMIT TRANSACTION;
GO
