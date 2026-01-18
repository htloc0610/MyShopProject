# =============================================================================
# DISCOUNTS TESTS (GET + CRUD + Domain)
# =============================================================================
. "$PSScriptRoot\config.ps1"
$BaseUrl = $global:BaseUrl; $Email = $global:Email; $Password = $global:Password
$passed = 0; $failed = 0

function Write-Pass { param($msg) Write-Host "  [PASS] $msg" -ForegroundColor Green; $script:passed++ }
function Write-Fail { param($msg) Write-Host "  [FAIL] $msg" -ForegroundColor Red; $script:failed++ }
function Write-Assert { param($cond, $pass, $fail) if ($cond) { Write-Pass $pass } else { Write-Fail $fail } }

Write-Host "`n=== DISCOUNTS TESTS ===" -ForegroundColor Magenta

# Login
$body = @{ email = $Email; password = $Password } | ConvertTo-Json
$login = Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/auth/login" -Body $body -ContentType "application/json"
$Auth = @{ Authorization = "Bearer $($login.accessToken)" }

# GET Tests
Write-Host "`n[GET]" -ForegroundColor Cyan
$list = Invoke-RestMethod -Uri "$BaseUrl/api/discounts" -Headers $Auth
Write-Assert ($list -ne $null) "GET /api/discounts OK" "Failed"
Write-Assert ($list.items -ne $null) "Response has items" "No items"
Write-Assert ($list.totalCount -ne $null) "Response has totalCount" "No totalCount"

# Pagination & filters
$page = Invoke-RestMethod -Uri "$BaseUrl/api/discounts?page=1&pageSize=5" -Headers $Auth
Write-Assert ($page.pageSize -eq 5) "Pagination works" "Failed"

$active = Invoke-RestMethod -Uri "$BaseUrl/api/discounts?status=active" -Headers $Auth
Write-Assert ($active -ne $null) "Filter status=active works" "Failed"

if ($list.items -and $list.items.Count -gt 0) {
    $id = $list.items[0].discountId; $code = $list.items[0].code
    $detail = Invoke-RestMethod -Uri "$BaseUrl/api/discounts/$id" -Headers $Auth
    Write-Assert ($detail.discountId -eq $id) "GET /api/discounts/{id} OK" "Failed"
    Write-Assert ($detail.code -ne $null) "Discount has code" "No code"
    Write-Assert ($detail.amount -ne $null) "Discount has amount" "No amount"
    
    $validate = Invoke-RestMethod -Uri "$BaseUrl/api/discounts/validate/$code" -Headers $Auth
    Write-Assert ($validate.isValid -ne $null) "Validate returns isValid" "Failed"
}

# CRUD Tests
Write-Host "`n[CRUD]" -ForegroundColor Cyan
$discId = $null
try {
    $code = "TEST$([guid]::NewGuid().ToString().Substring(0,6))"
    $body = @{ Code = $code; Amount = 15000; Description = "Test discount"; StartDate = (Get-Date).AddDays(-1).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"); EndDate = (Get-Date).AddDays(30).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"); UsageLimit = 100; IsActive = $true } | ConvertTo-Json
    $disc = Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/discounts" -Headers $Auth -Body $body -ContentType "application/json"
    $discId = $disc.discountId
    Write-Assert ($discId -ne $null) "CREATE OK (ID: $discId)" "No ID"
    Write-Assert ($disc.code -eq $code) "CREATE returns correct code" "Wrong code"
    Write-Assert ($disc.usedCount -eq 0) "New discount has usedCount=0" "Wrong usedCount"
    
    $read = Invoke-RestMethod -Uri "$BaseUrl/api/discounts/$discId" -Headers $Auth
    Write-Assert ($read.amount -eq 15000) "READ returns correct amount" "Wrong amount"
    
    $upBody = @{ DiscountId = $discId; Code = $code; Amount = 25000; Description = "Updated"; StartDate = (Get-Date).AddDays(-1).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"); EndDate = (Get-Date).AddDays(60).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"); IsActive = $true } | ConvertTo-Json
    Invoke-RestMethod -Method PUT -Uri "$BaseUrl/api/discounts/$discId" -Headers $Auth -Body $upBody -ContentType "application/json" | Out-Null
    $updated = Invoke-RestMethod -Uri "$BaseUrl/api/discounts/$discId" -Headers $Auth
    Write-Assert ($updated.amount -eq 25000) "UPDATE amount works" "Not updated"
    
    Invoke-RestMethod -Method DELETE -Uri "$BaseUrl/api/discounts/$discId" -Headers $Auth | Out-Null
    Write-Pass "DELETE OK"
    $discId = $null
} catch { Write-Fail "CRUD failed: $($_.Exception.Message)" }
finally { if ($discId) { Invoke-RestMethod -Method DELETE -Uri "$BaseUrl/api/discounts/$discId" -Headers $Auth -ErrorAction SilentlyContinue | Out-Null } }

# Domain Tests
Write-Host "`n[DOMAIN]" -ForegroundColor Cyan

# Amount = 0
try {
    $body = @{ Code = "TEST0"; Amount = 0; StartDate = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"); EndDate = (Get-Date).AddDays(1).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ") } | ConvertTo-Json
    Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/discounts" -Headers $Auth -Body $body -ContentType "application/json" -ErrorAction Stop
    Write-Fail "Amount=0 should fail"
} catch { $code = $_.Exception.Response.StatusCode.value__; Write-Assert ($code -eq 400) "Amount=0 → 400" "Wrong" }

# Negative amount
try {
    $body = @{ Code = "TESTNEG"; Amount = -1000; StartDate = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"); EndDate = (Get-Date).AddDays(1).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ") } | ConvertTo-Json
    Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/discounts" -Headers $Auth -Body $body -ContentType "application/json" -ErrorAction Stop
    Write-Fail "Negative amount should fail"
} catch { $code = $_.Exception.Response.StatusCode.value__; Write-Assert ($code -eq 400) "Negative amount → 400" "Wrong" }

# EndDate < StartDate
try {
    $body = @{ Code = "TESTDATE"; Amount = 10000; StartDate = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"); EndDate = (Get-Date).AddDays(-1).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ") } | ConvertTo-Json
    Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/discounts" -Headers $Auth -Body $body -ContentType "application/json" -ErrorAction Stop
    Write-Fail "EndDate<StartDate should fail"
} catch { $code = $_.Exception.Response.StatusCode.value__; Write-Assert ($code -eq 400) "EndDate<StartDate → 400" "Wrong" }

# Duplicate code
$dupId = $null
try {
    $dupCode = "DUP$([guid]::NewGuid().ToString().Substring(0,4))"
    $body = @{ Code = $dupCode; Amount = 10000; StartDate = (Get-Date).AddDays(-1).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"); EndDate = (Get-Date).AddDays(30).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ") } | ConvertTo-Json
    $first = Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/discounts" -Headers $Auth -Body $body -ContentType "application/json"
    $dupId = $first.discountId
    Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/discounts" -Headers $Auth -Body $body -ContentType "application/json" -ErrorAction Stop
    Write-Fail "Duplicate code should fail"
} catch { $code = $_.Exception.Response.StatusCode.value__; Write-Assert ($code -eq 400) "Duplicate code → 400" "Wrong" }
finally { if ($dupId) { Invoke-RestMethod -Method DELETE -Uri "$BaseUrl/api/discounts/$dupId" -Headers $Auth -ErrorAction SilentlyContinue | Out-Null } }

# Validate expired discount
$expId = $null
try {
    $expCode = "EXP$([guid]::NewGuid().ToString().Substring(0,4))"
    $body = @{ Code = $expCode; Amount = 5000; StartDate = (Get-Date).AddDays(-30).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"); EndDate = (Get-Date).AddDays(-1).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"); IsActive = $true } | ConvertTo-Json
    $exp = Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/discounts" -Headers $Auth -Body $body -ContentType "application/json"
    $expId = $exp.discountId
    $validate = Invoke-RestMethod -Uri "$BaseUrl/api/discounts/validate/$expCode" -Headers $Auth
    Write-Assert ($validate.isValid -eq $false) "Expired → isValid=false" "Should be invalid"
} catch { Write-Fail "Expired test failed" }
finally { if ($expId) { Invoke-RestMethod -Method DELETE -Uri "$BaseUrl/api/discounts/$expId" -Headers $Auth -ErrorAction SilentlyContinue | Out-Null } }

# Validate non-existent code
try {
    $validate = Invoke-RestMethod -Uri "$BaseUrl/api/discounts/validate/NONEXISTENT999" -Headers $Auth -ErrorAction Stop
    Write-Assert ($validate.isValid -eq $false) "Non-existent code → isValid=false" "Should be invalid"
} catch { $code = $_.Exception.Response.StatusCode.value__; Write-Assert ($code -eq 404) "Non-existent code → 404" "Wrong" }

Write-Host "`nPassed: $passed | Failed: $failed" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Red" })
