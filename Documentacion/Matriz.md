# Matriz de cumplimiento

El **perfil** es `Trabajo20_Grupo2Fase5.docx`. El **PDF** es `Tema de trabajo .pdf`. La práctica **4.5** es `4.5 Filtros LINQ.docx`. Los resultados de ejecución y las limitaciones se detallan en [Pruebas](Pruebas.md).

| Requisito | Fuente | Archivo o módulo | Estado | Prueba |
|---|---|---|---|---|
| Aplicación web de ropa y calzado con C#, ASP.NET MVC, Visual Studio y SQL Server | Perfil, objetivos y base teórica. PDF, pp. 1–4. | Solución, proyecto web, `Web.config` | Implementado | Compilación y conexión verificadas. |
| MVC, Razor, EF6, DbContext y DbSet por entidad | Perfil, §§3.1 y 3.3. PDF, p. 5. | `Models`, `Views`, `Controllers`, `Data/CatalogoContext.cs` | Implementado | Compilación del proyecto y recorrido HTTP. |
| Persistencia de producto, precio y existencias con integridad entre productos y categorías | Perfil, objetivos y §3.2. PDF, pp. 3 y 5. | `BaseDatos/01_Esquema.sql` y `02_DatosEjemplo.sql` | Implementado | SQL verificó inserción, lectura, actualización y restricciones. |
| Crear productos | Perfil, objetivos y tabla 3. PDF, p. 3. | `ProductosController.Crear` y `Crear.cshtml` | Implementado | Alta cubierta por pruebas SQL y HTTP. |
| Leer catálogo con precio y existencias | Perfil, objetivos y tabla 3. PDF, p. 3. | `ProductosController.Index` e `Index.cshtml` | Implementado | Consulta de datos conocidos en SQL y HTTP. |
| Actualizar precio, existencias y otros datos | Perfil, objetivos, §3.4 y tabla 3. PDF, p. 3. | `ProductosController.Editar` y `Editar.cshtml` | Implementado | Edición y stock cero cubiertos por SQL y HTTP. |
| Eliminar productos | Perfil, objetivos y tabla 3. PDF, p. 3. | `ProductosController.ConfirmarEliminacion` y `Eliminar.cshtml` | Implementado | Eliminación SQL y flujo web descritos en el reporte. |
| Buscar por nombre o categoría | Perfil, objetivos y tabla 3. PDF, p. 3. Práctica 4.5 como guía técnica. | LINQ en `ProductosController.Index` y formulario de filtros | Implementado | Nombre, categoría y combinación cubiertos por HTTP. |
| Definir el tratamiento de categorías y tallas | Perfil, tabla 4. | `Producto.cs`, `01_Esquema.sql` y `_Formulario.cshtml` | Implementado | Talla descriptiva en datos sintéticos y formulario. |
| Validar datos y presentar formularios sencillos y adaptables | Perfil, problema y objetivo general. PDF, p. 2. Guía 4.0. Solicitud de implementación. | `ProductoFormulario.cs`, controlador, vistas y CSS | Implementado | Validaciones principales cubiertas por HTTP y restricciones por SQL. Revisión visual en navegador pendiente. |
| Verificar etapas con datos sintéticos y registrar resultados mediante lista de cotejo | Perfil, §§4.4–4.6. PDF, p. 3. | `03_Verificar.sql`, `Pruebas/Verificar-Web.ps1` y reporte | Implementado | Resultados y alcance en el reporte. |
| Cuestionario de 12 preguntas, revisión del docente y piloto con tres personas | Perfil, §§4.4–4.5 y tabla 2. | `Investigacion.md` | Borrador preparado. Revisión y piloto pendientes. | Revisión estructural de 12 ítems en tres bloques. |
| Encuesta a 30 participantes durante dos semanas, con participación voluntaria y sin datos personales | Perfil, §§4.3–4.5. PDF, pp. 4–5, con población anterior. | Procedimiento en `Investigacion.md` | Pendiente de trabajo de campo | Sin respuestas recolectadas en esta entrega. |
| Tabulación, frecuencias, porcentajes, gráficos e informe final en APA 7 | Perfil, §§4.6 y 5. PDF, pp. 1–2 para el informe. | Plan de análisis en `Investigacion.md` y resultados funcionales en `Pruebas.md` | Evidencia técnica disponible. Análisis de encuesta e informe académico integrado pendientes. | Depende de la recolección real y la revisión académica. |
| Verificar cada fase y seguir el cronograma académico | Perfil, metodología y tabla 4. | Fases y dependencias en `Investigacion.md`; evidencia en `Pruebas.md` | Desarrollo y comprobación técnica ejecutados. Cronograma de campo pendiente. | La aplicación, el análisis y la entrega institucional dependen del equipo y el docente. |
