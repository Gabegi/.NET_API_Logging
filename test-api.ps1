# API Test Suite - Check logs for PII masking and correlation IDs
param([string]$Url = "http://localhost:5022")

Write-Host "`nTesting $Url...`n" -ForegroundColor Cyan

# Test 1: Product with PII
$id = "test-$(Get-Random)"
Invoke-RestMethod -Method Post -Uri "$Url/api/products" -ContentType "application/json" `
    -Headers @{"X-Correlation-ID" = $id} `
    -Body '{"name":"Product","description":"Contact: user@example.com","price":99,"stock":10}'
Write-Host "[1] Product created - Check logs for ID: $id" -ForegroundColor Green

# Test 2: Order with email (PII masking test)
$id = "pii-$(Get-Random)"
Invoke-RestMethod -Method Post -Uri "$Url/api/orders" -ContentType "application/json" `
    -Headers @{"X-Correlation-ID" = $id} `
    -Body '{"customerId":"john.doe@example.com","items":[]}'
Write-Host "[2] Order created - Email should show as: ***MASKED*** (ID: $id)" -ForegroundColor Green

# Test 3: Get products
Invoke-RestMethod -Method Get -Uri "$Url/api/products" | Out-Null
Write-Host "[3] GET /api/products - Check logs for auto-generated correlation ID" -ForegroundColor Green

# Test 4: Get orders
Invoke-RestMethod -Method Get -Uri "$Url/api/orders" | Out-Null
Write-Host "[4] GET /api/orders - Check logs for auto-generated correlation ID" -ForegroundColor Green

Write-Host "`nDone! Check console logs for:" -ForegroundColor Yellow
Write-Host "  - Correlation IDs in all requests" -ForegroundColor Gray
Write-Host "  - Email masked as: ***MASKED***" -ForegroundColor Gray
Write-Host "  - Structured JSON logging" -ForegroundColor Gray
Write-Host "  - OpenTelemetry traces with timing`n" -ForegroundColor Gray
