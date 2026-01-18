# =============================================================================
# PRODUCTS TESTS (GET + CRUD + Domain)
# =============================================================================
. "$PSScriptRoot\config.ps1"
$BaseUrl = $global:BaseUrl; $Email = $global:Email; $Password = $global:Password
$passed = 0; $failed = 0

function Write-Pass { param($msg) Write-Host "  [PASS] $msg" -ForegroundColor Green; $script:passed++ }
function Write-Fail { param($msg) Write-Host "  [FAIL] $msg" -ForegroundColor Red; $script:failed++ }
function Write-Assert { param($cond, $pass, $fail) if ($cond) { Write-Pass $pass } else { Write-Fail $fail } }

Write-Host "`n=== PRODUCTS TESTS ===" -ForegroundColor Magenta

# Login
$body = @{ email = $Email; password = $Password } | ConvertTo-Json
$login = Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/auth/login" -Body $body -ContentType "application/json"
$Auth = @{ Authorization = "Bearer $($login.accessToken)" }
$categories = Invoke-RestMethod -Uri "$BaseUrl/api/categories" -Headers $Auth
$categoryId = $categories[0].categoryId

# GET Tests
Write-Host "`n[GET]" -ForegroundColor Cyan
$list = Invoke-RestMethod -Uri "$BaseUrl/api/products" -Headers $Auth
Write-Assert ($list -ne $null) "GET /api/products OK" "Failed"
Write-Assert ($list.items -ne $null) "Response has items" "No items"
Write-Assert ($list.totalCount -ne $null) "Response has totalCount" "No totalCount"

$page = Invoke-RestMethod -Uri "$BaseUrl/api/products?page=1&pageSize=5" -Headers $Auth
Write-Assert ($page.pageSize -eq 5) "Pagination works" "Failed"

$all = Invoke-RestMethod -Uri "$BaseUrl/api/products/all" -Headers $Auth
Write-Assert ($all -ne $null) "GET /api/products/all OK" "Failed"
Write-Assert ($all.Count -gt 0) "Has products" "No products"

if ($all -and $all.Count -gt 0) {
    $id = $all[0].id
    $detail = Invoke-RestMethod -Uri "$BaseUrl/api/products/$id" -Headers $Auth
    Write-Assert ($detail.id -eq $id) "GET /api/products/{id} OK" "Failed"
    Write-Assert ($detail.sku -ne $null) "Product has SKU" "No SKU"
    Write-Assert ($detail.name -ne $null) "Product has name" "No name"
    Write-Assert ($detail.sellingPrice -ne $null) "Product has sellingPrice" "No price"
    Write-Assert ($detail.stock -ne $null) "Product has stock" "No stock"
}

# CRUD Tests
Write-Host "`n[CRUD]" -ForegroundColor Cyan
$prodId = $null
try {
    $sku = "TEST$([guid]::NewGuid().ToString().Substring(0,6))"
    $body = @{ Sku = $sku; Name = "Test Product"; ImportPrice = 100000; SellingPrice = 150000; Count = 50; Description = "Test product description"; CategoryId = $categoryId; Images = @() } | ConvertTo-Json
    $prod = Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/products" -Headers $Auth -Body $body -ContentType "application/json"
    $prodId = $prod.id
    Write-Assert ($prodId -ne $null) "CREATE OK (ID: $prodId)" "No ID"
    Write-Assert ($prod.sku -eq $sku) "CREATE returns correct SKU" "Wrong SKU"
    
    $read = Invoke-RestMethod -Uri "$BaseUrl/api/products/$prodId" -Headers $Auth
    Write-Assert ($read.name -eq "Test Product") "READ returns correct name" "Wrong name"
    Write-Assert ($read.sellingPrice -eq 150000) "READ returns correct price" "Wrong price"
    Write-Assert ($read.stock -eq 50) "READ returns correct stock" "Wrong stock"
    
    $upBody = @{ ProductId = $prodId; Sku = $sku; Name = "Updated Product"; ImportPrice = 120000; SellingPrice = 180000; Count = 100; Description = "Updated description"; CategoryId = $categoryId; Images = @() } | ConvertTo-Json
    Invoke-RestMethod -Method PUT -Uri "$BaseUrl/api/products/$prodId" -Headers $Auth -Body $upBody -ContentType "application/json" | Out-Null
    $updated = Invoke-RestMethod -Uri "$BaseUrl/api/products/$prodId" -Headers $Auth
    Write-Assert ($updated.name -eq "Updated Product") "UPDATE name works" "Not updated"
    Write-Assert ($updated.sellingPrice -eq 180000) "UPDATE price works" "Not updated"
    Write-Assert ($updated.stock -eq 100) "UPDATE stock works" "Not updated"
    
    Invoke-RestMethod -Method DELETE -Uri "$BaseUrl/api/products/$prodId" -Headers $Auth | Out-Null
    Write-Pass "DELETE OK"
    $prodId = $null
} catch { Write-Fail "CRUD failed: $($_.Exception.Message)" }
finally { if ($prodId) { Invoke-RestMethod -Method DELETE -Uri "$BaseUrl/api/products/$prodId" -Headers $Auth -ErrorAction SilentlyContinue | Out-Null } }

# Domain Tests
Write-Host "`n[DOMAIN]" -ForegroundColor Cyan

# Invalid category
try {
    $body = @{ Sku = "TESTCAT"; Name = "Test"; ImportPrice = 100000; SellingPrice = 150000; Count = 10; CategoryId = 999999; Images = @() } | ConvertTo-Json
    Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/products" -Headers $Auth -Body $body -ContentType "application/json" -ErrorAction Stop
    Write-Fail "Invalid category should fail"
} catch { $code = $_.Exception.Response.StatusCode.value__; Write-Assert ($code -eq 400) "Invalid category → 400" "Wrong" }

# Get non-existent product
try {
    Invoke-RestMethod -Uri "$BaseUrl/api/products/999999" -Headers $Auth -ErrorAction Stop
    Write-Fail "Non-existent should fail"
} catch { $code = $_.Exception.Response.StatusCode.value__; Write-Assert ($code -eq 404) "Non-existent → 404" "Wrong" }

# Stock decreases on order
$prodId = $null; $orderId = $null
try {
    $sku = "TESTSTOCK$([guid]::NewGuid().ToString().Substring(0,4))"
    $body = @{ Sku = $sku; Name = "Stock Test"; ImportPrice = 10000; SellingPrice = 15000; Count = 100; Description = "Test"; CategoryId = $categoryId; Images = @() } | ConvertTo-Json
    $prod = Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/products" -Headers $Auth -Body $body -ContentType "application/json"
    $prodId = $prod.id
    $initialStock = $prod.stock
    
    # Create order
    $checkoutBody = @{ CustomerName = "Test"; CustomerPhone = "0999999999"; CustomerAddress = "Test"; Items = @(@{ ProductId = $prodId; Quantity = 5 }) } | ConvertTo-Json -Depth 3
    $order = Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/orders/checkout" -Headers $Auth -Body $checkoutBody -ContentType "application/json"
    $orderId = $order.orderId
    
    # Check stock decreased
    $updatedProd = Invoke-RestMethod -Uri "$BaseUrl/api/products/$prodId" -Headers $Auth
    Write-Assert ($updatedProd.stock -eq ($initialStock - 5)) "Stock decreased by 5" "Wrong stock"
} catch { Write-Fail "Stock test failed: $($_.Exception.Message)" }
finally {
    if ($orderId) { Invoke-RestMethod -Method DELETE -Uri "$BaseUrl/api/orders/$orderId" -Headers $Auth -ErrorAction SilentlyContinue | Out-Null }
    if ($prodId) { Invoke-RestMethod -Method DELETE -Uri "$BaseUrl/api/products/$prodId" -Headers $Auth -ErrorAction SilentlyContinue | Out-Null }
}

Write-Host "`nPassed: $passed | Failed: $failed" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Red" })
