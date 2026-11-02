param([int]$Port = 5080)
$ErrorActionPreference = 'Stop'
Set-Location (Split-Path $PSScriptRoot -Parent)
$env:ASPNETCORE_ENVIRONMENT = 'Development'
if (-not $env:Jwt__Key) {
    $bytes = New-Object byte[] 48
    $random = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $random.GetBytes($bytes)
    $random.Dispose()
    $env:Jwt__Key = [Convert]::ToBase64String($bytes)
}
if (-not $env:Seed__Email) { $env:Seed__Email = 'admin@atlantic.local' }
if (-not $env:Seed__Password) { $env:Seed__Password = 'Reto2026!Local' }
dotnet run --project backend/src/Pedidos.Api --no-launch-profile -- --urls "http://localhost:$Port"
exit $LASTEXITCODE