#!/usr/bin/env pwsh
# Bookify.Client - Robust Netlify Deploy Script
# Optimized for .NET 10 / Blazor WASM on static hosts

$publishDir = "$PSScriptRoot\output\wwwroot"
$frameworkDir = "$publishDir\_framework"
$indexHtml = "$publishDir\index.html"
$appsettingsTemplate = "$PSScriptRoot\wwwroot\appsettings.json"

# 1. Environment Configuration
$siteId = $env:NETLIFY_SITE_ID 
if (-not $siteId) { $siteId = "967a94f3-a504-4470-8008-18bdb00f1f11" }

$authToken = $env:NETLIFY_AUTH_TOKEN
$apiBaseUrl = $env:API_BASE_URL
if (-not $apiBaseUrl) { $apiBaseUrl = "https://bookify-api-4oeq.onrender.com/" }

Write-Host "==> Preparing Configuration..." -ForegroundColor Cyan

# Inject environment variables into source appsettings.json
if (Test-Path $appsettingsTemplate) {
    Write-Host "   Injecting API_BASE_URL: $apiBaseUrl" -ForegroundColor Gray
    $content = Get-Content $appsettingsTemplate -Raw
    $patched = $content -replace '\{\{API_BASE_URL\}\}', $apiBaseUrl
    Set-Content $appsettingsTemplate $patched -NoNewline
}

# 2. Build & Publish
Write-Host "==> Publishing Bookify.Client (Release)..." -ForegroundColor Cyan
if (Test-Path "$PSScriptRoot\output") { Remove-Item "$PSScriptRoot\output" -Recurse -Force }

dotnet publish "$PSScriptRoot\Bookify.Client.csproj" -c Release -o "$PSScriptRoot\output"

if ($LASTEXITCODE -ne 0) { Write-Error "Publish failed!"; exit 1 }

# 3. Robust Asset Patching (Crucial for static hosts)
Write-Host "==> Performing Robust Asset Patching..." -ForegroundColor Cyan

# Find the fingerprinted blazor.webassembly.js
$blazorJsFingered = Get-ChildItem $frameworkDir -Filter "blazor.webassembly.*.js" |
    Where-Object { $_.Name -notlike "*.br" -and $_.Name -notlike "*.gz" } |
    Select-Object -First 1

if ($blazorJsFingered) {
    Write-Host "   Found fingerprinted Blazor JS: $($blazorJsFingered.Name)" -ForegroundColor Green
    
    # Update index.html to use the fingerprinted version (manual backup for automatic failure)
    if (Test-Path $indexHtml) {
        $content = Get-Content $indexHtml -Raw
        $patched = $content -replace '_framework/blazor\.webassembly\.js', "_framework/$($blazorJsFingered.Name)"
        if ($content -ne $patched) {
            Set-Content $indexHtml $patched -NoNewline
            Write-Host "   Patched index.html with fingerprinted Blazor JS." -ForegroundColor Green
        }
    }
    
    # Create un-fingerprinted stubs (copies) for compatibility
    $stubPatterns = @(
        @{ Pattern = "blazor.webassembly.*.js"; Target = "blazor.webassembly.js" },
        @{ Pattern = "dotnet.*.js";            Target = "dotnet.js" },
        @{ Pattern = "dotnet.native.*.js";     Target = "dotnet.native.js" },
        @{ Pattern = "dotnet.native.*.wasm";   Target = "dotnet.native.wasm" },
        @{ Pattern = "dotnet.runtime.*.js";    Target = "dotnet.runtime.js" }
    )
    
    foreach ($entry in $stubPatterns) {
        $found = Get-ChildItem $frameworkDir -Filter $entry.Pattern | 
                 Where-Object { $_.Name -notlike "*.br" -and $_.Name -notlike "*.gz" -and $_.Name -ne $entry.Target } |
                 Select-Object -First 1
                 
        if ($found) {
            $destPath = Join-Path $frameworkDir $entry.Target
            Copy-Item $found.FullName $destPath -Force
            Write-Host "   Created stub: $($entry.Target) (from $($found.Name))" -ForegroundColor Green
        }
    }
} else {
    Write-Warning "   No fingerprinted blazor.webassembly.*.js found. Skipping stubs."
}

# 4. Deploy to Netlify
Write-Host "==> Deploying to Netlify..." -ForegroundColor Cyan

$netlifyCmd = "npx netlify deploy --prod --dir=`"$publishDir`" --site=$siteId"
if ($authToken) { $netlifyCmd += " --auth=$authToken" }

cmd /c $netlifyCmd

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n==> Deploy successful! Check https://bookify-dev.netlify.app" -ForegroundColor Green
} else {
    Write-Error "`n==> Netlify deployment failed."
    exit 1
}
