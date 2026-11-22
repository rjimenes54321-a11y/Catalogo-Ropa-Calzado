# Arquitectura y módulos

## Ampliación de ventas

- `TiendaController` publica los productos activos y mantiene el carrito en la sesión del comprador. Cada consulta de pedido filtra por el identificador de la cuenta autenticada.
- `CuentaController` valida el registro y el acceso. La cookie identifica la cuenta y el rol se consulta en SQL Server. Las claves se derivan con PBKDF2-SHA256 y una sal por cuenta. El registro público asigna el rol Comprador en el servidor.
- `PanelController` centraliza pedidos, facturación, clientes, categorías y reportes. El acceso al listado de usuarios y a la creación de cuentas privadas requiere el rol Administrador.
- `Ventas` aplica la transición de estados y actualiza stock con comandos SQL parametrizados. La confirmación del pedido y su descuento se ejecutan en una transacción. Solo una cancelación de un pedido pendiente puede devolver existencias.
- `DetallePedidos` conserva cantidad, nombre y precio históricos. Cada pedido pagado puede vincularse a una factura interna única. La factura reutiliza los datos históricos de su pedido.

## Solicitud de consulta

El navegador envía un GET a `Tienda/Catalogo` o a `Productos/Index`, según la interfaz. El controlador compone una consulta LINQ con los filtros recibidos. EF6 traduce la consulta a SQL cuando se ejecuta `ToList()`. La vista Razor muestra los resultados y conserva los filtros.

## Solicitud de escritura

La vista muestra un formulario con un token antifalsificación. El controlador comprueba el token, vincula una lista explícita de campos de entrada, valida el modelo y comprueba que la categoría existe. Luego copia los valores a la entidad y ejecuta `SaveChanges()`. Después de guardar redirige al inventario para evitar reenviar el formulario al actualizar la página. La desactivación pide confirmación y escribe mediante POST.

## Responsabilidades

| Ruta | Responsabilidad |
|---|---|
| `Models/` | Entidades `Producto` y `Categoria`, mapeadas a tablas de SQL Server. |
| `Data/CatalogoContext.cs` | Contexto EF6, relación entre tablas y precisión del precio. El esquema se instala mediante un script. |
| `ViewModels/` | Campos que presenta cada vista y validaciones del formulario. |
| `Controllers/ProductosController.cs` | Búsqueda, CRUD, errores de operaciones y coordinación entre contexto y vistas. |
| `Views/` y `Content/` | Vistas Razor, formulario común, estructura y estilos adaptables. |
| `App_Start/RouteConfig.cs` | Rutas que vinculan la URL con el controlador y sus acciones. |
| `BaseDatos/` | Tablas, restricciones, datos sintéticos y comprobación SQL. |

## Decisiones

- MVC 5 clásico, .NET Framework 4.8, C#, Razor y EF6 siguen la descripción del perfil y la práctica 4.5. La referencia a ASP.NET Core del marco teórico es bibliográfica.
- Cada producto pertenece a una categoría. `Productos.CategoriaId` es una clave foránea e indexada. El esquema conserva precio en `DECIMAL(12,2)` y stock como entero.
- La talla es un campo descriptivo opcional por registro. El stock pertenece al registro completo. El precio se presenta en USD como en la práctica 4.5.
- El catálogo es la pantalla de consulta del CRUD. Los formularios son de creación, edición y confirmación de eliminación.
- La encuesta forma parte de la investigación académica. El equipo utilizará los canales externos que indica el perfil.

Para la defensa, abre primero `ProductosController.Index` y sigue una búsqueda hasta la vista. Después cambia el stock desde el formulario de edición y explica cómo las validaciones, `SaveChanges()` y la redirección mantienen el listado actualizado.
