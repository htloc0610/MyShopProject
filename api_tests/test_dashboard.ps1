# Test Dashboard API
. "$PSScriptRoot\_common.ps1"

Write-Host "`n=== DASHBOARD API TEST ===" -ForegroundColor Magenta
$Auth = Get-AuthToken

Test-Endpoint -Name "GET /api/dashboard/summary" -Method "GET" -Url "$BaseUrl/api/dashboard/summary" -Headers $Auth
Test-Endpoint -Name "GET /api/dashboard/low-stock" -Method "GET" -Url "$BaseUrl/api/dashboard/low-stock" -Headers $Auth
Test-Endpoint -Name "GET /api/dashboard/top-selling" -Method "GET" -Url "$BaseUrl/api/dashboard/top-selling" -Headers $Auth
Test-Endpoint -Name "GET /api/dashboard/recent-orders" -Method "GET" -Url "$BaseUrl/api/dashboard/recent-orders" -Headers $Auth
Test-Endpoint -Name "GET /api/dashboard/revenue-month" -Method "GET" -Url "$BaseUrl/api/dashboard/revenue-month" -Headers $Auth

Show-Summary

