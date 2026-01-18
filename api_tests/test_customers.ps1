# =============================================================================
# CUSTOMERS TESTS (GET + CRUD + Domain)
# =============================================================================
. "$PSScriptRoot\config.ps1"
$BaseUrl = $global:BaseUrl; $Email = $global:Email; $Password = $global:Password
$passed = 0; $failed = 0

function Write-Pass { param($msg) Write-Host "  [PASS] $msg" -ForegroundColor Green; $script:passed++ }
function Write-Fail { param($msg) Write-Host "  [FAIL] $msg" -ForegroundColor Red; $script:failed++ }
function Write-Assert { param($cond, $pass, $fail) if ($cond) { Write-Pass $pass } else { Write-Fail $fail } }

Write-Host "`n=== CUSTOMERS TESTS ===" -ForegroundColor Magenta

# Login
$body = @{ email = $Email; password = $Password } | ConvertTo-Json
$login = Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/auth/login" -Body $body -ContentType "application/json"
$Auth = @{ Authorization = "Bearer $($login.accessToken)" }

# GET Tests
Write-Host "`n[GET]" -ForegroundColor Cyan
$list = Invoke-RestMethod -Uri "$BaseUrl/api/customers" -Headers $Auth
Write-Assert ($list -ne $null) "GET /api/customers OK" "Failed"
Write-Assert ($list.items -ne $null) "Response has items array" "No items"
Write-Assert ($list.totalCount -ne $null) "Response has totalCount" "No totalCount"

# Pagination
$page = Invoke-RestMethod -Uri "$BaseUrl/api/customers?page=1&pageSize=5" -Headers $Auth
Write-Assert ($page.pageSize -eq 5) "Pagination pageSize=5 works" "Wrong pageSize"
Write-Assert ($page.currentPage -eq 1) "Pagination currentPage=1" "Wrong page"

if ($list.items -and $list.items.Count -gt 0) {
    $id = $list.items[0].id
    $detail = Invoke-RestMethod -Uri "$BaseUrl/api/customers/$id" -Headers $Auth
    Write-Assert ($detail.id -eq $id) "GET /api/customers/{id} OK" "Failed"
    Write-Assert ($detail.name -ne $null) "Customer has name" "No name"
    Write-Assert ($detail.phoneNumber -ne $null) "Customer has phoneNumber" "No phone"
}

# CRUD Tests
Write-Host "`n[CRUD]" -ForegroundColor Cyan
$custId = $null
try {
    $phone = "09$(Get-Random -Min 10000000 -Max 99999999)"
    $body = @{ Name = "Test Customer"; PhoneNumber = $phone; Address = "Test Address 123" } | ConvertTo-Json
    $cust = Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/customers" -Headers $Auth -Body $body -ContentType "application/json"
    $custId = $cust.id
    Write-Assert ($custId -ne $null) "CREATE OK (ID: $custId)" "No ID"
    Write-Assert ($cust.totalSpent -eq 0) "New customer has totalSpent=0" "Wrong totalSpent"
    
    $read = Invoke-RestMethod -Uri "$BaseUrl/api/customers/$custId" -Headers $Auth
    Write-Assert ($read.phoneNumber -eq $phone) "READ returns correct phone" "Wrong phone"
    Write-Assert ($read.name -eq "Test Customer") "READ returns correct name" "Wrong name"
    
    $upBody = @{ Id = $custId; Name = "Updated Customer"; PhoneNumber = $phone; Address = "Updated Address 456" } | ConvertTo-Json
    Invoke-RestMethod -Method PUT -Uri "$BaseUrl/api/customers/$custId" -Headers $Auth -Body $upBody -ContentType "application/json" | Out-Null
    $updated = Invoke-RestMethod -Uri "$BaseUrl/api/customers/$custId" -Headers $Auth
    Write-Assert ($updated.name -eq "Updated Customer") "UPDATE name works" "Name not updated"
    Write-Assert ($updated.address -eq "Updated Address 456") "UPDATE address works" "Address not updated"
    
    Invoke-RestMethod -Method DELETE -Uri "$BaseUrl/api/customers/$custId" -Headers $Auth | Out-Null
    Write-Pass "DELETE OK"
    $custId = $null
} catch { Write-Fail "CRUD failed: $($_.Exception.Message)" }
finally { if ($custId) { Invoke-RestMethod -Method DELETE -Uri "$BaseUrl/api/customers/$custId" -Headers $Auth -ErrorAction SilentlyContinue | Out-Null } }

# Domain Tests
Write-Host "`n[DOMAIN]" -ForegroundColor Cyan

# Empty name
try {
    $body = @{ Name = ""; PhoneNumber = "0912345678"; Address = "Test" } | ConvertTo-Json
    Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/customers" -Headers $Auth -Body $body -ContentType "application/json" -ErrorAction Stop
    Write-Fail "Empty name should fail"
} catch { $code = $_.Exception.Response.StatusCode.value__; Write-Assert ($code -eq 400) "Empty name → 400" "Wrong" }

# Empty phone
try {
    $body = @{ Name = "Test"; PhoneNumber = ""; Address = "Test" } | ConvertTo-Json
    Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/customers" -Headers $Auth -Body $body -ContentType "application/json" -ErrorAction Stop
    Write-Fail "Empty phone should fail"
} catch { $code = $_.Exception.Response.StatusCode.value__; Write-Assert ($code -eq 400) "Empty phone → 400" "Wrong" }

# Get non-existent customer
try {
    Invoke-RestMethod -Uri "$BaseUrl/api/customers/00000000-0000-0000-0000-000000000000" -Headers $Auth -ErrorAction Stop
    Write-Fail "Non-existent ID should fail"
} catch { $code = $_.Exception.Response.StatusCode.value__; Write-Assert ($code -eq 404) "Non-existent → 404" "Wrong" }

# Delete non-existent customer
try {
    Invoke-RestMethod -Method DELETE -Uri "$BaseUrl/api/customers/00000000-0000-0000-0000-000000000000" -Headers $Auth -ErrorAction Stop
    Write-Fail "Delete non-existent should fail"
} catch { $code = $_.Exception.Response.StatusCode.value__; Write-Assert ($code -eq 404) "Delete non-existent → 404" "Wrong" }

Write-Host "`nPassed: $passed | Failed: $failed" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Red" })
