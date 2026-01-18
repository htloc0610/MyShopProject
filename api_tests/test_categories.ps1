# =============================================================================
# CATEGORIES TESTS (GET + CRUD + Domain)
# =============================================================================
. "$PSScriptRoot\config.ps1"
$BaseUrl = $global:BaseUrl; $Email = $global:Email; $Password = $global:Password
$passed = 0; $failed = 0

function Write-Pass { param($msg) Write-Host "  [PASS] $msg" -ForegroundColor Green; $script:passed++ }
function Write-Fail { param($msg) Write-Host "  [FAIL] $msg" -ForegroundColor Red; $script:failed++ }
function Write-Assert { param($cond, $pass, $fail) if ($cond) { Write-Pass $pass } else { Write-Fail $fail } }

Write-Host "`n=== CATEGORIES TESTS ===" -ForegroundColor Magenta

# Login
$body = @{ email = $Email; password = $Password } | ConvertTo-Json
$login = Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/auth/login" -Body $body -ContentType "application/json"
$Auth = @{ Authorization = "Bearer $($login.accessToken)" }

# GET Tests
Write-Host "`n[GET]" -ForegroundColor Cyan
$list = Invoke-RestMethod -Uri "$BaseUrl/api/categories" -Headers $Auth
Write-Assert ($list -ne $null) "GET /api/categories OK" "Failed"
Write-Assert ($list.Count -gt 0) "Has categories" "No categories"

if ($list -and $list.Count -gt 0) {
    $id = $list[0].categoryId
    $detail = Invoke-RestMethod -Uri "$BaseUrl/api/categories/$id" -Headers $Auth
    Write-Assert ($detail.categoryId -eq $id) "GET /api/categories/{id} OK" "Failed"
    Write-Assert ($detail.name -ne $null) "Category has name" "No name"
}

# CRUD Tests
Write-Host "`n[CRUD]" -ForegroundColor Cyan
$catId = $null
try {
    $name = "TestCat$([guid]::NewGuid().ToString().Substring(0,4))"
    $body = @{ Name = $name; Description = "Test category description" } | ConvertTo-Json
    $cat = Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/categories" -Headers $Auth -Body $body -ContentType "application/json"
    $catId = $cat.categoryId
    Write-Assert ($catId -ne $null) "CREATE OK (ID: $catId)" "No ID"
    Write-Assert ($cat.name -eq $name) "CREATE returns correct name" "Wrong name"
    
    $read = Invoke-RestMethod -Uri "$BaseUrl/api/categories/$catId" -Headers $Auth
    Write-Assert ($read.description -eq "Test category description") "READ returns correct description" "Wrong"
    
    $upBody = @{ CategoryId = $catId; Name = "Updated Category"; Description = "Updated description" } | ConvertTo-Json
    Invoke-RestMethod -Method PUT -Uri "$BaseUrl/api/categories/$catId" -Headers $Auth -Body $upBody -ContentType "application/json" | Out-Null
    $updated = Invoke-RestMethod -Uri "$BaseUrl/api/categories/$catId" -Headers $Auth
    Write-Assert ($updated.name -eq "Updated Category") "UPDATE name works" "Not updated"
    Write-Assert ($updated.description -eq "Updated description") "UPDATE description works" "Not updated"
    
    Invoke-RestMethod -Method DELETE -Uri "$BaseUrl/api/categories/$catId" -Headers $Auth | Out-Null
    Write-Pass "DELETE OK"
    $catId = $null
} catch { Write-Fail "CRUD failed: $($_.Exception.Message)" }
finally { if ($catId) { Invoke-RestMethod -Method DELETE -Uri "$BaseUrl/api/categories/$catId" -Headers $Auth -ErrorAction SilentlyContinue | Out-Null } }

# Domain Tests  
Write-Host "`n[DOMAIN]" -ForegroundColor Cyan

# Get non-existent category
try {
    Invoke-RestMethod -Uri "$BaseUrl/api/categories/999999" -Headers $Auth -ErrorAction Stop
    Write-Fail "Non-existent should fail"
} catch { $code = $_.Exception.Response.StatusCode.value__; Write-Assert ($code -eq 404) "Non-existent → 404" "Wrong" }

# Delete non-existent category
try {
    Invoke-RestMethod -Method DELETE -Uri "$BaseUrl/api/categories/999999" -Headers $Auth -ErrorAction Stop
    Write-Fail "Delete non-existent should fail"
} catch { $code = $_.Exception.Response.StatusCode.value__; Write-Assert ($code -eq 404) "Delete non-existent → 404" "Wrong" }

Write-Host "`nPassed: $passed | Failed: $failed" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Red" })
