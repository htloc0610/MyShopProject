# Run All Tests - Output to console and log file
. "$PSScriptRoot\config.ps1"

$logFile = "$PSScriptRoot\test_results.log"
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

# Start transcript to capture all output
Start-Transcript -Path $logFile -Force | Out-Null

Write-Host "`n============================================="
Write-Host "   MYSHOP API TESTS"
Write-Host "   Server: $global:BaseUrl"
Write-Host "   Time: $timestamp"
Write-Host "============================================="

$tests = @("test_auth", "test_categories", "test_customers", "test_discounts", "test_products", "test_orders", "test_dashboard", "test_reports")

foreach ($name in $tests) {
    $path = "$PSScriptRoot\$name.ps1"
    if (Test-Path $path) { & $path }
}

Write-Host "`n============================================="
Write-Host "   ALL TESTS COMPLETED"
Write-Host "============================================="

Stop-Transcript | Out-Null

Write-Host "`nLog saved to: $logFile" -ForegroundColor Cyan
