param(
    [switch]$DryRun,
    [string]$Key
)

$cliPath = Join-Path $PSScriptRoot "Bhbk.Cli.Identity\bin\Debug\net8.0\Bhbk.Cli.Identity.exe"

if (-not (Test-Path $cliPath)) {
    Write-Host "CLI not found at: $cliPath" -ForegroundColor Red
    Write-Host "Run build.ps1 first to compile the CLI." -ForegroundColor Yellow
    exit 1
}

$configFiles = @(
    "Bhbk.WebApi.Identity.Sts\appsettings.json",
    "Bhbk.WebApi.Identity.Admin\appsettings.json",
    "Bhbk.WebApi.Identity.User\appsettings.json",
    "Bhbk.WebApi.Alert\appsettings.json",
    "Bhbk.Cli.Identity\clisettings.json",
    "Bhbk.Cli.Alert\clisettings.json"
)

foreach ($file in $configFiles) {
    $fullPath = Join-Path $PSScriptRoot $file

    if (Test-Path $fullPath) {
        $args = @("decrypt-config", "-f", $fullPath)

        if ($Key) { $args += @("-k", $Key) }
        if ($DryRun) { $args += "-d" }

        & $cliPath $args
    } else {
        Write-Host "Skipping (not found): $file" -ForegroundColor Yellow
    }
}
