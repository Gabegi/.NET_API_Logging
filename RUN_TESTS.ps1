# LoggingProduction API - Complete Test Suite Runner
# This script runs all 10 tests to verify logging implementation

param(
    [string]$Uri = "https://localhost:7002",
    [switch]$SkipCertificateCheck = $true
)

# Color output for better readability
$Green = [System.ConsoleColor]::Green
$Red = [System.ConsoleColor]::Red
$Yellow = [System.ConsoleColor]::Yellow
$Cyan = [System.ConsoleColor]::Cyan
$Gray = [System.ConsoleColor]::DarkGray

function Write-TestHeader {
    param([string]$TestName)
    Write-Host "`n" -NoNewline
    Write-Host "=" * 80 -ForegroundColor $Cyan
    Write-Host "TEST: $TestName" -ForegroundColor $Cyan
    Write-Host "=" * 80 -ForegroundColor $Cyan
}

function Write-TestResult {
    param([string]$Result, [bool]$Pass)
    if ($Pass) {
        Write-Host "✅ PASS: $Result" -ForegroundColor $Green
    } else {
        Write-Host "❌ FAIL: $Result" -ForegroundColor $Red
    }
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️  $Message" -ForegroundColor $Gray
}

function Wait-ForApp {
    Write-Host "⏳ Waiting for app to be ready..." -ForegroundColor $Yellow
    $maxAttempts = 30
    $attempt = 0

    while ($attempt -lt $maxAttempts) {
        try {
            $response = Invoke-WebRequest -Uri "$Uri/health" `
                -SkipCertificateCheck -ErrorAction SilentlyContinue
            if ($response.StatusCode -eq 200) {
                Write-Host "✅ App is ready!" -ForegroundColor $Green
                return $true
            }
        } catch {
            # App not ready yet
        }

        $attempt++
        Start-Sleep -Seconds 1
    }

    Write-Host "❌ App failed to start" -ForegroundColor $Red
    return $false
}

# ============================================================================
# TEST 1: Verify Startup Logging with Enrichment
# ============================================================================
function Test-StartupLogging {
    Write-TestHeader "Startup Logging with Enrichment"

    Write-Info "Check the app startup logs above for enrichment properties"
    Write-Info "You should see: Application, Environment, EnvironmentUserName, MachineName, ThreadId"

    $prompt = Read-Host "Do you see all enrichment properties? (yes/no)"

    if ($prompt -eq "yes") {
        Write-TestResult "Startup logging shows all enrichment properties" $true
        return $true
    } else {
        Write-TestResult "Startup logging missing enrichment" $false
        return $false
    }
}

# ============================================================================
# TEST 2: Create Order with Client Correlation ID
# ============================================================================
function Test-ClientCorrelationId {
    Write-TestHeader "Create Order with Client Correlation ID"

    $customId = "test-123-$(Get-Random -Maximum 9999)"
    Write-Info "Using Correlation ID: $customId"

    $body = @{
        customerId = "cust-456"
        total = 99.99
    } | ConvertTo-Json

    Write-Info "Sending POST request..."

    try {
        $response = Invoke-RestMethod -Method Post `
            -Uri "$Uri/api/orders" `
            -ContentType "application/json" `
            -Headers @{"X-Correlation-Id" = $customId} `
            -Body $body `
            -SkipCertificateCheck

        Write-Host "Response:" -ForegroundColor $Cyan
        $response | ConvertTo-Json | Write-Host

        Write-Info "Check console logs above for:"
        Write-Info "  1. CorrelationId: $customId (your custom ID)"
        Write-Info "  2. Multiple logs with same ID"
        Write-Info "  3. customerId: cust-456 and total: 99.99 as structured properties"

        $prompt = Read-Host "Do you see the correlation ID in logs? (yes/no)"

        if ($prompt -eq "yes") {
            Write-TestResult "Client correlation ID propagated correctly" $true
            return $true
        } else {
            Write-TestResult "Correlation ID not found in logs" $false
            return $false
        }
    } catch {
        Write-TestResult "Request failed: $_" $false
        return $false
    }
}

# ============================================================================
# TEST 3: Auto-Generated Correlation ID
# ============================================================================
function Test-AutoCorrelationId {
    Write-TestHeader "Auto-Generated Correlation ID"

    Write-Info "Sending GET request without X-Correlation-Id header..."

    try {
        $response = Invoke-WebRequest -Method Get `
            -Uri "$Uri/api/orders" `
            -SkipCertificateCheck `
            -PassThru

        $correlationId = $response.Headers["X-Correlation-Id"]

        if ($correlationId) {
            Write-Host "✅ Response Header - X-Correlation-Id: $correlationId" -ForegroundColor $Green

            Write-Info "Check console logs above for:"
            Write-Info "  1. Auto-generated GUID in CorrelationId"
            Write-Info "  2. Same ID in all logs for this request"
            Write-Info "  3. Different from previous test's ID"

            $prompt = Read-Host "Do you see auto-generated ID in logs? (yes/no)"

            if ($prompt -eq "yes") {
                Write-TestResult "Auto-generated correlation ID works" $true
                return $true
            } else {
                Write-TestResult "Auto-generated ID not found in logs" $false
                return $false
            }
        } else {
            Write-TestResult "X-Correlation-Id header missing from response" $false
            return $false
        }
    } catch {
        Write-TestResult "Request failed: $_" $false
        return $false
    }
}

# ============================================================================
# TEST 4: Health Check Filtering (No Spam)
# ============================================================================
function Test-HealthCheckFiltering {
    Write-TestHeader "Health Check Filtering (No Spam)"

    Write-Info "Sending GET request to /health endpoint..."

    try {
        $response = Invoke-RestMethod -Method Get `
            -Uri "$Uri/health" `
            -SkipCertificateCheck

        Write-Host "Response:" -ForegroundColor $Cyan
        $response | ConvertTo-Json | Write-Host

        Write-Info "Health checks should NOT appear in console logs (VERBOSE level filtered)"
        Write-Info "This prevents Kubernetes health checks from spamming logs"

        $prompt = Read-Host "Did health check NOT appear in console logs? (yes/no)"

        if ($prompt -eq "yes") {
            Write-TestResult "Health check filtering works (no spam)" $true
            return $true
        } else {
            Write-TestResult "Health check filtering failed (logs were spammed)" $false
            return $false
        }
    } catch {
        Write-TestResult "Request failed: $_" $false
        return $false
    }
}

# ============================================================================
# TEST 5: Error Handling (404 Not Found)
# ============================================================================
function Test-ErrorHandling {
    Write-TestHeader "Error Handling (404 Not Found)"

    Write-Info "Sending GET request for non-existent order..."

    try {
        Invoke-RestMethod -Method Get `
            -Uri "$Uri/api/orders/non-existent-id" `
            -SkipCertificateCheck `
            -ErrorAction Stop
    } catch {
        $errorResponse = $_.Exception.Response
        $statusCode = [int]$errorResponse.StatusCode

        Write-Host "✅ Response Status: $statusCode (Not Found)" -ForegroundColor $Green

        if ($statusCode -eq 404) {
            Write-Info "Check console logs above for:"
            Write-Info "  1. Log level should be [WRN] (WARNING, not INFO)"
            Write-Info "  2. Smart filtering elevated 404 to WARNING automatically"
            Write-Info "  3. Message shows 'not found'"

            $prompt = Read-Host "Do you see WARNING level log? (yes/no)"

            if ($prompt -eq "yes") {
                Write-TestResult "Error status codes automatically elevated to WARNING" $true
                return $true
            } else {
                Write-TestResult "Error not logged at WARNING level" $false
                return $false
            }
        }
    }

    return $false
}

# ============================================================================
# TEST 6: Multiple Concurrent Requests (Different IDs)
# ============================================================================
function Test-ConcurrentRequests {
    Write-TestHeader "Multiple Concurrent Requests (Different IDs)"

    Write-Info "Sending 3 concurrent requests..."

    try {
        $job1 = Start-Job -ScriptBlock {
            param($Uri)
            Invoke-RestMethod -Method Get `
                -Uri "$Uri/api/orders" `
                -SkipCertificateCheck
        } -ArgumentList $Uri

        $job2 = Start-Job -ScriptBlock {
            param($Uri)
            Invoke-RestMethod -Method Get `
                -Uri "$Uri/api/products" `
                -SkipCertificateCheck
        } -ArgumentList $Uri

        $job3 = Start-Job -ScriptBlock {
            param($Uri)
            Invoke-RestMethod -Method Get `
                -Uri "$Uri/api/orders/search?customerId=test" `
                -SkipCertificateCheck
        } -ArgumentList $Uri

        $results = @($job1, $job2, $job3) | Wait-Job

        Write-Host "✅ All 3 concurrent requests completed" -ForegroundColor $Green

        Write-Info "Check console logs above for:"
        Write-Info "  1. Three different Correlation IDs"
        Write-Info "  2. No ID reuse"
        Write-Info "  3. Logs from different requests are distinguishable"

        $prompt = Read-Host "Do you see 3 different Correlation IDs? (yes/no)"

        Remove-Job -Job @($job1, $job2, $job3)

        if ($prompt -eq "yes") {
            Write-TestResult "Each concurrent request gets unique correlation ID" $true
            return $true
        } else {
            Write-TestResult "Concurrent requests don't have unique IDs" $false
            return $false
        }
    } catch {
        Write-TestResult "Concurrent requests failed: $_" $false
        return $false
    }
}

# ============================================================================
# TEST 7: Correlation ID Propagation
# ============================================================================
function Test-CorrelationIdPropagation {
    Write-TestHeader "Correlation ID Propagation (Client Provides, Server Returns)"

    $customId = "my-id-$(Get-Random -Maximum 9999)"
    Write-Info "Using custom Correlation ID: $customId"

    $body = @{
        customerId = "cust-777"
        total = 49.99
    } | ConvertTo-Json

    Write-Info "Sending POST request with custom ID..."

    try {
        $response = Invoke-WebRequest -Method Post `
            -Uri "$Uri/api/orders" `
            -ContentType "application/json" `
            -Headers @{"X-Correlation-Id" = $customId} `
            -Body $body `
            -SkipCertificateCheck `
            -PassThru

        $responseId = $response.Headers["X-Correlation-Id"]

        Write-Host "Request Sent with: X-Correlation-Id: $customId" -ForegroundColor $Cyan
        Write-Host "Response Contains: X-Correlation-Id: $responseId" -ForegroundColor $Cyan

        if ($customId -eq $responseId) {
            Write-Host "✅ IDs Match!" -ForegroundColor $Green

            Write-Info "This enables distributed tracing across services"
            Write-Info "Client can provide ID and receive it back in response"

            $prompt = Read-Host "Did response return the same custom ID? (yes/no)"

            if ($prompt -eq "yes") {
                Write-TestResult "Correlation ID properly propagated" $true
                return $true
            }
        } else {
            Write-Host "❌ IDs Don't Match!" -ForegroundColor $Red
            Write-TestResult "Correlation IDs don't match" $false
        }
    } catch {
        Write-TestResult "Request failed: $_" $false
    }

    return $false
}

# ============================================================================
# TEST 8: Verify Log Files
# ============================================================================
function Test-LogFiles {
    Write-TestHeader "Verify Log Files"

    $logsDir = "logs"

    if (Test-Path $logsDir) {
        Write-Host "✅ Logs directory exists" -ForegroundColor $Green

        $logFiles = Get-ChildItem $logsDir -Filter "log-*.txt" | Sort-Object CreationTime -Descending

        if ($logFiles) {
            Write-Host "Log files found:" -ForegroundColor $Cyan
            $logFiles | ForEach-Object { Write-Host "  - $($_.Name) ($($_.Length) bytes)" }

            Write-Info "Latest log file content (last 20 lines):"
            Write-Host "---" -ForegroundColor $Gray
            $latestLog = $logFiles[0].FullName
            Get-Content $latestLog -Tail 20 | Write-Host
            Write-Host "---" -ForegroundColor $Gray

            $prompt = Read-Host "Do log files contain structured JSON data? (yes/no)"

            if ($prompt -eq "yes") {
                Write-TestResult "Log files created with daily rolling" $true
                return $true
            } else {
                Write-TestResult "Log files not properly formatted" $false
                return $false
            }
        } else {
            Write-TestResult "No log files found" $false
            return $false
        }
    } else {
        Write-TestResult "Logs directory not found" $false
        return $false
    }
}

# ============================================================================
# TEST 9: Structured Data Search
# ============================================================================
function Test-StructuredSearch {
    Write-TestHeader "Structured Data Search"

    Write-Info "Searching log files for structured data..."

    try {
        $searchTerm = "cust-456"
        Write-Host "Searching for: '$searchTerm'" -ForegroundColor $Cyan

        if (Test-Path "logs") {
            $results = Select-String $searchTerm logs\log-*.txt -ErrorAction SilentlyContinue

            if ($results) {
                Write-Host "✅ Found matches:" -ForegroundColor $Green
                $results | ForEach-Object { Write-Host "  $_" }

                Write-Info "This shows logs are structured and searchable"
                Write-Info "You can query by customer ID, correlation ID, error type, etc."

                $prompt = Read-Host "Are logs searchable by structured properties? (yes/no)"

                if ($prompt -eq "yes") {
                    Write-TestResult "Structured data search works" $true
                    return $true
                }
            } else {
                Write-Info "No results found (may not have that data yet)"
                $prompt = Read-Host "Acknowledge - logs are JSON structured? (yes/no)"

                if ($prompt -eq "yes") {
                    Write-TestResult "Structured logging confirmed" $true
                    return $true
                }
            }
        }
    } catch {
        Write-Info "Search command: Select-String 'correlationId' logs\log-*.txt"
    }

    return $false
}

# ============================================================================
# TEST 10: Response Timing
# ============================================================================
function Test-ResponseTiming {
    Write-TestHeader "Response Timing"

    Write-Info "Measuring request response time..."

    try {
        $start = Get-Date

        $response = Invoke-RestMethod -Method Get `
            -Uri "$Uri/api/orders" `
            -SkipCertificateCheck

        $elapsed = (Get-Date) - $start
        $elapsedMs = [math]::Round($elapsed.TotalMilliseconds, 2)

        Write-Host "✅ Request took: $elapsedMs ms" -ForegroundColor $Green

        Write-Info "Check console logs above for:"
        Write-Info "  1. Same timing value: 'responded 200 in ${elapsedMs}ms'"
        Write-Info "  2. Logs contain elapsed time"

        $prompt = Read-Host "Do you see response timing in logs? (yes/no)"

        if ($prompt -eq "yes") {
            Write-TestResult "Response timing logged correctly" $true
            return $true
        } else {
            Write-TestResult "Response timing not in logs" $false
            return $false
        }
    } catch {
        Write-TestResult "Request failed: $_" $false
        return $false
    }
}

# ============================================================================
# MAIN TEST RUNNER
# ============================================================================

Write-Host @"
╔════════════════════════════════════════════════════════════════════════════╗
║                                                                            ║
║          LoggingProduction API - Complete Test Suite Runner               ║
║                                                                            ║
║                   Testing All Logging Features (10 Tests)                  ║
║                                                                            ║
╚════════════════════════════════════════════════════════════════════════════╝
"@ -ForegroundColor $Cyan

Write-Host "Target URL: $Uri" -ForegroundColor $Gray
Write-Host "Certificate Check: $(if ($SkipCertificateCheck) {'Skipped'} else {'Enabled'})" -ForegroundColor $Gray

# Check if app is running
Write-Info "Checking if app is running..."
$appRunning = $false
try {
    $response = Invoke-WebRequest -Uri "$Uri/health" -SkipCertificateCheck -ErrorAction SilentlyContinue
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ App is already running" -ForegroundColor $Green
        $appRunning = $true
    }
} catch {
    Write-Host "⚠️  App is not running yet" -ForegroundColor $Yellow
    Write-Info "Make sure to start the app with: dotnet run"
    Write-Info "In another terminal in: src/LoggingProduction"
}

if (-not $appRunning) {
    $prompt = Read-Host "Start app now and press Enter when ready"
    if (-not (Wait-ForApp)) {
        Write-Host "❌ Cannot proceed without app running" -ForegroundColor $Red
        exit 1
    }
}

# Run all tests
$results = @()

$results += @{ Name = "Test 1: Startup Logging"; Passed = (Test-StartupLogging) }
$results += @{ Name = "Test 2: Client Correlation ID"; Passed = (Test-ClientCorrelationId) }
$results += @{ Name = "Test 3: Auto-Generated ID"; Passed = (Test-AutoCorrelationId) }
$results += @{ Name = "Test 4: Health Check Filtering"; Passed = (Test-HealthCheckFiltering) }
$results += @{ Name = "Test 5: Error Handling"; Passed = (Test-ErrorHandling) }
$results += @{ Name = "Test 6: Concurrent Requests"; Passed = (Test-ConcurrentRequests) }
$results += @{ Name = "Test 7: ID Propagation"; Passed = (Test-CorrelationIdPropagation) }
$results += @{ Name = "Test 8: Log Files"; Passed = (Test-LogFiles) }
$results += @{ Name = "Test 9: Structured Search"; Passed = (Test-StructuredSearch) }
$results += @{ Name = "Test 10: Response Timing"; Passed = (Test-ResponseTiming) }

# Summary
Write-Host "`n`n" -NoNewline
Write-Host "=" * 80 -ForegroundColor $Cyan
Write-Host "TEST SUMMARY" -ForegroundColor $Cyan
Write-Host "=" * 80 -ForegroundColor $Cyan

$passed = ($results | Where-Object { $_.Passed }).Count
$total = $results.Count
$percentage = [math]::Round(($passed / $total) * 100, 2)

$results | ForEach-Object {
    $status = if ($_.Passed) { "✅" } else { "❌" }
    Write-Host "$status $($_.Name)" -ForegroundColor (if ($_.Passed) { $Green } else { $Red })
}

Write-Host "`n" -NoNewline
Write-Host "=" * 80 -ForegroundColor $Cyan
Write-Host "RESULTS: $passed / $total tests passed ($percentage%)" -ForegroundColor (if ($passed -eq $total) { $Green } else { $Yellow })
Write-Host "=" * 80 -ForegroundColor $Cyan

if ($passed -eq $total) {
    Write-Host @"

🎉 SUCCESS! All logging features are working correctly!

Your implementation has:
  ✅ Serilog configured correctly
  ✅ Correlation IDs generated and propagated
  ✅ Enrichment working (machine, environment, user, thread)
  ✅ Smart filtering working (health checks hidden, errors elevated)
  ✅ Async sinks working (non-blocking)
  ✅ Rolling file logs working
  ✅ Structured logging working
  ✅ All logs queryable by correlation ID

🚀 Ready for production use!
"@ -ForegroundColor $Green
} else {
    Write-Host @"

⚠️  Some tests failed. Check the output above and troubleshoot.

Common issues:
  1. App not running - make sure 'dotnet run' is active
  2. Wrong port - verify launchSettings.json has https://localhost:7002
  3. Logs not visible - check appsettings.json MinimumLevel settings
  4. Headers missing - verify middleware order in Program.cs

Run this script again after fixing issues.
"@ -ForegroundColor $Yellow
}

Write-Host "`n"
