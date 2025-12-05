# LoggingProduction API - Test Suite
# Quick tests to verify all logging features are working

param(
    [string]$BaseUrl = "http://localhost:5022"
)

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "   LoggingProduction API Test Suite" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan
Write-Host "Target: $BaseUrl`n" -ForegroundColor Gray

# Test 1: Create Product with PII data
Write-Host "Test 1: POST /api/products (PII Masking Test)" -ForegroundColor Yellow
$correlationId = "test-$(Get-Random -Maximum 9999)"
$body = @{
    name = "Test Product"
    description = "Contact support@company.com"
    price = 99.99
    stock = 10
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Method Post `
        -Uri "$BaseUrl/api/products" `
        -ContentType "application/json" `
        -Headers @{"X-Correlation-ID" = $correlationId} `
        -Body $body

    Write-Host "✅ Product created: $($response.id)" -ForegroundColor Green
    Write-Host "   Check logs for CorrelationId: $correlationId" -ForegroundColor Gray
} catch {
    Write-Host "❌ Failed: $_" -ForegroundColor Red
}

# Test 2: Create Order with email as customer ID (PII)
Write-Host "`nTest 2: POST /api/orders (Email PII Masking)" -ForegroundColor Yellow
$correlationId = "pii-test-$(Get-Random -Maximum 9999)"
$body = @{
    customerId = "john.doe@example.com"
    items = @()
    shippingAddress = "123 Main St"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Method Post `
        -Uri "$BaseUrl/api/orders" `
        -ContentType "application/json" `
        -Headers @{"X-Correlation-ID" = $correlationId} `
        -Body $body

    Write-Host "✅ Order created: $($response.id)" -ForegroundColor Green
    Write-Host "   Email should be masked as: ***MASKED***" -ForegroundColor Gray
    Write-Host "   Check logs for CorrelationId: $correlationId" -ForegroundColor Gray
} catch {
    Write-Host "❌ Failed: $_" -ForegroundColor Red
}

# Test 3: Get all products
Write-Host "`nTest 3: GET /api/products" -ForegroundColor Yellow
$correlationId = "get-test-$(Get-Random -Maximum 9999)"

try {
    $response = Invoke-RestMethod -Method Get `
        -Uri "$BaseUrl/api/products" `
        -Headers @{"X-Correlation-ID" = $correlationId}

    Write-Host "✅ Retrieved $($response.Count) products" -ForegroundColor Green
    Write-Host "   Check logs for CorrelationId: $correlationId" -ForegroundColor Gray
} catch {
    Write-Host "❌ Failed: $_" -ForegroundColor Red
}

# Test 4: Get all orders
Write-Host "`nTest 4: GET /api/orders" -ForegroundColor Yellow
$correlationId = "get-orders-$(Get-Random -Maximum 9999)"

try {
    $response = Invoke-RestMethod -Method Get `
        -Uri "$BaseUrl/api/orders" `
        -Headers @{"X-Correlation-ID" = $correlationId}

    Write-Host "✅ Retrieved $($response.Count) orders" -ForegroundColor Green
    Write-Host "   Check logs for CorrelationId: $correlationId" -ForegroundColor Gray
} catch {
    Write-Host "❌ Failed: $_" -ForegroundColor Red
}

# Test 5: Concurrent requests
Write-Host "`nTest 5: Concurrent Requests (3 parallel)" -ForegroundColor Yellow

try {
    $job1 = Start-Job -ScriptBlock {
        param($url)
        Invoke-RestMethod -Method Get -Uri "$url/api/products"
    } -ArgumentList $BaseUrl

    $job2 = Start-Job -ScriptBlock {
        param($url)
        Invoke-RestMethod -Method Get -Uri "$url/api/orders"
    } -ArgumentList $BaseUrl

    $job3 = Start-Job -ScriptBlock {
        param($url)
        Invoke-RestMethod -Method Get -Uri "$url/api/products"
    } -ArgumentList $BaseUrl

    $job1, $job2, $job3 | Wait-Job | Out-Null
    Remove-Job -Job $job1, $job2, $job3

    Write-Host "✅ All 3 concurrent requests completed" -ForegroundColor Green
    Write-Host "   Check logs for 3 different auto-generated Correlation IDs" -ForegroundColor Gray
} catch {
    Write-Host "❌ Failed: $_" -ForegroundColor Red
}

# Summary
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "             Test Summary" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "What to verify in logs:" -ForegroundColor Yellow
Write-Host "  ✓ All requests show Correlation IDs" -ForegroundColor Gray
Write-Host "  ✓ Custom IDs are used when provided" -ForegroundColor Gray
Write-Host "  ✓ Auto-generated GUIDs when not provided" -ForegroundColor Gray
Write-Host "  ✓ Email addresses masked as: ***MASKED***" -ForegroundColor Gray
Write-Host "  ✓ Structured logging (JSON format)" -ForegroundColor Gray
Write-Host "  ✓ Activity traces with timing information" -ForegroundColor Gray
Write-Host "  ✓ Source-generated logging (high performance)" -ForegroundColor Gray

Write-Host "`nLog locations:" -ForegroundColor Yellow
Write-Host "  • Console output (structured JSON)" -ForegroundColor Gray
Write-Host "  • File: src/LoggingProduction/Logs/log-YYYY-MM-DD.txt" -ForegroundColor Gray
Write-Host "  • Elasticsearch (if running): http://localhost:9200" -ForegroundColor Gray

Write-Host "`n✅ All tests completed!`n" -ForegroundColor Green
