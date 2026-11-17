# Pulls the latest fix/backend, then builds and tests the backend.
# Output goes to pull.log, build.log and test.log next to this script (git-ignored),
# so Claude can read the results from this folder and fix any errors.
# Run: right-click > "Run with PowerShell", or in PowerShell: .\sync-and-build.ps1

$root = $PSScriptRoot
$backend = Join-Path $root 'New folder (5)\AlSadat-Seram\AlSadat-Seram-Project\AlSadat-Seram'

Set-Location $root
git pull origin fix/backend *> (Join-Path $root 'pull.log')

Set-Location $backend
dotnet build AlSadat-Seram.slnx *> (Join-Path $root 'build.log')
# The projects target net9.0 until F1 moves them to .NET 10. If only the .NET 10
# runtime is installed, let the test host roll forward to it.
$env:DOTNET_ROLL_FORWARD = 'Major'
dotnet test AlSadat-Seram.slnx --no-build *> (Join-Path $root 'test.log')
Remove-Item Env:\DOTNET_ROLL_FORWARD

Set-Location $root
Write-Host 'Done. See pull.log, build.log, test.log'
