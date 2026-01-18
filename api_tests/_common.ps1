# =============================================================================
# MyShop API Testing - Common Functions & Login
# =============================================================================

# Load config
. "$PSScriptRoot\config.ps1"

$BaseUrl = $global:BaseUrl
$Email = $global:Email
$Password = $global:Password

# Colors
function Write-Pass { param($msg) Write-Host "[PASS] $msg" -ForegroundColor Green }
function Write-Fail { param($msg) Write-Host "[FAIL] $msg" -ForegroundColor Red }
function Write-Info { param($msg) Write-Host "[INFO] $msg" -ForegroundColor Cyan }
function Write-Header { param($msg) Write-Host "`n=== $msg ===" -ForegroundColor Yellow }
function Write-Skip { param($msg) Write-Host "[SKIP] $msg" -ForegroundColor DarkGray }

# Statistics
$script:passed = 0
$script:failed = 0
$script:skipped = 0

function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Url,
        [hashtable]$Headers = @{},
        [string]$Body = $null
    )
    
    try {
        $params = @{
            Uri = $Url
            Method = $Method
            ContentType = "application/json"
            ErrorAction = "Stop"
        }
        if ($Headers.Count -gt 0) { $params.Headers = $Headers }
        if ($Body) { $params.Body = $Body }
        
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        $response = Invoke-RestMethod @params
        $sw.Stop()
        
        Write-Pass "$Name (${sw.ElapsedMilliseconds}ms)"
        $script:passed++
        return $response
    }
    catch {
        $code = $_.Exception.Response.StatusCode.value__
        Write-Fail "$Name - Status: $code"
        $script:failed++
        return $null
    }
}

function Get-AuthToken {
    Write-Header "LOGIN"
    $body = @{ email = $Email; password = $Password } | ConvertTo-Json
    $result = Test-Endpoint -Name "POST /api/auth/login" -Method "POST" -Url "$BaseUrl/api/auth/login" -Body $body
    
    if ($result -and $result.accessToken) {
        Write-Info "Token acquired"
        return @{ Authorization = "Bearer $($result.accessToken)" }
    }
    Write-Host "[CRITICAL] Login failed!" -ForegroundColor Red
    exit 1
}

function Show-Summary {
    Write-Host "`n=============================================" -ForegroundColor Magenta
    Write-Host "            TEST SUMMARY                    " -ForegroundColor Magenta
    Write-Host "=============================================" -ForegroundColor Magenta
    Write-Host "Passed:  $script:passed" -ForegroundColor Green
    Write-Host "Failed:  $script:failed" -ForegroundColor Red
    Write-Host "Skipped: $script:skipped" -ForegroundColor DarkGray
    Write-Host "---------------------------------------------"
    
    if ($script:failed -eq 0) {
        Write-Host "[SUCCESS] All tests passed!" -ForegroundColor Green
    } else {
        Write-Host "[WARNING] Some tests failed!" -ForegroundColor Yellow
    }
}
