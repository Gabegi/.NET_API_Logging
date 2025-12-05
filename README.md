# .NET 9 Production Logging Solution

Production-ready .NET 9 API with structured logging, PII masking, distributed tracing, and Elasticsearch integration.

## Features

- ✅ **Structured Logging** - JSON format with Serilog
- ✅ **PII Masking** - GDPR/PCI-DSS compliant (emails, credit cards, etc.)
- ✅ **Distributed Tracing** - OpenTelemetry with correlation IDs
- ✅ **Source Generators** - High-performance logging (3x faster)
- ✅ **Multiple Sinks** - Console, File (rolling), Elasticsearch
- ✅ **Smart Filtering** - Health checks hidden, errors auto-elevated
- ✅ **Enrichment** - Client IP, User Agent, Machine, Thread, Environment

## Quick Start

### Prerequisites
- .NET 9 SDK
- Docker & Docker Compose (for Elasticsearch/Kibana)

### 1. Start Elasticsearch & Kibana (Optional)
```bash
docker-compose up -d
```

### 2. Run the API
```bash
cd src/LoggingProduction
dotnet run
```

API: **http://localhost:5022**

### 3. Run Tests
```powershell
.\test-api.ps1
```

## API Endpoints

### Products
```bash
# List all
GET /api/products

# Get by ID
GET /api/products/{id}

# Create
POST /api/products
Content-Type: application/json
{"name":"Laptop","price":999.99,"sku":"LAPTOP-001"}

# Update
PUT /api/products/{id}
{"name":"Updated","price":1299.99,"sku":"LAPTOP-001"}

# Delete
DELETE /api/products/{id}
```

### Orders
```bash
# List all
GET /api/orders

# Get by ID
GET /api/orders/{id}

# Create
POST /api/orders
{"customerId":"CUST-001","productId":"PROD-001","quantity":5}

# Update
PUT /api/orders/{id}
{"customerId":"CUST-001","productId":"PROD-001","quantity":10}

# Delete
DELETE /api/orders/{id}
```

### Health Check
```bash
GET /health
# Returns: {"status":"healthy","timestamp":"2025-12-05T..."}
```

## Correlation IDs

Add correlation ID for distributed tracing:

```bash
curl -H "X-Correlation-ID: my-trace-id" http://localhost:5022/api/products
```

The API will:
- Auto-generate ID if not provided
- Include ID in all logs
- Return ID in response header

## PII Masking

Automatically masks sensitive data in logs:

| PII Type | Example | Masked Output |
|----------|---------|---------------|
| Email | john@example.com | ***MASKED*** |
| Credit Card | 4532-1234-5678-9010 | ****-****-****-9010 |
| Phone | 555-123-4567 | ***-***-4567 |
| Password | myPassword123 | ***MASKED*** |

**Example:**
```bash
# Request with email
POST /api/orders
{"customerId":"john.doe@example.com","items":[]}

# Log output (email masked)
Creating order for customer ***MASKED*** with total 0
```

## Elasticsearch & Kibana Setup

### Start Services
```bash
docker-compose up -d
```

### Access Kibana
1. Open: **http://localhost:5601**
2. Create data view: `logstash-*`
3. Go to **Discover** to view logs

### Search Logs
```
# By correlation ID
CorrelationId: "my-trace-id"

# By customer
customerId: "CUST-001"

# Errors only
Level: "Error"

# Slow requests
ElapsedMilliseconds: >= 1000
```

### Stop Services
```bash
# Stop but keep data
docker-compose stop

# Stop and delete everything
docker-compose down -v
```

## Configuration

### Development (`appsettings.Development.json`)
- Console + File sinks
- 7-day log retention
- Verbose output with properties

### Production (`appsettings.Production.json`)
- Console + File + Elasticsearch
- 30-day retention, 100MB file limit
- Monthly Elasticsearch indices
- Offline buffering enabled

## Architecture

```
LoggingProduction/
├── API/
│   ├── Endpoints/          # Minimal API routes
│   └── Middleware/         # CorrelationIdMiddleware
├── Data/
│   ├── Models/            # Product, Order entities
│   └── Repositories/      # In-memory storage
├── Services/              # Business logic
├── Telemetry/             # Source-generated loggers
├── LoggingExtensions/     # Serilog, OpenTelemetry config
└── Program.cs
```

## Logging Features

### Smart Log Levels
- Health checks → Verbose (hidden)
- Normal requests → Information
- Client errors (4xx) → Warning
- Server errors (5xx) → Error
- Slow requests (>1s) → Warning

### Log Enrichment
Every log includes:
- `CorrelationId` - Request tracing
- `ClientIP` - Client address
- `UserAgent` - Client browser/tool
- `MachineName` - Server name
- `Environment` - Dev/Prod
- `ThreadId` - Thread number
- `Application` - "LoggingProduction"

### Source-Generated Logging
Uses `[LoggerMessage]` attributes for high performance:

```csharp
[LoggerMessage(Level = LogLevel.Information,
    Message = "Creating product with name {ProductName}")]
public static partial void LogCreatingProduct(ILogger logger, string productName);
```

**Performance:** 3x faster than manual logging, zero allocations.

## OpenTelemetry Tracing

Distributed tracing with step-by-step timing:

```
Activity.TraceId:    624bc726a90c58eb5414aa22319ac7d5
Activity.SpanId:     c9453b92aea51cab
Activity.DisplayName: POST /api/products/
Activity.Duration:   00:00:00.6670350
Activity.Tags:
  - http.response.status_code: 201
  - server.address: localhost
  - server.port: 5022
```

**Environment-based export:**
- Development: Console (for debugging)
- Production: OTLP to Elastic APM (port 4317)

## Testing

Run the test suite:

```powershell
.\test-api.ps1
```

Tests include:
1. Product creation with PII
2. Order creation (email masking test)
3. GET requests with auto-generated correlation IDs
4. Concurrent requests

Check logs for:
- All requests have correlation IDs
- Emails masked as `***MASKED***`
- Structured JSON logging
- OpenTelemetry traces with timing

## Production Readiness

### Security
- ✅ PII masking (GDPR/PCI-DSS compliant)
- ✅ No sensitive data in logs
- ✅ Correlation IDs for audit trails

### Performance
- ✅ Async sinks (non-blocking)
- ✅ Source-generated logging (3x faster)
- ✅ Batch processing for Elasticsearch
- ✅ File size limits and rotation

### Observability
- ✅ Structured logging (queryable)
- ✅ Distributed tracing
- ✅ Centralized log aggregation
- ✅ Real-time monitoring in Kibana

### Scalability
- ✅ Daily rolling files
- ✅ Monthly Elasticsearch indices
- ✅ Auto-cleanup (7-30 day retention)
- ✅ Buffering for offline resilience

## Best Practices Demonstrated

- Minimal API with method injection
- Service layer abstraction
- Repository pattern (in-memory)
- Async/await throughout
- Clean separation of concerns
- Environment-specific configuration
- Source generators for performance
- PII protection by design

## Troubleshooting

### App Won't Start
```bash
# Check port 5022 is available
netstat -ano | findstr :5022

# Rebuild
dotnet clean
dotnet build
```

### No Logs in Kibana
```bash
# Check Elasticsearch is running
curl http://localhost:9200

# Check if indices created
curl http://localhost:9200/logstash-*/_stats

# Restart containers
docker-compose restart
```

### PII Not Masked
Check logs for `***MASKED***`. If not appearing:
- Verify `Serilog.Enrichers.Sensitive` package installed
- Check `SerilogConfiguration.cs` has masking enricher
- Rebuild: `dotnet build`

## Resources

- [Serilog Documentation](https://serilog.net/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/net/)
- [Elasticsearch Guide](https://www.elastic.co/guide/en/elasticsearch/reference/current/index.html)
- [Kibana User Guide](https://www.elastic.co/guide/en/kibana/current/index.html)

## License

MIT
