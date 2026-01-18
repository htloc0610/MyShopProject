# =============================================================================
# AUTH TESTS (GET + Domain)
# =============================================================================
. "$PSScriptRoot\config.ps1"
$BaseUrl = $global:BaseUrl; $Email = $global:Email; $Password = $global:Password
$passed = 0; $failed = 0

function Write-Pass { param($msg) Write-Host "  [PASS] $msg" -ForegroundColor Green; $script:passed++ }
function Write-Fail { param($msg) Write-Host "  [FAIL] $msg" -ForegroundColor Red; $script:failed++ }
function Write-Assert { param($cond, $pass, $fail) if ($cond) { Write-Pass $pass } else { Write-Fail $fail } }

Write-Host "`n=== AUTH TESTS ===" -ForegroundColor Magenta

# GET Tests
Write-Host "`n[GET]" -ForegroundColor Cyan
try {
    $body = @{ email = $Email; password = $Password } | ConvertTo-Json
    $result = Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/auth/login" -Body $body -ContentType "application/json"
    Write-Assert ($result.accessToken -ne $null) "POST /api/auth/login returns token" "No token"
    Write-Assert ($result.accessToken.Length -gt 50) "Token has valid length" "Token too short"
    $Auth = @{ Authorization = "Bearer $($result.accessToken)" }
    
    $me = Invoke-RestMethod -Uri "$BaseUrl/api/auth/me" -Headers $Auth
    Write-Assert ($me.email -eq $Email) "GET /api/auth/me returns email" "Wrong email"
    Write-Assert ($me.shopName -ne $null) "GET /api/auth/me returns shopName" "No shopName"
} catch { Write-Fail "Login failed" }

# Domain Tests
Write-Host "`n[DOMAIN]" -ForegroundColor Cyan

# Wrong password
try {
    $body = @{ email = $Email; password = "WrongPass123!" } | ConvertTo-Json
    Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/auth/login" -Body $body -ContentType "application/json" -ErrorAction Stop
    Write-Fail "Wrong password should fail"
} catch { $code = $_.Exception.Response.StatusCode.value__; Write-Assert ($code -eq 400 -or $code -eq 401) "Wrong password → $code" "Wrong" }

# Non-existent email
try {
    $body = @{ email = "fake@fake.com"; password = "Pass123!" } | ConvertTo-Json
    Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/auth/login" -Body $body -ContentType "application/json" -ErrorAction Stop
    Write-Fail "Fake email should fail"
} catch { $code = $_.Exception.Response.StatusCode.value__; Write-Assert ($code -eq 400 -or $code -eq 401) "Fake email → $code" "Wrong" }

# Empty email
try {
    $body = @{ email = ""; password = "Pass123!" } | ConvertTo-Json
    Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/auth/login" -Body $body -ContentType "application/json" -ErrorAction Stop
    Write-Fail "Empty email should fail"
} catch { $code = $_.Exception.Response.StatusCode.value__; Write-Assert ($code -eq 400 -or $code -eq 401) "Empty email → $code" "Wrong" }

# Empty password
try {
    $body = @{ email = $Email; password = "" } | ConvertTo-Json
    Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/auth/login" -Body $body -ContentType "application/json" -ErrorAction Stop
    Write-Fail "Empty password should fail"
} catch { $code = $_.Exception.Response.StatusCode.value__; Write-Assert ($code -eq 400 -or $code -eq 401) "Empty password → $code" "Wrong" }

# No token
try {
    Invoke-RestMethod -Uri "$BaseUrl/api/auth/me" -ErrorAction Stop
    Write-Fail "No token should fail 401"
} catch { $code = $_.Exception.Response.StatusCode.value__; Write-Assert ($code -eq 401) "No token → 401" "Wrong" }

# Invalid token
try {
    $fakeAuth = @{ Authorization = "Bearer fake.invalid.token" }
    Invoke-RestMethod -Uri "$BaseUrl/api/auth/me" -Headers $fakeAuth -ErrorAction Stop
    Write-Fail "Invalid token should fail 401"
} catch { $code = $_.Exception.Response.StatusCode.value__; Write-Assert ($code -eq 401) "Invalid token → 401" "Wrong" }

Write-Host "`nPassed: $passed | Failed: $failed" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Red" })
