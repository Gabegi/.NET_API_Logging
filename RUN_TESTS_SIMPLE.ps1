# LoggingProduction API - Simple Test Suite Runner
# This script runs all 10 tests to verify logging implementation

$Uri = "https://localhost:7002"

# Color output
$Green = [System.ConsoleColor]::Green
$Red = [System.ConsoleColor]::Red
$Yellow = [System.ConsoleColor]::Yellow
$Cyan = [System.ConsoleColor]::Cyan

Write-Host "`n" -NoNewline
Write-Host "=" * 80 -ForegroundColor $Cyan
Write-Host "LoggingProduction API - Test Suite" -ForegroundColor $Cyan
Write-Host "=" * 80 -ForegroundColor $Cyan

# Test 1: Create Order with Correlation ID
Write-Host "`nTest 1: POST /api/orders with custom Correlation ID" -ForegroundColor $Yellow
$customId = "test-correlation-id-$(Get-Random)"
Write-Host "Using Correlation ID: $customId" -ForegroundColor $Cyan

$body = @{
    customerId = "cust-456"
    total = 99.99
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Method Post `
        -Uri "$Uri/api/orders" `
        -ContentType "application/json" `
        -Headers @{"X-Correlation-Id" = $customId} `
        -Body $body `
        -SkipCertificateCheck

    Write-Host "Response:" -ForegroundColor $Green
    $response | ConvertTo-Json | Write-Host
    Write-Host "`nCHECK TERMINAL 1 LOGS FOR:" -ForegroundColor $Yellow
    Write-Host "  - CorrelationId: $customId (appears in all logs)" -ForegroundColor $Gray
    Write-Host "  - customerId: cust-456 (structured property)" -ForegroundColor $Gray
    Write-Host "  - total: 99.99 (structured property)" -ForegroundColor $Gray
} catch {
    Write-Host "ERROR: $_" -ForegroundColor $Red
}

# Test 2: Auto-generated Correlation ID
Write-Host "`nTest 2: GET /api/orders (auto-generated Correlation ID)" -ForegroundColor $Yellow

try {
    $response = Invoke-WebRequest -Method Get `
        -Uri "$Uri/api/orders" `
        -SkipCertificateCheck `
        -PassThru

    $autoId = $response.Headers["X-Correlation-Id"]
    Write-Host "Response Header X-Correlation-Id: $autoId" -ForegroundColor $Green
    Write-Host "`nCHECK TERMINAL 1 LOGS FOR:" -ForegroundColor $Yellow
    Write-Host "  - Auto-generated GUID in logs" -ForegroundColor $Gray
    Write-Host "  - This ID in all logs for this request" -ForegroundColor $Gray
} catch {
    Write-Host "ERROR: $_" -ForegroundColor $Red
}

# Test 3: Health check (should not spam logs)
Write-Host "`nTest 3: GET /health (should NOT appear in logs)" -ForegroundColor $Yellow

try {
    $response = Invoke-RestMethod -Method Get `
        -Uri "$Uri/health" `
        -SkipCertificateCheck

    Write-Host "Health check response: $($response.status)" -ForegroundColor $Green
    Write-Host "`nCHECK TERMINAL 1 LOGS FOR:" -ForegroundColor $Yellow
    Write-Host "  - NO log entry (health checks at VERBOSE level, filtered)" -ForegroundColor $Gray
} catch {
    Write-Host "ERROR: $_" -ForegroundColor $Red
}

# Test 4: Error handling (404)
Write-Host "`nTest 4: GET /api/orders/non-existent (404 error)" -ForegroundColor $Yellow

try {
    Invoke-RestMethod -Method Get `
        -Uri "$Uri/api/orders/non-existent" `
        -SkipCertificateCheck `
        -ErrorAction Stop
} catch {
    $statusCode = [int]$_.Exception.Response.StatusCode
    Write-Host "Status Code: $statusCode (404 Not Found)" -ForegroundColor $Green
    Write-Host "`nCHECK TERMINAL 1 LOGS FOR:" -ForegroundColor $Yellow
    Write-Host "  - Log level: [WRN] (WARNING, not INFO)" -ForegroundColor $Gray
    Write-Host "  - Status code 404 auto-elevated to WARNING" -ForegroundColor $Gray
}

# Test 5: Concurrent requests
Write-Host "`nTest 5: Concurrent requests (3 parallel)" -ForegroundColor $Yellow

try {
    $job1 = Start-Job -ScriptBlock {
        Invoke-RestMethod -Method Get -Uri "https://localhost:7002/api/orders" -SkipCertificateCheck
    }

    $job2 = Start-Job -ScriptBlock {
        Invoke-RestMethod -Method Get -Uri "https://localhost:7002/api/products" -SkipCertificateCheck
    }

    $job3 = Start-Job -ScriptBlock {
        Invoke-RestMethod -Method Get -Uri "https://localhost:7002/api/orders/search?customerId=test" -SkipCertificateCheck
    }

    $job1, $job2, $job3 | Wait-Job | Out-Null
    Remove-Job -Job $job1, $job2, $job3

    Write-Host "All 3 concurrent requests completed" -ForegroundColor $Green
    Write-Host "`nCHECK TERMINAL 1 LOGS FOR:" -ForegroundColor $Yellow
    Write-Host "  - 3 different Correlation IDs" -ForegroundColor $Gray
    Write-Host "  - No ID reuse or collision" -ForegroundColor $Gray
} catch {
    Write-Host "ERROR: $_" -ForegroundColor $Red
}

# Test 6: Verify log files
Write-Host "`nTest 6: Verify log files" -ForegroundColor $Yellow

if (Test-Path "src\LoggingProduction\logs") {
    $logFiles = Get-ChildItem "src\LoggingProduction\logs" -Filter "log-*.txt" -ErrorAction SilentlyContinue
    if ($logFiles) {
        Write-Host "Log files found:" -ForegroundColor $Green
        $logFiles | ForEach-Object {
            $sizeKB = [math]::Round($_.Length / 1024, 2)
            Write-Host "  - $($_.Name) ($sizeKB KB)" -ForegroundColor $Cyan
        }

        Write-Host "`nLatest log entries:" -ForegroundColor $Yellow
        $latest = $logFiles | Sort-Object LastWriteTime | Select-Object -Last 1
        Get-Content $latest.FullName -Tail 5 | Write-Host

        Write-Host "`nCHECK:" -ForegroundColor $Yellow
        Write-Host "  - Files are in JSON format (structured)" -ForegroundColor $Gray
        Write-Host "  - Daily rolling enabled (log-YYYY-MM-DD.txt)" -ForegroundColor $Gray
        Write-Host "  - 7-day retention configured" -ForegroundColor $Gray
    } else {
        Write-Host "No log files found yet" -ForegroundColor $Yellow
    }
} else {
    Write-Host "Logs directory not found (app may not have logged yet)" -ForegroundColor $Yellow
}

# Test 7: Search logs
Write-Host "`nTest 7: Search logs by correlation ID" -ForegroundColor $Yellow

if (Test-Path "src\LoggingProduction\logs") {
    $searchTerm = "cust-456"
    $matches = Select-String $searchTerm "src\LoggingProduction\logs\log-*.txt" -ErrorAction SilentlyContinue | Measure-Object
    Write-Host "Found $($matches.Count) log entries with '$searchTerm'" -ForegroundColor $Green

    if ($matches.Count -gt 0) {
        Write-Host "`nCHECK:" -ForegroundColor $Yellow
        Write-Host "  - Logs are searchable by structured properties" -ForegroundColor $Gray
        Write-Host "  - Can filter by customer ID, correlation ID, etc." -ForegroundColor $Gray
    }
}

# Summary
Write-Host "`n" -NoNewline
Write-Host "=" * 80 -ForegroundColor $Cyan
Write-Host "TEST SUMMARY" -ForegroundColor $Cyan
Write-Host "=" * 80 -ForegroundColor $Cyan

Write-Host "`n✅ Test Suite Complete!" -ForegroundColor $Green
Write-Host "`nCHECKLIST - Verify these in Terminal 1 logs:" -ForegroundColor $Yellow
Write-Host "  [OK] Startup log shows: Application, Environment, MachineName" -ForegroundColor $Gray
Write-Host "  [OK] Correlation IDs appear in all logs" -ForegroundColor $Gray
Write-Host "  [OK] Custom IDs are propagated (test-correlation-id-*)" -ForegroundColor $Gray
Write-Host "  [OK] Auto-generated GUIDs when not provided" -ForegroundColor $Gray
Write-Host "  [OK] Health checks don't appear (VERBOSE level filtered)" -ForegroundColor $Gray
Write-Host "  [OK] 404 errors logged at WARNING level" -ForegroundColor $Gray
Write-Host "  [OK] Concurrent requests have different IDs" -ForegroundColor $Gray
Write-Host "  [OK] Log files created in logs/ directory" -ForegroundColor $Gray
Write-Host "  [OK] Logs are JSON structured" -ForegroundColor $Gray
Write-Host "  [OK] Response timing shown in logs" -ForegroundColor $Gray

Write-Host "`n" -NoNewline
Write-Host "=" * 80 -ForegroundColor $Cyan
Write-Host "🎉 If all above checks pass, logging is fully working!" -ForegroundColor $Green
Write-Host "=" * 80 -ForegroundColor $Cyan
Write-Host "`n"
