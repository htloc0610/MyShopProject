# Test Reports API
. "$PSScriptRoot\_common.ps1"

Write-Host "`n=== REPORTS API TEST ===" -ForegroundColor Magenta
$Auth = Get-AuthToken

$today = Get-Date -Format "yyyy-MM-dd"

Test-Endpoint -Name "GET /api/reports/product-sales-summary" -Method "GET" -Url "$BaseUrl/api/reports/product-sales-summary?to=$today" -Headers $Auth
Test-Endpoint -Name "GET /api/reports/product-revenue-profit-summary" -Method "GET" -Url "$BaseUrl/api/reports/product-revenue-profit-summary?to=$today" -Headers $Auth
Test-Endpoint -Name "GET /api/reports/sales-quantity-series" -Method "GET" -Url "$BaseUrl/api/reports/sales-quantity-series?to=$today&groupBy=day" -Headers $Auth
Test-Endpoint -Name "GET /api/reports/revenue-profit-series" -Method "GET" -Url "$BaseUrl/api/reports/revenue-profit-series?to=$today&groupBy=day" -Headers $Auth

Show-Summary

