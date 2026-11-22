USE CatalogoRopaCalzado;
GO
SET NOCOUNT ON;
SET XACT_ABORT OFF;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @categoria INT = (SELECT CategoriaId FROM dbo.Categorias WHERE Nombre = N'Ropa');
    IF @categoria IS NULL
        THROW 51000, 'Carga primero los datos de ejemplo.', 1;

    DECLARE @nombre NVARCHAR(120) = N'Prueba SQL ' + CONVERT(NVARCHAR(36), NEWID());
    INSERT INTO dbo.Productos (Codigo, Nombre, Talla, CategoriaId, Precio, Stock)
    VALUES (CONVERT(NVARCHAR(36), NEWID()), @nombre, N'M', @categoria, 12.34, 6);
    DECLARE @producto INT = CONVERT(INT, SCOPE_IDENTITY());

    IF NOT EXISTS (SELECT 1 FROM dbo.Productos WHERE ProductoId = @producto)
        THROW 51001, 'Fallo en insercion.', 1;
    PRINT 'PASS insercion';

    IF NOT EXISTS (
        SELECT 1 FROM dbo.Productos p JOIN dbo.Categorias c ON c.CategoriaId = p.CategoriaId
        WHERE p.ProductoId = @producto AND p.Nombre LIKE N'Prueba SQL %'
          AND c.Nombre = N'Ropa' AND p.Precio = 12.34 AND p.Stock = 6
    )
        THROW 51002, 'Fallo en consulta.', 1;
    PRINT 'PASS consulta por nombre y categoria con precio y stock';

    UPDATE dbo.Productos SET Precio = 15.25, Stock = 0 WHERE ProductoId = @producto;
    IF NOT EXISTS (
        SELECT 1 FROM dbo.Productos WHERE ProductoId = @producto AND Precio = 15.25 AND Stock = 0
    )
        THROW 51003, 'Fallo en actualizacion.', 1;
    PRINT 'PASS actualizacion a stock cero';

    BEGIN TRY
        UPDATE dbo.Productos SET Stock = -1 WHERE ProductoId = @producto;
        THROW 51004, 'Se acepto stock negativo.', 1;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() <> 547 THROW;
        PRINT 'PASS restriccion de stock';
    END CATCH;

    BEGIN TRY
        UPDATE dbo.Productos SET Precio = -1 WHERE ProductoId = @producto;
        THROW 51005, 'Se acepto precio negativo.', 1;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() <> 547 THROW;
        PRINT 'PASS restriccion de precio';
    END CATCH;

    BEGIN TRY
        UPDATE dbo.Productos SET CategoriaId = 0 WHERE ProductoId = @producto;
        THROW 51006, 'Se acepto una categoria inexistente.', 1;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() <> 547 THROW;
        PRINT 'PASS integridad referencial';
    END CATCH;

    DELETE FROM dbo.Productos WHERE ProductoId = @producto;
    IF EXISTS (SELECT 1 FROM dbo.Productos WHERE ProductoId = @producto)
        THROW 51007, 'Fallo en eliminacion.', 1;
    PRINT 'PASS eliminacion';

    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
