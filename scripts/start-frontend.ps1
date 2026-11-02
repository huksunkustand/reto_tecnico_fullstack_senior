param([int]$Port = 5173)
$ErrorActionPreference = 'Stop'
Set-Location (Join-Path (Split-Path $PSScriptRoot -Parent) 'frontend')
$major = [int]((node --version).TrimStart('v').Split('.')[0])
if ($major -lt 22) {
    npm exec --yes --package=node@22 -- node node_modules/vite/bin/vite.js --host localhost --port $Port
} else {
    npm run dev -- --host localhost --port $Port
}
exit $LASTEXITCODE