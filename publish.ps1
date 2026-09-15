param(
    [Parameter(Mandatory = $true)]
    [string]$PublishDir,
)

$ErrorActionPreference = "Stop"

# Publish GalleryViewer(API)
dotnet publish "$PSScriptRoot\GalleryViewer\GalleryViewer.csproj" `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
	-p:ExcludeDevelopmentSettings=true `
    -o $PublishDir

if ($LASTEXITCODE -ne 0) {
    throw "GalleryViewer publish failed."
}

# Publish Tools
dotnet publish "$PSScriptRoot\Tools\Tools.csproj" `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -o $PublishDir

if ($LASTEXITCODE -ne 0) {
    throw "Tools publish failed."
}

Write-Host ""
Write-Host "Publish completed:"
Get-ChildItem $PublishDir