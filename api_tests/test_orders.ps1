# =============================================================================
# ORDERS TESTS (GET + CRUD + Domain)
# =============================================================================
. "$PSScriptRoot\config.ps1"
$BaseUrl = $global:BaseUrl; $Email = $global:Email; $Password = $global:Password
$passed = 0; $failed = 0

function Write-Pass { param($msg) Write-Host "  [PASS] $msg" -ForegroundColor Green; $script:passed++ }
function Write-Fail { param($msg) Write-Host "  [FAIL] $msg" -ForegroundColor Red; $script:failed++ }
function Write-Assert { param($cond, $pass, $fail) if ($cond) { Write-Pass $pass } else { Write-Fail $fail } }

Write-Host "`n=== ORDERS TESTS ===" -ForegroundColor Magenta

# Login
$body = @{ email = $Email; password = $Password } | ConvertTo-Json
$login = Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/auth/login" -Body $body -ContentType "application/json"
$Auth = @{ Authorization = "Bearer $($login.accessToken)" }
$products = Invoke-RestMethod -Uri "$BaseUrl/api/products/all" -Headers $Auth
$product = $products[0]

# GET Tests
Write-Host "`n[GET]" -ForegroundColor Cyan
$list = Invoke-RestMethod -Uri "$BaseUrl/api/orders" -Headers $Auth
Write-Assert ($list -ne $null) "GET /api/orders OK" "Failed"
Write-Assert ($list.items -ne $null) "Response has items" "No items"
Write-Assert ($list.totalCount -ne $null) "Response has totalCount" "No totalCount"

$page = Invoke-RestMethod -Uri "$BaseUrl/api/orders?page=1&pageSize=5" -Headers $Auth
Write-Assert ($page.pageSize -eq 5) "Pagination works" "Failed"

if ($list.items -and $list.items.Count -gt 0) {
    $id = $list.items[0].orderId
    $detail = Invoke-RestMethod -Uri "$BaseUrl/api/orders/$id" -Headers $Auth
    Write-Assert ($detail.orderId -eq $id) "GET /api/orders/{id} OK" "Failed"
    Write-Assert ($detail.totalAmount -ne $null) "Order has totalAmount" "No totalAmount"
    Write-Assert ($detail.finalAmount -ne $null) "Order has finalAmount" "No finalAmount"
    Write-Assert ($detail.status -ne $null) "Order has status" "No status"
    Write-Assert ($detail.items -ne $null) "Order has items" "No items"
}

$coupons = Invoke-RestMethod -Uri "$BaseUrl/api/orders/available-coupons" -Headers $Auth
Write-Assert ($coupons -ne $null) "GET /api/orders/available-coupons OK" "Failed"

# CRUD Tests
Write-Host "`n[CRUD]" -ForegroundColor Cyan
$orderId = $null
try {
    $phone = "09$(Get-Random -Min 10000000 -Max 99999999)"
    $checkoutBody = @{ CustomerName = "Test Order"; CustomerPhone = $phone; CustomerAddress = "Test Address 123"; Items = @(@{ ProductId = $product.id; Quantity = 1 }) } | ConvertTo-Json -Depth 3
    $order = Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/orders/checkout" -Headers $Auth -Body $checkoutBody -ContentType "application/json"
    $orderId = $order.orderId
    Write-Assert ($orderId -ne $null) "CREATE OK (ID: $orderId)" "No ID"
    Write-Assert ($order.finalAmount -gt 0) "Order has finalAmount > 0" "No amount"
    
    $read = Invoke-RestMethod -Uri "$BaseUrl/api/orders/$orderId" -Headers $Auth
    Write-Assert ($read.orderId -eq $orderId) "READ OK" "Wrong ID"
    Write-Assert ($read.status -eq "Created") "New order status = Created" "Wrong status"
    Write-Assert ($read.items.Count -eq 1) "Order has 1 item" "Wrong item count"
    
    $upBody = @{ Status = "Created"; CustomerName = "Updated Name"; CustomerPhone = $phone; CustomerAddress = "Updated Address" } | ConvertTo-Json
    Invoke-RestMethod -Method PUT -Uri "$BaseUrl/api/orders/$orderId" -Headers $Auth -Body $upBody -ContentType "application/json" | Out-Null
    $updated = Invoke-RestMethod -Uri "$BaseUrl/api/orders/$orderId" -Headers $Auth
    Write-Assert ($updated.customerName -eq "Updated Name") "UPDATE name works" "Not updated"
    
    Invoke-RestMethod -Method DELETE -Uri "$BaseUrl/api/orders/$orderId" -Headers $Auth | Out-Null
    Write-Pass "DELETE OK"
    $orderId = $null
} catch { Write-Fail "CRUD failed: $($_.Exception.Message)" }

# Domain Tests
Write-Host "`n[DOMAIN]" -ForegroundColor Cyan

# Cannot update cancelled order
$orderId = $null
try {
    $checkoutBody = @{ CustomerName = "Test"; CustomerPhone = "09$(Get-Random -Min 10000000 -Max 99999999)"; CustomerAddress = "Test"; Items = @(@{ ProductId = $product.id; Quantity = 1 }) } | ConvertTo-Json -Depth 3
    $order = Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/orders/checkout" -Headers $Auth -Body $checkoutBody -ContentType "application/json"
    $orderId = $order.orderId
    $cancelBody = @{ Status = "Cancelled"; CustomerName = "Test"; CustomerPhone = "0999999999"; CustomerAddress = "Test" } | ConvertTo-Json
    Invoke-RestMethod -Method PUT -Uri "$BaseUrl/api/orders/$orderId" -Headers $Auth -Body $cancelBody -ContentType "application/json" | Out-Null
    $updateBody = @{ Status = "Created"; CustomerName = "X"; CustomerPhone = "0888888888"; CustomerAddress = "X" } | ConvertTo-Json
    Invoke-RestMethod -Method PUT -Uri "$BaseUrl/api/orders/$orderId" -Headers $Auth -Body $updateBody -ContentType "application/json" -ErrorAction Stop
    Write-Fail "Update cancelled should fail"
} catch { $code = $_.Exception.Response.StatusCode.value__; Write-Assert ($code -eq 400) "Update cancelled → 400" "Wrong" }

# Empty items should fail
try {
    $checkoutBody = @{ CustomerName = "Test"; CustomerPhone = "0999999999"; CustomerAddress = "Test"; Items = @() } | ConvertTo-Json -Depth 3
    Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/orders/checkout" -Headers $Auth -Body $checkoutBody -ContentType "application/json" -ErrorAction Stop
    Write-Fail "Empty items should fail"
} catch { $code = $_.Exception.Response.StatusCode.value__; Write-Assert ($code -eq 400) "Empty items → 400" "Wrong" }

# Invalid product ID
try {
    $checkoutBody = @{ CustomerName = "Test"; CustomerPhone = "0999999999"; CustomerAddress = "Test"; Items = @(@{ ProductId = 999999; Quantity = 1 }) } | ConvertTo-Json -Depth 3
    Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/orders/checkout" -Headers $Auth -Body $checkoutBody -ContentType "application/json" -ErrorAction Stop
    Write-Fail "Invalid product should fail"
} catch { $code = $_.Exception.Response.StatusCode.value__; Write-Assert ($code -eq 400) "Invalid product → 400" "Wrong" }

# Quantity = 0
try {
    $checkoutBody = @{ CustomerName = "Test"; CustomerPhone = "0999999999"; CustomerAddress = "Test"; Items = @(@{ ProductId = $product.id; Quantity = 0 }) } | ConvertTo-Json -Depth 3
    Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/orders/checkout" -Headers $Auth -Body $checkoutBody -ContentType "application/json" -ErrorAction Stop
    Write-Fail "Quantity=0 should fail"
} catch { $code = $_.Exception.Response.StatusCode.value__; Write-Assert ($code -eq 400) "Quantity=0 → 400" "Wrong" }

# Discount applies correctly
$discId = $null; $orderId2 = $null
try {
    $discCode = "TESTORD$([guid]::NewGuid().ToString().Substring(0,4))"
    $discBody = @{ Code = $discCode; Amount = 10000; StartDate = (Get-Date).AddDays(-1).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"); EndDate = (Get-Date).AddDays(30).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"); UsageLimit = 100; IsActive = $true } | ConvertTo-Json
    $disc = Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/discounts" -Headers $Auth -Body $discBody -ContentType "application/json"
    $discId = $disc.discountId
    $checkoutBody = @{ CustomerName = "Test"; CustomerPhone = "09$(Get-Random -Min 10000000 -Max 99999999)"; CustomerAddress = "Test"; Items = @(@{ ProductId = $product.id; Quantity = 1 }); CouponCode = $discCode } | ConvertTo-Json -Depth 3
    $checkout = Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/orders/checkout" -Headers $Auth -Body $checkoutBody -ContentType "application/json"
    $orderId2 = $checkout.orderId
    $detail = Invoke-RestMethod -Uri "$BaseUrl/api/orders/$orderId2" -Headers $Auth
    $discountApplied = $detail.totalAmount - $detail.finalAmount
    Write-Assert ($discountApplied -eq 10000) "Discount: $discountApplied VND" "Wrong"
} catch { Write-Fail "Discount test failed" }
finally {
    if ($orderId2) { Invoke-RestMethod -Method DELETE -Uri "$BaseUrl/api/orders/$orderId2" -Headers $Auth -ErrorAction SilentlyContinue | Out-Null }
    if ($discId) { Invoke-RestMethod -Method DELETE -Uri "$BaseUrl/api/discounts/$discId" -Headers $Auth -ErrorAction SilentlyContinue | Out-Null }
}

# Status transitions: Created → Paid
$orderId3 = $null; $custId = $null
try {
    # Create customer
    $custBody = @{ Name = "TotalSpent Test"; PhoneNumber = "09$(Get-Random -Min 10000000 -Max 99999999)"; Address = "Test" } | ConvertTo-Json
    $cust = Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/customers" -Headers $Auth -Body $custBody -ContentType "application/json"
    $custId = $cust.id
    $initialSpent = $cust.totalSpent
    
    # Create order
    $checkoutBody = @{ CustomerName = $cust.name; CustomerPhone = $cust.phoneNumber; CustomerAddress = "Test"; CustomerId = $custId; Items = @(@{ ProductId = $product.id; Quantity = 1 }) } | ConvertTo-Json -Depth 3
    $order = Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/orders/checkout" -Headers $Auth -Body $checkoutBody -ContentType "application/json"
    $orderId3 = $order.orderId
    $orderAmount = $order.finalAmount
    
    # Change to Paid
    $paidBody = @{ Status = "Paid"; CustomerName = $cust.name; CustomerPhone = $cust.phoneNumber; CustomerAddress = "Test" } | ConvertTo-Json
    Invoke-RestMethod -Method PUT -Uri "$BaseUrl/api/orders/$orderId3" -Headers $Auth -Body $paidBody -ContentType "application/json" | Out-Null
    
    # Check TotalSpent increased
    $updatedCust = Invoke-RestMethod -Uri "$BaseUrl/api/customers/$custId" -Headers $Auth
    Write-Assert ($updatedCust.totalSpent -eq ($initialSpent + $orderAmount)) "TotalSpent increased on Paid" "Wrong"
    
    # Cancel and check TotalSpent decreased
    $cancelBody = @{ Status = "Cancelled"; CustomerName = $cust.name; CustomerPhone = $cust.phoneNumber; CustomerAddress = "Test" } | ConvertTo-Json
    Invoke-RestMethod -Method PUT -Uri "$BaseUrl/api/orders/$orderId3" -Headers $Auth -Body $cancelBody -ContentType "application/json" | Out-Null
    $finalCust = Invoke-RestMethod -Uri "$BaseUrl/api/customers/$custId" -Headers $Auth
    Write-Assert ($finalCust.totalSpent -eq $initialSpent) "TotalSpent restored on Cancel" "Wrong"
} catch { Write-Fail "Status transition test failed: $($_.Exception.Message)" }
finally {
    if ($custId) { Invoke-RestMethod -Method DELETE -Uri "$BaseUrl/api/customers/$custId" -Headers $Auth -ErrorAction SilentlyContinue | Out-Null }
}

Write-Host "`nPassed: $passed | Failed: $failed" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Red" })
