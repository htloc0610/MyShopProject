# API Tests

## Config
Mở **`config.ps1`** để đổi server:
```powershell
$global:BaseUrl = "http://localhost:5002"           # Local
# $global:BaseUrl = "http://34.69.122.4:5002"       # Production
```

## Run
```powershell
.\api_tests\run_all.ps1          # All tests
.\api_tests\test_auth.ps1        # Auth only
.\api_tests\test_orders.ps1      # Orders only
```

## Test Files (1 file per entity)
| File | Tests |
|------|-------|
| `test_auth.ps1` | Login, token validation |
| `test_categories.ps1` | GET + CRUD |
| `test_customers.ps1` | GET + CRUD + validation |
| `test_discounts.ps1` | GET + CRUD + validation |
| `test_products.ps1` | GET + CRUD |
| `test_orders.ps1` | GET + CRUD + status + discount |
| `test_dashboard.ps1` | GET endpoints |
| `test_reports.ps1` | GET endpoints |
