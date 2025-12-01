# .NET_API_Logging

Production-ready .NET 9 logging solution with Serilog, Elasticsearch, and Kibana integration.

## Quick Start

### Prerequisites
- Docker & Docker Compose
- .NET 9 SDK
- Port 5022, 5601 (Kibana), 9200 (Elasticsearch) available

### Start Services

**1. Start Elasticsearch & Kibana:**
```bash
docker-compose up -d
```

**2. Start the API:**
```bash
cd src/LoggingProduction
dotnet run --no-build
```

The API will be available at **http://localhost:5022**

**3. Access Kibana:**
- URL: http://localhost:5601
- Create data view: `logstash-*`
- View logs with correlation IDs

## API Endpoints

### Health Check
```bash
GET http://localhost:5022/health
```
Response:
```json
{"status":"healthy","timestamp":"2025-12-01T14:50:30Z"}
```

### Products

#### List All Products
```bash
curl http://localhost:5022/api/products
```

#### Get Product by ID
```bash
curl http://localhost:5022/api/products/{id}
```

#### Create Product
```bash
curl -X POST http://localhost:5022/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Laptop","price":999.99,"sku":"LAPTOP-001"}'
```

#### Update Product
```bash
curl -X PUT http://localhost:5022/api/products/{id} \
  -H "Content-Type: application/json" \
  -d '{"name":"Updated Laptop","price":1299.99,"sku":"LAPTOP-001"}'
```

#### Delete Product
```bash
curl -X DELETE http://localhost:5022/api/products/{id}
```

### Orders

#### List All Orders
```bash
curl http://localhost:5022/api/orders
```

#### Get Order by ID
```bash
curl http://localhost:5022/api/orders/{id}
```

#### Create Order
```bash
curl -X POST http://localhost:5022/api/orders \
  -H "Content-Type: application/json" \
  -d '{"customerId":"CUST-001","productId":"PROD-001","quantity":5}'
```

#### Update Order
```bash
curl -X PUT http://localhost:5022/api/orders/{id} \
  -H "Content-Type: application/json" \
  -d '{"customerId":"CUST-001","productId":"PROD-001","quantity":10}'
```

#### Delete Order
```bash
curl -X DELETE http://localhost:5022/api/orders/{id}
```

## Request Headers

All requests can include a correlation ID for distributed tracing:

```bash
curl -H "X-Correlation-Id: my-trace-id" http://localhost:5022/api/products
```

The API will automatically:
- Generate a correlation ID if not provided
- Include it in all logs
- Return it in the response header `X-Correlation-Id`

## Logging Features

- **Structured Logging**: JSON format for all logs
- **Multiple Sinks**: Console, File (daily rolling), Elasticsearch
- **Correlation IDs**: Request tracing across all layers
- **Smart Filtering**: Health checks (Verbose), Errors (Error), Slow requests (Warning)
- **Enrichment**: Client IP, User Agent, Machine Name, Thread ID, Environment

## Architecture

```
LoggingProduction/
├── API/
│   ├── Endpoints/        # Minimal API route handlers
│   ├── Middleware/       # CorrelationIdMiddleware
│   └── ...
├── Data/
│   ├── Models/          # Entity models
│   └── Repositories/    # In-memory repository pattern
├── Services/            # Business logic (IProductService, IOrderService)
├── LoggingExtensions/   # Serilog configuration
└── Program.cs           # Application entry point
```

## Development

Build and run with detailed output:
```bash
cd src/LoggingProduction
dotnet build
dotnet run
```

Build without running:
```bash
dotnet build
```

Run without rebuilding:
```bash
dotnet run --no-build
```

## Best Practices Demonstrated

- ✅ Minimal API pattern with method injection
- ✅ Service layer abstraction
- ✅ Repository pattern (in-memory)
- ✅ Structured logging with Serilog
- ✅ Correlation ID propagation
- ✅ Async/await throughout
- ✅ Proper error handling
- ✅ Centralized log aggregation (Elasticsearch)
- ✅ Log visualization (Kibana)
- ✅ Clean separation of concerns
