param([string]$Url = 'http://localhost:51244')
$ErrorActionPreference = 'Stop'
$Url = $Url.TrimEnd('/')
$sesion = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$nombre = 'Prueba web ' + [Guid]::NewGuid().ToString('N')
$nombreEditado = "$nombre editado"
$id = $null
$categoria = $null

function Consultar($ruta) {
    Invoke-WebRequest -Uri "$Url$ruta" -WebSession $sesion -UseBasicParsing
}

function Enviar($ruta, $pagina, $campos) {
    $token = [regex]::Match($pagina.Content, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"')
    if (!$token.Success) { throw 'No se encontró el token del formulario.' }
    $campos['__RequestVerificationToken'] = $token.Groups[1].Value
    Invoke-WebRequest -Uri "$Url$ruta" -Method Post -Body $campos -WebSession $sesion -UseBasicParsing
}

function Comprobar($condicion, $caso) {
    if (!$condicion) { throw "FALLO $caso" }
    Write-Output "PASS $caso"
}

function ObtenerId($pagina, $producto) {
    $patron = 'href="[^"]*/Productos/Editar/(\d+)"[^>]*aria-label="Editar ' +
        [regex]::Escape($producto) + '"'
    return [regex]::Match($pagina.Content, $patron, 'IgnoreCase').Groups[1].Value
}

try {
    $pagina = Consultar '/'
    $html = [System.Net.WebUtility]::HtmlDecode($pagina.Content)
    $fila = [regex]::Match($html, '(?s)<tr>\s*<th scope="row">Camiseta de algodón</th>.*?</tr>').Value
    Comprobar ($fila -match '14[.,]95' -and $fila -match '>18<') 'consulta de precio y stock de la camiseta'

    $formulario = Consultar '/Productos/Crear'
    $categoria = [regex]::Match($formulario.Content, '<option value="(\d+)">Ropa</option>').Groups[1].Value
    Comprobar (![string]::IsNullOrEmpty($categoria)) 'categoría disponible en el formulario'

    $pagina = Consultar "/Productos?CategoriaId=$categoria"
    Comprobar ($pagina.Content -match 'Camiseta' -and $pagina.Content -notmatch 'Tenis urbanos') 'filtro solo por categoría'

    $nombreConsulta = [uri]::EscapeDataString($nombre)
    $campos = @{ Nombre = $nombre; CategoriaId = $categoria; Talla = 'M'; Precio = '12.34'; Stock = '-1' }
    $pagina = Enviar '/Productos/Crear' $formulario $campos
    Comprobar ($pagina.Content -match 'entero no negativo') 'validación de stock negativo'
    $campos.Stock = '6'
    $campos.Precio = '12.345'
    $pagina = Enviar '/Productos/Crear' $pagina $campos
    Comprobar ($pagina.Content -match 'hasta dos decimales') 'validación de decimales del precio'
    $campos.Precio = '12.34'
    $campos.CategoriaId = '2147483647'
    $pagina = Enviar '/Productos/Crear' $pagina $campos
    Comprobar ($pagina.Content -match 'seleccionada no existe') 'validación de categoría inexistente'

    $campos.CategoriaId = $categoria
    $pagina = Enviar '/Productos/Crear' $pagina $campos
    $id = ObtenerId $pagina $nombre
    Comprobar (![string]::IsNullOrEmpty($id)) 'creación de producto'

    $pagina = Consultar "/Productos?Buscar=$nombreConsulta"
    Comprobar ((ObtenerId $pagina $nombre) -eq $id -and $pagina.Content -match '1 producto<') 'búsqueda solo por nombre'
    $pagina = Consultar "/Productos?Buscar=$nombreConsulta&CategoriaId=$categoria"
    Comprobar ((ObtenerId $pagina $nombre) -eq $id) 'búsqueda combinada'

    $pagina = Consultar "/Productos/Editar/$id"
    $campos.Nombre = $nombreEditado
    $campos.Precio = '15.25'
    $campos.Stock = '0'
    $pagina = Enviar "/Productos/Editar/$id" $pagina $campos
    $fila = [regex]::Match($pagina.Content, '(?s)<tr>\s*<th scope="row">' + [regex]::Escape($nombreEditado) + '</th>.*?</tr>').Value
    Comprobar ($fila -match '15[.,]25' -and $fila -match 'Agotado') 'edición de nombre, precio y stock'

    $pagina = Consultar "/Productos/Eliminar/$id"
    Comprobar ($pagina.Content -match 'Confirmar eliminación') 'confirmación de eliminación'
    $pagina = Enviar "/Productos/Eliminar/$id" $pagina @{}
    Comprobar ($pagina.Content -match 'Producto eliminado') 'eliminación confirmada'
    $pagina = Consultar "/Productos?Buscar=$nombreConsulta"
    Comprobar ($pagina.Content -match 'No hay productos') 'producto eliminado y estado sin resultados'
    $id = $null
}
finally {
    if ($id) {
        try {
            $pagina = Consultar "/Productos/Eliminar/$id"
            $null = Enviar "/Productos/Eliminar/$id" $pagina @{}
        }
        catch {
            Write-Warning "Si el producto de prueba sigue presente, elimínalo desde el catálogo. ID $id."
        }
    }
}
