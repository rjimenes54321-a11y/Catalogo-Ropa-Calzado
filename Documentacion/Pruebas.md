# Reporte y lista de cotejo

Verificación local del 22 de noviembre de 2026. Se restauraron Microsoft ASP.NET MVC 5.2.9 y Entity Framework 6.5.1 desde NuGet. MSBuild de Visual Studio compiló la solución .NET Framework 4.8 con código de salida 0. Se creó la base en SQL Server Express con autenticación de Windows y se insertaron dos categorías y seis productos de ejemplo. IIS Express sirvió la aplicación y la prueba web ejecutó solicitudes reales contra la base local.

Cada fila se registró como cumple cuando el script encontró el resultado esperado. Para repetir la verificación, usa los pasos de [instalación y pruebas](../README.md).

## SQL Server

| Caso | Resultado esperado y observado | Estado |
|---|---|---|
| Inserción | El producto temporal se guarda con su identificador. | Cumple |
| Consulta | El producto aparece con categoría Ropa, precio 12.34 y stock 6. | Cumple |
| Actualización | El valor guardado cambia a 15.25 y stock 0. | Cumple |
| Restricción de existencias | SQL Server rechaza una existencia negativa. | Cumple |
| Restricción de precio | SQL Server rechaza un precio negativo. | Cumple |
| Integridad referencial | SQL Server rechaza una categoría inexistente. | Cumple |
| Eliminación | El identificador deja de existir. | Cumple |

Resultado de `03_Verificar.sql`: **7 de 7 (100 %)**. El script revirtió la transacción de prueba.

## Aplicación web

| Caso | Resultado esperado y observado | Estado |
|---|---|---|
| Consulta inicial | Camiseta de algodón muestra precio 14.95 y stock 18. | Cumple |
| Categoría en el formulario | Ropa tiene una opción disponible con identificador. | Cumple |
| Filtro por categoría | Se incluye la camiseta y se excluyen los tenis. | Cumple |
| Stock negativo | El formulario devuelve un mensaje de validación. | Cumple |
| Precisión de precio | El formulario rechaza 12.345. | Cumple |
| Categoría inexistente | El formulario rechaza el identificador no registrado. | Cumple |
| Alta | El producto temporal aparece con un identificador navegable. | Cumple |
| Búsqueda por nombre | Devuelve un solo resultado con el producto temporal. | Cumple |
| Búsqueda combinada | Devuelve el mismo producto al agregar su categoría. | Cumple |
| Edición | El cambio de nombre y precio se refleja y el stock cero aparece como agotado. | Cumple |
| Confirmación | El GET de eliminación presenta el botón para confirmar. | Cumple |
| Eliminación confirmada | El POST devuelve el mensaje de eliminación. | Cumple |
| Consulta posterior | El producto eliminado ya no aparece y se informa que no hay coincidencias. | Cumple |

Resultado final de `Verificar-Web.ps1`: **13 de 13 (100 %)**. Las ejecuciones iniciales detectaron una diferencia entre texto HTML y texto decodificado en la prueba y un problema de UTF-8 en las vistas. Se corrigieron ambos y se volvió a ejecutar el recorrido completo.

## Alcance de la evidencia

La verificación web fue HTTP contra IIS Express. Queda pendiente inspeccionar visualmente la interfaz en un navegador en resoluciones de escritorio y móvil. También quedan sin prueba específica los errores de conexión, las solicitudes con identificador inexistente y los cambios concurrentes. El piloto, la encuesta y el análisis de campo requieren participantes reales y revisión docente.
