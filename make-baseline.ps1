# One-off script for F1-S2 (fresh baseline migration, decision D0). Delete after F1-S2 is done.
# Run it ONCE instead of sync-and-build.ps1: right-click > "Run with PowerShell".
#
# What it does:
#   1. git pull origin fix/backend (stops if the backend has local changes)
#   2. points ConnectionStrings:con to a NEW database name, written to the API user secrets (they override the
#      local, git-ignored appsettings.json); the old database and appsettings.json are left untouched
#   3. dotnet ef migrations add InitialCreate
#   4. dotnet ef migrations script  -> _out\InitialCreate.sql (for review)
#   5. dotnet ef database update     -> creates the new database with the schema (seed runs later, when the API starts)
#   6. moves the generated migration files to _out\Migrations (git-ignored) so the working tree stays clean;
#      Claude reads them from there, reviews them and pushes them.
# Output: make-baseline.log next to this script. The connection string itself is never written to the log.
param([string]$NewDatabaseName = 'SadatSearmErp')

$root = $PSScriptRoot
$backend = Join-Path $root 'New folder (5)\AlSadat-Seram\AlSadat-Seram-Project\AlSadat-Seram'
$apiDir = Join-Path $backend 'AlSadat-Seram.Api'
$api = Join-Path $apiDir 'AlSadat-Seram.Api.csproj'
$infra = Join-Path $backend 'Infrastructure\Infrastructure.csproj'
$migrations = Join-Path $backend 'Infrastructure\Migrations'
$out = Join-Path $root '_out'
$log = Join-Path $root 'make-baseline.log'

function Write-Log([string]$text) {
    $text | Out-File -FilePath $log -Append -Encoding utf8
    Write-Host $text
}

function Stop-Script([string]$reason) {
    Write-Log "STOPPED: $reason"
    Read-Host 'Press Enter to close'
    exit 1
}

function Invoke-Step([string]$title, [scriptblock]$command) {
    Write-Log "== $title"
    $output = & $command 2>&1 | Out-String
    Write-Log $output
    if ($LASTEXITCODE -ne 0) { Stop-Script "$title failed (exit code $LASTEXITCODE)" }
}

Remove-Item $log -ErrorAction SilentlyContinue
Remove-Item $out -Recurse -Force -ErrorAction SilentlyContinue
Set-Location $root

# 1. pull
$dirty = git status --porcelain -- 'New folder (5)'
if ($dirty) { Stop-Script ("the backend has local changes:`n" + ($dirty | Out-String)) }
Invoke-Step 'git pull' { git pull origin fix/backend }

# dotnet-ef is a local tool whose manifest lives in AlSadat-Seram.Api, so every dotnet tool / ef command runs from there
Set-Location $apiDir
Invoke-Step 'dotnet tool restore' { dotnet tool restore }

# 2. new database name (values are never logged: they may contain a password)
Write-Log '== ConnectionStrings:con'
$current = $null
$source = $null
$secretLines = @(dotnet user-secrets list --project $api)
$line = $secretLines | Where-Object { $_ -match '^ConnectionStrings:con = ' } | Select-Object -First 1
if ($line) {
    $current = $line -replace '^ConnectionStrings:con = ', ''
    $source = 'user secrets'
} else {
    $appSettings = Join-Path $apiDir 'appsettings.json'
    if (Test-Path $appSettings) {
        $json = Get-Content $appSettings -Raw | ConvertFrom-Json
        if ($json.ConnectionStrings -and $json.ConnectionStrings.con) {
            $current = $json.ConnectionStrings.con
            $source = 'appsettings.json (local, git-ignored)'
        }
    }
}
if (-not $current) {
    $keys = ($secretLines | ForEach-Object { ($_ -split ' = ', 2)[0] }) -join ', '
    Stop-Script "ConnectionStrings:con was not found in user secrets nor in AlSadat-Seram.Api\appsettings.json. User secret keys found: $keys"
}

$builder = New-Object System.Data.Common.DbConnectionStringBuilder
$builder.ConnectionString = $current
$key = @('Database', 'Initial Catalog') | Where-Object { $builder.ContainsKey($_) } | Select-Object -First 1
if (-not $key) { Stop-Script 'the connection string has no Database / Initial Catalog.' }

$oldName = $builder[$key]
$builder[$key] = $NewDatabaseName
dotnet user-secrets set 'ConnectionStrings:con' $builder.ConnectionString --project $api | Out-Null
if ($LASTEXITCODE -ne 0) { Stop-Script 'could not write ConnectionStrings:con to user secrets.' }
Write-Log "read from: $source"
Write-Log "database: $oldName -> $NewDatabaseName (written to user secrets; the old database and appsettings.json are not touched)"

# 3-5. baseline migration
if ((Test-Path $migrations) -and (Get-ChildItem $migrations -File)) {
    Stop-Script 'Infrastructure\Migrations is not empty after the pull.'
}
New-Item -ItemType Directory -Force -Path (Join-Path $out 'Migrations') | Out-Null

Invoke-Step 'migrations add InitialCreate' { dotnet ef migrations add InitialCreate -p $infra -s $api }
Invoke-Step 'migrations script' { dotnet ef migrations script -p $infra -s $api -o (Join-Path $out 'InitialCreate.sql') }
Invoke-Step 'database update' { dotnet ef database update -p $infra -s $api --no-build }

# 6. keep the working tree clean: Claude pushes these files after review
Move-Item -Path (Join-Path $migrations '*') -Destination (Join-Path $out 'Migrations') -Force
Write-Log ('generated files: ' + ((Get-ChildItem (Join-Path $out 'Migrations') -File).Name -join ', '))

Set-Location $root
Write-Log 'Done. Tell Claude: make-baseline finished.'
Read-Host 'Press Enter to close'
