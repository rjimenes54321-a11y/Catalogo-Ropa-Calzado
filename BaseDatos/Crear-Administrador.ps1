# Ejecutar en Windows PowerShell 5.1 después de compilar y aplicar 04_Ventas.sql.
param([string]$Instancia = '.\SQLEXPRESS')
$ErrorActionPreference = 'Stop'

$ensamblado = Join-Path $PSScriptRoot '..\CatalogoRopaCalzado.Web\bin\CatalogoRopaCalzado.Web.dll'
if (!(Test-Path -LiteralPath $ensamblado)) { throw 'Compila el proyecto antes de ejecutar este script.' }
Add-Type -Path $ensamblado
Add-Type -AssemblyName System.Data

$nombre = (Read-Host 'Nombre completo del primer administrador').Trim()
$correo = (Read-Host 'Correo electrónico').Trim().ToLowerInvariant()
if (!$nombre -or $nombre.Length -gt 120 -or $correo.Length -gt 254) {
    throw 'Verifica el nombre y el correo.'
}
try { $validado = [System.Net.Mail.MailAddress]::new($correo) }
catch { throw 'El correo no tiene un formato válido.' }
if ($validado.Address -ne $correo) { throw 'Introduce únicamente la dirección de correo.' }

$claveSegura = Read-Host 'Contraseña (mínimo 8 caracteres, con mayúscula, minúscula y número)' -AsSecureString
$puntero = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($claveSegura)
try {
    $clave = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($puntero)
    if (![CatalogoRopaCalzado.Data.Claves]::EsValida($clave)) { throw 'La contraseña no cumple los requisitos.' }
    $hash = [CatalogoRopaCalzado.Data.Claves]::Crear($clave)
}
finally {
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($puntero)
    $claveSegura.Dispose()
    $clave = $null
}

$cadena = [System.Data.SqlClient.SqlConnectionStringBuilder]::new()
$cadena['Data Source'] = $Instancia
$cadena['Initial Catalog'] = 'CatalogoRopaCalzado'
$cadena['Integrated Security'] = $true
$conexion = [System.Data.SqlClient.SqlConnection]::new($cadena.ConnectionString)
try {
    $conexion.Open()
    $orden = $conexion.CreateCommand()
    $orden.CommandText = @'
INSERT INTO dbo.Usuarios (Nombre, Correo, ClaveHash, Rol)
SELECT @nombre, @correo, @hash, N'Administrador'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Usuarios WITH (UPDLOCK, HOLDLOCK) WHERE Rol = N'Administrador');
'@
    $orden.Parameters.Add('@nombre', [Data.SqlDbType]::NVarChar, 120).Value = $nombre
    $orden.Parameters.Add('@correo', [Data.SqlDbType]::NVarChar, 254).Value = $correo
    $orden.Parameters.Add('@hash', [Data.SqlDbType]::NVarChar, 200).Value = $hash
    if ($orden.ExecuteNonQuery() -ne 1) { throw 'Ya existe un administrador. Usa el módulo Usuarios para crear otra cuenta.' }
    Write-Output 'Administrador creado. Inicia sesión desde el sitio.'
}
finally {
    $conexion.Dispose()
}
