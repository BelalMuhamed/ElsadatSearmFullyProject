# One-off script for F1-S2 (fresh baseline migration, decision D0). Delete after F1-S2 is done.
# Run it ONCE instead of sync-and-build.ps1: right-click > "Run with PowerShell".
#
# What it does:
#   1. git pull origin fix/backend (stops if the backend has local changes)
#   2. points ConnectionStrings:con (user secrets) to a NEW database name; the old database is left untouched
#   3. dotnet ef migrations add InitialCreate
#   4. dotnet ef migrations script  -> _out\InitialCreate.sql (for review)
#   5. dotnet ef database update     -> creates the new database with the schema (seed runs later, when the API starts)
#   6. moves the generated migration files to _out\Migrations (git-ignored) so the working tree stays clean;
#      Claude reads them from there, reviews them and pushes them.
# Output: make-baseline.log next to this script. The connection string itself is never written to the log.
param([string]$NewDatabaseName = 'SadatSearmErp')

$root = $PSScriptRoot
$backend = Join-Path $root 'New folder (5)\AlSadat-Seram\AlSadat-Seram-Project\AlSadat-Seram'
$api = Join-Path $backend 'AlSadat-Seram.Api\AlSadat-Seram.Api.csproj'
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

Set-Location $backend
Invoke-Step 'dotnet tool restore' { dotnet tool restore }

# 2. new database name (the value is not logged: it may contain a password)
Write-Log '== user secrets: ConnectionStrings:con'
$prefix = 'ConnectionStrings:con = '
$line = dotnet user-secrets list --project $api | Where-Object { $_.StartsWith($prefix) } | Select-Object -First 1
if (-not $line) { Stop-Script 'ConnectionStrings:con was not found in the API user secrets.' }

$builder = New-Object System.Data.Common.DbConnectionStringBuilder
$builder.ConnectionString = $line.Substring($prefix.Length)
$key = @('Database', 'Initial Catalog') | Where-Object { $builder.ContainsKey($_) } | Select-Object -First 1
if (-not $key) { Stop-Script 'the connection string has no Database / Initial Catalog.' }

$oldName = $builder[$key]
if ($oldName -ne $NewDatabaseName) {
    $builder[$key] = $NewDatabaseName
    dotnet user-secrets set 'ConnectionStrings:con' $builder.ConnectionString --project $api | Out-Null
    if ($LASTEXITCODE -ne 0) { Stop-Script 'could not update ConnectionStrings:con in user secrets.' }
}
Write-Log "database: $oldName -> $NewDatabaseName (the old database is not touched)"

# 3-5. baseline migration
if ((Test-Path $migrations) -and (Get-ChildItem $migrations -File)) {
    Stop-Script 'Infrastructure\Migrations is not empty after the pull.'
}
New-Item -ItemType Directory -Force -Path (Join-Path $out 'Migrations') | Out-Null

Invoke-Step 'migrations add InitialCreate' { dotnet ef migrations add InitialCreate -p Infrastructure -s AlSadat-Seram.Api }
Invoke-Step 'migrations script' { dotnet ef migrations script -p Infrastructure -s AlSadat-Seram.Api -o (Join-Path $out 'InitialCreate.sql') --no-build }
Invoke-Step 'database update' { dotnet ef database update -p Infrastructure -s AlSadat-Seram.Api --no-build }

# 6. keep the working tree clean: Claude pushes these files after review
Move-Item -Path (Join-Path $migrations '*') -Destination (Join-Path $out 'Migrations') -Force
Write-Log ('generated files: ' + ((Get-ChildItem (Join-Path $out 'Migrations') -File).Name -join ', '))

Set-Location $root
Write-Log 'Done. Tell Claude: make-baseline finished.'
Read-Host 'Press Enter to close'
