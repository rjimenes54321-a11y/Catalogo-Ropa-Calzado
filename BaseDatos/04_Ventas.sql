USE CatalogoRopaCalzado;
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH('dbo.Productos', 'Codigo') IS NULL
BEGIN
    ALTER TABLE dbo.Productos ADD Codigo NVARCHAR(40) NULL, Descripcion NVARCHAR(500) NULL,
        Imagen NVARCHAR(500) NULL, Activo BIT NOT NULL CONSTRAINT DF_Productos_Activo DEFAULT 1;
    EXEC(N'UPDATE dbo.Productos SET Codigo = CONCAT(N''P-'', ProductoId)');
    ALTER TABLE dbo.Productos ALTER COLUMN Codigo NVARCHAR(40) NOT NULL;
    ALTER TABLE dbo.Productos ADD CONSTRAINT UQ_Productos_Codigo UNIQUE(Codigo);
    EXEC(N'UPDATE dbo.Productos SET Activo = 0 WHERE Precio = 0');
    EXEC(N'ALTER TABLE dbo.Productos ADD CONSTRAINT CK_Productos_PrecioActivo CHECK (Activo = 0 OR Precio > 0)');
END;

IF OBJECT_ID('dbo.Usuarios', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios (
        UsuarioId INT IDENTITY PRIMARY KEY,
        Nombre NVARCHAR(120) NOT NULL,
        Correo NVARCHAR(254) NOT NULL UNIQUE,
        ClaveHash NVARCHAR(200) NOT NULL,
        Rol NVARCHAR(20) NOT NULL CHECK (Rol IN (N'Comprador',N'Vendedor',N'Administrador')),
        Intentos INT NOT NULL DEFAULT 0 CHECK (Intentos >= 0),
        BloqueadoHasta DATETIME2 NULL
    );

    CREATE TABLE dbo.Pedidos (
        PedidoId INT IDENTITY PRIMARY KEY,
        CompradorId INT NOT NULL REFERENCES dbo.Usuarios(UsuarioId),
        CompradorNombre NVARCHAR(120) NOT NULL,
        CompradorCorreo NVARCHAR(254) NOT NULL,
        Fecha DATETIME2 NOT NULL,
        Direccion NVARCHAR(300) NOT NULL,
        Metodo NVARCHAR(30) NOT NULL CHECK (Metodo IN (N'Efectivo',N'Transferencia')),
        Estado NVARCHAR(20) NOT NULL CHECK (Estado IN (N'Pendiente',N'Pagado',N'Cancelado')),
        Total DECIMAL(18,2) NOT NULL CHECK (Total > 0),
        VendedorId INT NULL REFERENCES dbo.Usuarios(UsuarioId),
        FechaPago DATETIME2 NULL,
        CHECK ((Estado = N'Pagado' AND VendedorId IS NOT NULL AND FechaPago IS NOT NULL)
            OR (Estado <> N'Pagado' AND VendedorId IS NULL AND FechaPago IS NULL))
    );
    CREATE INDEX IX_Pedidos_Comprador ON dbo.Pedidos(CompradorId, Fecha);
    CREATE INDEX IX_Pedidos_Estado ON dbo.Pedidos(Estado, FechaPago);

    CREATE TABLE dbo.DetallePedidos (
        DetallePedidoId INT IDENTITY PRIMARY KEY,
        PedidoId INT NOT NULL REFERENCES dbo.Pedidos(PedidoId),
        ProductoId INT NOT NULL REFERENCES dbo.Productos(ProductoId),
        Nombre NVARCHAR(120) NOT NULL,
        Cantidad INT NOT NULL CHECK (Cantidad > 0),
        Precio DECIMAL(12,2) NOT NULL CHECK (Precio > 0),
        UNIQUE(PedidoId, ProductoId)
    );

    CREATE TABLE dbo.Facturas (
        FacturaId INT IDENTITY PRIMARY KEY,
        PedidoId INT NOT NULL UNIQUE REFERENCES dbo.Pedidos(PedidoId),
        Fecha DATETIME2 NOT NULL,
        VendedorId INT NOT NULL REFERENCES dbo.Usuarios(UsuarioId),
        VendedorNombre NVARCHAR(120) NOT NULL
    );
END;

COMMIT TRANSACTION;
GO
