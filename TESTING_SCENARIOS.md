# Manual Testing Scenarios - Logging Production API

This document describes manual test scenarios to demonstrate the logging capabilities of the LoggingProduction API. Each scenario includes the request commands and expected log output.

---

## Setup

1. **Start the application**:
   ```bash
   cd src/LoggingProduction
   dotnet run
   ```

2. **Monitor logs in real-time**:
   - Logs are written to: `logs/log-YYYY-MM-DD.txt` (file)
   - Logs also appear in: Console output (terminal)

3. **Use curl or Postman** to make requests (examples use curl):

---

## Scenario 1: Successful GET Request - Get All Products

### Test Request:
```bash
curl -X GET http://localhost:5000/api/products \
  -H "User-Agent: curl/7.68.0" \
  -v
```

### Expected Console Log Output:
```
[14:23:45 INF] HTTP GET /api/products responded 200 in 15ms {"ClientIP":"127.0.0.1","UserAgent":"curl/7.68.0","RequestHost":"localhost:5000","RequestMethod":"GET","RequestPath":"/api/products","ResponseStatusCode":200}
[14:23:45 INF] Retrieving all products {"SourceContext":"LoggingProduction.Services.ProductService","Application":"LoggingProduction","Environment":"Development","EnvironmentUserName":"lelyg","MachineName":"DESKTOP-ABC","ThreadId":5}
```

### What We Learn:
✅ Request received (GET /api/products)
✅ Response time (15ms - very fast)
✅ Client context captured (IP: 127.0.0.1, User-Agent: curl)
✅ Business logic logging (service logging retrieval)
✅ Timestamp, environment, machine name all included

---

## Scenario 2: Successful POST - Create Product (With Simulated Delay)

### Test Request:
```bash
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -H "User-Agent: Postman/10.0" \
  -d '{
    "name": "Laptop Pro",
    "description": "High-performance laptop",
    "price": 1299.99,
    "stockQuantity": 50
  }' \
  -v
```

### Expected Console Log Output:
```
[14:25:12 INF] HTTP POST /api/products responded 201 in 75ms {"ClientIP":"127.0.0.1","UserAgent":"Postman/10.0","RequestHost":"localhost:5000","RequestMethod":"POST","RequestPath":"/api/products","ResponseStatusCode":201}
[14:25:12 INF] Creating product with name Laptop Pro and price 1299.99 {"SourceContext":"LoggingProduction.Services.ProductService","Application":"LoggingProduction"}
[14:25:12 INF] Product prod-abc-123 created successfully {"SourceContext":"LoggingProduction.Services.ProductService"}
```

### What We Learn:
✅ POST request tracked (201 Created)
✅ Response time captured (75ms includes simulated delay)
✅ Structured data: ProductName, Price as separate fields (searchable!)
✅ Product ID generated and tracked
✅ Multiple logs from same request all linked via correlation ID

---

## Scenario 3: Product Not Found - 404 Error

### Test Request:
```bash
curl -X GET http://localhost:5000/api/products/non-existent-id \
  -H "User-Agent: iPhone Safari" \
  -v
```

### Expected Console Log Output:
```
[14:26:30 WRN] HTTP GET /api/products/non-existent-id responded 404 in 8ms {"ClientIP":"127.0.0.1","UserAgent":"iPhone Safari","RequestHost":"localhost:5000","RequestMethod":"GET","RequestPath":"/api/products/non-existent-id","ResponseStatusCode":404}
[14:26:30 INF] Retrieving product non-existent-id {"SourceContext":"LoggingProduction.Services.ProductService"}
[14:26:30 WRN] Product non-existent-id not found {"SourceContext":"LoggingProduction.Services.ProductService"}
```

### What We Learn:
✅ HTTP error (404) → Log level upgraded to WARNING (automatic!)
✅ Business logic also logged warning (product not found)
✅ User-Agent shows iPhone Safari (helpful for debugging)
✅ No exception, just validation logs

---

## Scenario 4: Health Check - No Spam (Verbose Level Hidden)

### Test Request:
```bash
curl -X GET http://localhost:5000/health \
  -v
```

### Expected Console Log Output:
```
(No output - health checks are logged at VERBOSE level which is filtered out)
```

### File Log Output (if you set min level to Verbose):
```
2025-01-28 14:27:45.123 +00:00 [VRB] HTTP GET /health responded 200 in 2ms {...}
```

### What We Learn:
✅ Health checks don't spam your logs
✅ They're still logged to file at VERBOSE level
✅ Can be enabled for debugging if needed
✅ Kubernetes won't flood your logs with health probes

---

## Scenario 5: Slow Request - Simulated (>1 second)

### Test Request:
```bash
# Create product (simulates 50-200ms delay in repository)
# Run it multiple times - one might be >1 second
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Slow Product",
    "description": "Testing slow requests",
    "price": 99.99,
    "stockQuantity": 10
  }' \
  -v
```

### Expected Console Log Output (when request > 1000ms):
```
[14:28:50 WRN] HTTP POST /api/products responded 201 in 1234ms {"ClientIP":"127.0.0.1","UserAgent":"curl","RequestHost":"localhost:5000","RequestMethod":"POST","ResponseStatusCode":201}
```

### What We Learn:
✅ Slow requests automatically flagged as WARNING
✅ Response time clearly visible (1234ms)
✅ Performance issues stand out in logs
✅ Can set alerts based on WARNING level logs

---

## Scenario 6: Order Creation - Demonstrating Correlation ID

### Test Request 1:
```bash
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "cust-12345",
    "total": 2599.98
  }' \
  -v
```

### Expected Console Log Output:
```
[14:30:15 INF] HTTP POST /api/orders responded 201 in 45ms {"CorrelationId":"550e8400-e29b-41d4-a716-446655440000","ClientIP":"127.0.0.1","RequestHost":"localhost:5000","ResponseStatusCode":201}
[14:30:15 INF] Creating order for customer cust-12345 with total 2599.98 {"CorrelationId":"550e8400-e29b-41d4-a716-446655440000","SourceContext":"LoggingProduction.Services.OrderService"}
[14:30:15 INF] Order ord-xyz-789 created successfully with status Pending {"CorrelationId":"550e8400-e29b-41d4-a716-446655440000"}
```

### What We Learn:
✅ **Correlation ID** (550e8400-...) appears in ALL logs
✅ One ID = One Request's entire journey
✅ Can grep for this ID to see full request trace
✅ Appears in response headers (check `-v` output):
   ```
   X-Correlation-Id: 550e8400-e29b-41d4-a716-446655440000
   ```

---

## Scenario 7: Search Orders by Customer

### Test Request:
```bash
curl -X GET "http://localhost:5000/api/orders/search?customerId=cust-12345" \
  -H "User-Agent: Mobile App v2.1" \
  -v
```

### Expected Console Log Output:
```
[14:31:20 INF] HTTP GET /api/orders/search?customerId=cust-12345 responded 200 in 12ms {"ClientIP":"127.0.0.1","UserAgent":"Mobile App v2.1","RequestHost":"localhost:5000"}
[14:31:20 INF] Searching orders with customerId filter: cust-12345 {"SourceContext":"LoggingProduction.Services.OrderService"}
```

### What We Learn:
✅ Query parameters included in logging
✅ Mobile app identified by User-Agent
✅ Customer ID tracked as structured property
✅ Search filtering logged at business level

---

## Scenario 8: Server Error - Timeout Exception (10% chance on create)

### Test Request (run multiple times - one should fail):
```bash
for i in {1..10}; do
  curl -X POST http://localhost:5000/api/products \
    -H "Content-Type: application/json" \
    -d '{
      "name": "Product '$i'",
      "description": "Testing",
      "price": 99.99,
      "stockQuantity": 10
    }' \
    -w "Status: %{http_code}\n" \
    -o /dev/null
done
```

### Expected Console Log Output (when timeout occurs):
```
[14:32:45 ERR] HTTP POST /api/products responded 503 in 185ms {"ClientIP":"127.0.0.1","RequestHost":"localhost:5000","ResponseStatusCode":503}
[14:32:45 ERR] Creating product with name Product 7 and price 99.99 {"SourceContext":"LoggingProduction.Services.ProductService"}
[14:32:45 ERR] Failed to create product prod-xyz due to timeout Exception: System.TimeoutException: Operation timed out
  at LoggingProduction.Data.Repositories.InMemoryProductRepository.CreateAsync(Product product)
```

### What We Learn:
✅ Error responses (500+) → ERROR log level
✅ Full exception details logged
✅ Stack trace included
✅ Error operation clearly visible
✅ Time to failure tracked (185ms)

---

## Scenario 9: Multiple Requests - Correlation ID Tracking

### Test Multiple Requests Quickly:
```bash
# Request 1
curl -X GET http://localhost:5000/api/products 2>/dev/null &

# Request 2
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{"customerId":"cust-999","total":50}' 2>/dev/null &

# Request 3
curl -X GET http://localhost:5000/api/orders/search?customerId=cust-999 2>/dev/null &

wait
```

### Expected Console Log Output:
```
[14:33:10 INF] HTTP GET /api/products responded 200 in 8ms {"CorrelationId":"abc-123...","ClientIP":"127.0.0.1"}
[14:33:10 INF] Retrieving all products {"CorrelationId":"abc-123..."}
[14:33:10 INF] HTTP POST /api/orders responded 201 in 42ms {"CorrelationId":"def-456...","ClientIP":"127.0.0.1"}
[14:33:10 INF] Creating order for customer cust-999 with total 50 {"CorrelationId":"def-456..."}
[14:33:10 INF] HTTP GET /api/orders/search?customerId=cust-999 responded 200 in 9ms {"CorrelationId":"ghi-789..."}
```

### What We Learn:
✅ **Each request has its own Correlation ID**
✅ Logs from different requests don't interleave
✅ Can grep for specific ID: `grep "abc-123" logs/log-*.txt`
✅ Concurrent requests isolated in logs

---

## Scenario 10: Update Product - Demonstrating Structured Data

### Test Request:
```bash
# First, get a product ID
PRODUCT_ID=$(curl -s http://localhost:5000/api/products | jq '.[0].id' -r)

# Then update it
curl -X PUT http://localhost:5000/api/products/$PRODUCT_ID \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Updated Laptop Pro",
    "description": "With new features",
    "price": 1499.99,
    "stockQuantity": 25
  }' \
  -v
```

### Expected Console Log Output:
```
[14:34:50 INF] HTTP PUT /api/products/prod-123-abc responded 200 in 28ms {"ClientIP":"127.0.0.1","RequestHost":"localhost:5000"}
[14:34:50 INF] Updating product prod-123-abc with name Updated Laptop Pro {"SourceContext":"LoggingProduction.Services.ProductService"}
[14:34:50 INF] Product prod-123-abc updated successfully {"SourceContext":"LoggingProduction.Services.ProductService"}
```

### What We Learn:
✅ Structured logging of update operations
✅ Product ID and name as separate searchable fields
✅ Update tracked from endpoint through service

---

## Log Analysis Examples

### Find all errors in the last hour:
```bash
grep "\[ERR\]" logs/log-2025-01-28.txt
```

### Find all requests for a specific customer:
```bash
grep "cust-12345" logs/log-2025-01-28.txt
```

### Find all slow requests (>1 second):
```bash
grep "\[WRN\].*HTTP" logs/log-2025-01-28.txt | grep -E "[0-9]{4,}ms"
```

### Trace a single request (by Correlation ID):
```bash
grep "550e8400-e29b-41d4-a716-446655440000" logs/log-2025-01-28.txt
```

### Count requests by endpoint:
```bash
grep "HTTP" logs/log-2025-01-28.txt | grep -o "/api/[a-z]*" | sort | uniq -c
```

### Count errors by type:
```bash
grep "\[ERR\]" logs/log-2025-01-28.txt | grep -o "Exception: [^}]*" | sort | uniq -c
```

---

## Summary of Log Capabilities Demonstrated

| Feature | Demonstrated | Example |
|---------|--------------|---------|
| **Request Logging** | ✅ Scenario 1 | GET /api/products in 15ms |
| **Response Tracking** | ✅ Scenario 2 | 201 Created for POST |
| **Error Handling** | ✅ Scenario 3 | 404 Not Found warnings |
| **Health Check Filtering** | ✅ Scenario 4 | Verbose level suppression |
| **Performance Monitoring** | ✅ Scenario 5 | Slow request warnings >1s |
| **Correlation ID** | ✅ Scenario 6 | Unique ID per request |
| **Structured Data** | ✅ Scenario 7 | CustomerID, Total as fields |
| **Exception Details** | ✅ Scenario 8 | TimeoutException with stack |
| **Concurrent Requests** | ✅ Scenario 9 | Multiple IDs, no interleaving |
| **Audit Trail** | ✅ Scenario 10 | Product updates tracked |

---

## Log Output Example (Raw File Format)

```
2025-01-28 14:23:45.123 +00:00 [INF] HTTP GET /api/products responded 200 in 15ms {"ClientIP":"127.0.0.1","UserAgent":"curl/7.68.0","RequestHost":"localhost:5000","RequestMethod":"GET","RequestPath":"/api/products","ResponseStatusCode":200,"Application":"LoggingProduction","Environment":"Development","EnvironmentUserName":"lelyg","MachineName":"DESKTOP-ABC123","ThreadId":5}
2025-01-28 14:23:45.124 +00:00 [INF] Retrieving all products {"SourceContext":"LoggingProduction.Services.ProductService","Application":"LoggingProduction","Environment":"Development","EnvironmentUserName":"lelyg","MachineName":"DESKTOP-ABC123","ThreadId":5}
```

This is JSON-compatible and can be ingested by:
- ELK Stack (Elasticsearch, Logstash, Kibana)
- Splunk
- Azure Application Insights
- Datadog
- Any JSON log aggregation platform

---

## Cleanup

After testing, check the logs directory:
```bash
ls -lah logs/
```

Logs older than 7 days are automatically deleted by the rolling file sink configuration.
