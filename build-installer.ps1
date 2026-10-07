$ErrorActionPreference = "Stop"

$projectDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$publishDirectory = Join-Path $projectDirectory "bin\Release\net8.0-windows\win-x64\publish"
$projectFile = [xml](Get-Content (Join-Path $projectDirectory "QuickBrowse.csproj"))
$version = $projectFile.Project.PropertyGroup.Version
if (-not $version) {
    throw "Не удалось прочитать версию приложения из QuickBrowse.csproj."
}
$setupCommand = Get-Command "ISCC.exe" -ErrorAction SilentlyContinue
$setupCompilerPath = if ($setupCommand) { $setupCommand.Source } else { $null }

if (-not $setupCompilerPath) {
    $knownCompilerPaths = @(
        (Join-Path ${env:ProgramFiles(x86)} "Inno Setup 6\ISCC.exe"),
        (Join-Path $env:ProgramFiles "Inno Setup 6\ISCC.exe")
    )
    $setupCompilerPath = $knownCompilerPaths | Where-Object { Test-Path $_ } | Select-Object -First 1
}

if (-not $setupCompilerPath) {
    throw "Не найден Inno Setup Compiler (ISCC.exe). Установите Inno Setup 6 и повторите запуск."
}

Push-Location $projectDirectory
try {
    & dotnet publish ".\QuickBrowse.csproj" -c Release -r win-x64 --self-contained true
    if ($LASTEXITCODE -ne 0) {
        throw "Публикация браузера завершилась с кодом $LASTEXITCODE. Закройте QuickBrowse и повторите."
    }

    if (-not (Test-Path (Join-Path $publishDirectory "QuickBrowse.exe"))) {
        throw "Не найден QuickBrowse.exe в папке публикации: $publishDirectory"
    }

    & $setupCompilerPath "/DMyAppVersion=`"$version`"" ".\QuickBrowse.iss"
    if ($LASTEXITCODE -ne 0) {
        throw "Сборка установщика завершилась с кодом $LASTEXITCODE."
    }

    $installerPath = Join-Path $projectDirectory "installer\QuickBrowse-Setup.exe"
    Write-Host "Установщик создан: $installerPath"
}
finally {
    Pop-Location
}
