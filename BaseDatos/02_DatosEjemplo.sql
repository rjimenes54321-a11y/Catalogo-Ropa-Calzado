USE CatalogoRopaCalzado;
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;

INSERT INTO dbo.Categorias (Nombre)
SELECT v.Nombre
FROM (VALUES (N'Ropa'), (N'Calzado')) AS v(Nombre)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Categorias AS c WITH (UPDLOCK, HOLDLOCK) WHERE c.Nombre = v.Nombre
);

INSERT INTO dbo.Productos (Codigo, Nombre, Talla, CategoriaId, Precio, Stock, Descripcion)
SELECT v.Codigo, v.Nombre, v.Talla, c.CategoriaId, v.Precio, v.Stock, v.Descripcion
FROM (VALUES
    (N'DEMO-01', N'Camiseta de algodón', N'M', N'Ropa', 14.95, 18, N'Camiseta de corte regular para uso diario.'),
    (N'DEMO-02', N'Pantalón recto', N'32', N'Ropa', 32.50, 9, N'Pantalón de corte recto.'),
    (N'DEMO-03', N'Chaqueta ligera', N'L', N'Ropa', 39.95, 0, N'Chaqueta de peso ligero.'),
    (N'DEMO-04', N'Tenis urbanos', N'39', N'Calzado', 49.90, 12, N'Tenis con cordones para uso urbano.'),
    (N'DEMO-05', N'Sandalias de paseo', N'38', N'Calzado', 24.75, 6, N'Sandalias para recorridos cotidianos.'),
    (N'DEMO-06', N'Botines casuales', N'40', N'Calzado', 55.00, 0, N'Botines de estilo casual.')
) AS v(Codigo, Nombre, Talla, Categoria, Precio, Stock, Descripcion)
JOIN dbo.Categorias AS c ON c.Nombre = v.Categoria
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Productos AS p WITH (UPDLOCK, HOLDLOCK)
    WHERE p.Codigo = v.Codigo OR (p.Nombre = v.Nombre AND p.Talla = v.Talla AND p.CategoriaId = c.CategoriaId)
);

-- Se añaden fotografías al catálogo de demostración, incluso si se sembró antes de la migración.
UPDATE dbo.Productos
SET Imagen = N'https://images.unsplash.com/photo-1722310752951-4d459d28c678?auto=format&fit=crop&w=800&q=75'
WHERE Nombre = N'Camiseta de algodón' AND Imagen IS NULL AND (Codigo = N'DEMO-01' OR Codigo LIKE N'P-%');

UPDATE dbo.Productos
SET Imagen = N'https://images.unsplash.com/photo-1622760808027-095ea611f657?auto=format&fit=crop&w=800&q=75'
WHERE Nombre = N'Tenis urbanos' AND Imagen IS NULL AND (Codigo = N'DEMO-04' OR Codigo LIKE N'P-%');

UPDATE dbo.Productos
SET Imagen = N'https://crownofwildflowers.wordpress.com/wp-content/uploads/2013/09/ptbl1-14014861v275.jpg?w=488'
WHERE Nombre = N'Botines casuales' AND NULLIF(LTRIM(RTRIM(Imagen)), N'') IS NULL
    AND (Codigo = N'DEMO-06' OR Codigo LIKE N'P-%');

UPDATE dbo.Productos
SET Imagen = N'https://res.charma.io/image/upload/v1749085103/Charma/Uploads/T9701.030__B_1__6f27c9bd8a9b412591da81a0a686fbd5_gh0kin.jpg'
WHERE Nombre = N'Chaqueta ligera' AND NULLIF(LTRIM(RTRIM(Imagen)), N'') IS NULL
    AND (Codigo = N'DEMO-03' OR Codigo LIKE N'P-%');

UPDATE dbo.Productos
SET Imagen = N'https://image.uniqlo.com/UQ/ST3/AsianCommon/imagesgoods/452081/sub/goods_452081_sub14_3x4.jpg?width=494'
WHERE Nombre = N'Pantalón recto' AND NULLIF(LTRIM(RTRIM(Imagen)), N'') IS NULL
    AND (Codigo = N'DEMO-02' OR Codigo LIKE N'P-%');

UPDATE dbo.Productos
SET Imagen = N'https://hypeshoes.in/cdn/shop/files/HYPE20-3.jpg?v=1732789192'
WHERE Nombre = N'Sandalias de paseo' AND NULLIF(LTRIM(RTRIM(Imagen)), N'') IS NULL
    AND (Codigo = N'DEMO-05' OR Codigo LIKE N'P-%');

COMMIT TRANSACTION;
GO
