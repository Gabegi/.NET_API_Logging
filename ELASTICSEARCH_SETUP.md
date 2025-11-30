# Elasticsearch + Kibana Setup Guide

## Overview

This guide shows how to set up Elasticsearch and Kibana for log aggregation and visualization in the LoggingProduction API.

**Benefits:**
- Centralized log storage
- Real-time log aggregation
- Full-text search across all logs
- Custom dashboards and visualizations
- Correlation ID tracing
- Performance analytics

---

## Architecture

```
LoggingProduction API
    ↓
  Serilog
    ├─→ Console (terminal)
    ├─→ File (logs/log-*.txt)
    └─→ Elasticsearch Sink (http://localhost:9200)
           ↓
    Elasticsearch Cluster
           ↓
    Kibana Dashboard
    (http://localhost:5601)
```

---

## Prerequisites

- Docker & Docker Compose installed
- 2GB RAM available for Elasticsearch
- Port 9200 (Elasticsearch) and 5601 (Kibana) available

---

## Step 1: Install Serilog Packages

**Already done!** The following packages are installed:
```
✅ Serilog.Sinks.Elasticsearch (v10.0.0)
✅ Serilog.Formatting.Elasticsearch (v10.0.0)
```

Verify in `LoggingProduction.csproj`:
```xml
<PackageReference Include="Serilog.Sinks.Elasticsearch" Version="10.0.0" />
<PackageReference Include="Serilog.Formatting.Elasticsearch" Version="10.0.0" />
```

---

## Step 2: Start Elasticsearch & Kibana

### Option A: Using Docker Compose (Recommended)

```bash
# Navigate to the repository root
cd C:\Users\lelyg\Desktop\code\.NET_API_Logging

# Start Elasticsearch and Kibana
docker-compose up -d

# Check status
docker-compose ps
docker-compose logs elasticsearch
docker-compose logs kibana
```

### Option B: Using Docker CLI

```bash
# Start Elasticsearch
docker run -d \
  --name elasticsearch \
  -e discovery.type=single-node \
  -e xpack.security.enabled=false \
  -e "ES_JAVA_OPTS=-Xms512m -Xmx512m" \
  -p 9200:9200 \
  docker.elastic.co/elasticsearch/elasticsearch:8.11.0

# Start Kibana
docker run -d \
  --name kibana \
  -e ELASTICSEARCH_HOSTS=http://elasticsearch:9200 \
  -p 5601:5601 \
  --link elasticsearch:elasticsearch \
  docker.elastic.co/kibana/kibana:8.11.0
```

### Verify Services Are Running

```bash
# Check Elasticsearch
curl http://localhost:9200

# Expected output:
# {
#   "name": "...",
#   "version": {
#     "number": "8.11.0",
#     ...
#   }
# }

# Check Kibana
curl http://localhost:5601/api/status

# Expected output:
# {"statusCode":200,...}
```

---

## Step 3: Verify Elasticsearch Configuration

The following is already configured in `Program.cs`:

```csharp
.WriteTo.Async(a => a.Elasticsearch(
    new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
    {
        // Daily indices: logstash-2025.11.28
        IndexFormat = "logstash-{0:yyyy.MM.dd}",

        // Batch logs for better performance
        BatchPostingLimit = 100,
        Period = TimeSpan.FromSeconds(5),

        // Auto-create index mappings
        AutoRegisterTemplate = true,
        TemplateName = "logstash-template"
    }))
```

---

## Step 4: Start the Application

```bash
cd src/LoggingProduction
dotnet run
```

Watch the console for:
```
[HH:MM:SS INF] Starting application {...}
```

---

## Step 5: Generate Log Data

Run the test suite to generate logs:

```bash
# In another terminal, navigate to repo root
cd C:\Users\lelyg\Desktop\code\.NET_API_Logging

# Run tests
.\RUN_TESTS_SIMPLE.ps1
```

Or manually make requests:

```powershell
# Create an order (generates logs)
$body = @{
    customerId = "cust-123"
    total = 99.99
} | ConvertTo-Json

Invoke-RestMethod -Method Post `
    -Uri "https://localhost:7002/api/orders" `
    -ContentType "application/json" `
    -Headers @{"X-Correlation-Id" = "test-123"} `
    -Body $body `
    -SkipCertificateCheck
```

---

## Step 6: View Logs in Kibana

### Access Kibana Dashboard

1. Open browser: **http://localhost:5601**
2. Wait for Kibana to fully load (may take 30 seconds)

### Create Data View

1. Click **☰ Menu** (top left)
2. Select **Stack Management** → **Data Views**
3. Click **Create data view**
4. Name: `logstash-*`
5. Timestamp field: `@timestamp`
6. Click **Save data view**

### View Logs

1. Click **☰ Menu** → **Discover**
2. Select data view: `logstash-*`
3. See all logs in real-time!

---

## Step 7: Search and Filter Logs

### Find logs by Correlation ID

In the search bar, type:
```
CorrelationId: "test-123"
```

### Find logs by Customer ID

```
customerId: "cust-456"
```

### Find errors only

```
Level: "Error"
```

### Find slow requests (>1 second)

```
ElapsedMilliseconds: >= 1000
```

### Time-based filtering

- Use the time picker (top right) to view logs from specific time ranges
- Default: Last 15 minutes

---

## Step 8: Create a Dashboard

### Create Custom Dashboard

1. Click **☰ Menu** → **Dashboard**
2. Click **Create dashboard**
3. Click **Create visualization**

### Example Visualizations

**1. Log Count Over Time (Line Chart)**
- Metric: Count
- X-axis: @timestamp (auto)

**2. Error Rate by Endpoint (Pie Chart)**
- Metric: Count
- Buckets: Terms → RequestPath

**3. Response Time Distribution (Histogram)**
- Metric: Average of ElapsedMilliseconds
- Buckets: Terms → RequestPath

**4. Top Correlation IDs (Table)**
- Metric: Count
- Buckets: Terms → CorrelationId

---

## Log Index Structure

Logs are stored in daily indices:

```
Index Pattern: logstash-YYYY.MM.DD

Examples:
- logstash-2025.11.28
- logstash-2025.11.29
- logstash-2025.11.30
```

### Index Management

View indices in Kibana:
1. **☰ Menu** → **Stack Management** → **Index Management**
2. See all `logstash-*` indices
3. Each index contains one day's logs

Auto-cleanup (optional):
- Elasticsearch automatically manages index retention
- By default, indices are kept indefinitely
- Configure Index Lifecycle Management (ILM) in Stack Management

---

## Fields Available for Searching

All logs include these searchable fields:

| Field | Description | Example |
|-------|-------------|---------|
| `CorrelationId` | Request correlation ID | `test-123` |
| `customerId` | Order customer ID | `cust-456` |
| `total` | Order total | `99.99` |
| `RequestPath` | HTTP endpoint | `/api/orders` |
| `ResponseStatusCode` | HTTP status | `201` |
| `ElapsedMilliseconds` | Response time | `45.3` |
| `ClientIP` | Client IP address | `127.0.0.1` |
| `UserAgent` | Client user agent | `Mozilla/5.0...` |
| `Application` | App name | `LoggingProduction` |
| `Environment` | Dev/Prod | `Development` |
| `MachineName` | Server name | `DESKTOP-ABC` |
| `Level` | Log level | `Information` |
| `@timestamp` | Log timestamp | `2025-11-28T...` |
| `message` | Log message | `Retrieving order 123` |

---

## Troubleshooting

### Elasticsearch not responding

```bash
# Check if container is running
docker ps | grep elasticsearch

# Check logs
docker logs elasticsearch

# Restart
docker-compose restart elasticsearch
```

### No logs appearing in Kibana

**Check 1: App is logging to console?**
```
Look for logs in dotnet run terminal
```

**Check 2: Elasticsearch is receiving logs?**
```bash
curl http://localhost:9200/logstash-*/_stats

# Should show indices created today
```

**Check 3: Data view configured?**
```
In Kibana → Stack Management → Data Views → logstash-* exists?
```

### Logs showing but no data

```bash
# Check if data is in Elasticsearch
curl http://localhost:9200/logstash-2025.11.28/_count

# Expected: {"count": <number>, ...}
```

### Kibana page blank/loading

- Wait 30 seconds for Kibana to start
- Check browser console for errors (F12)
- Try different browser
- Restart Kibana: `docker-compose restart kibana`

---

## Advanced: Index Lifecycle Management

**Optional:** Set automatic cleanup for old indices

In Kibana:
1. **Stack Management** → **Index Lifecycle Policies**
2. Create policy to delete indices after 30 days

---

## Useful Kibana Shortcuts

| Action | Shortcut |
|--------|----------|
| Quick filter | Click any field value |
| Exclude filter | Ctrl + Click field value |
| Remove filter | Click the X on filter |
| Time range | Use date picker (top right) |
| Full screen | Click view options → Full screen |

---

## Next Steps

1. ✅ Elasticsearch running (`docker-compose up -d`)
2. ✅ App logging to Elasticsearch (`dotnet run`)
3. ✅ Data visible in Kibana (`http://localhost:5601`)
4. 📊 Create custom dashboards
5. 🔔 Set up alerting (optional)
6. 📈 Monitor application metrics

---

## Stop Services

```bash
# Stop but keep data
docker-compose stop

# Restart later
docker-compose start

# Stop and delete everything
docker-compose down -v
```

---

## Performance Tips

1. **Bulk indexing**: Logs are batched (100 logs every 5 seconds)
2. **Async sinks**: Non-blocking logging to file and Elasticsearch
3. **Index size**: Daily indices prevent massive single files
4. **Retention**: Old indices can be deleted automatically via ILM

---

## Success Checklist

- [ ] Docker Compose started (`docker-compose ps` shows 2 services)
- [ ] Elasticsearch responding (`curl http://localhost:9200`)
- [ ] Kibana accessible (`http://localhost:5601`)
- [ ] Data view created (`logstash-*`)
- [ ] Logs visible in Kibana Discover
- [ ] Can search by Correlation ID
- [ ] Can filter by status code
- [ ] Custom dashboard created

---

## Production Considerations

**For production, configure:**

1. **Security**: Enable xpack.security
2. **Storage**: Increase heap size, use SSD
3. **Retention**: Set up Index Lifecycle Management
4. **Redundancy**: Multi-node Elasticsearch cluster
5. **Backup**: Automated snapshots
6. **Monitoring**: Elasticsearch X-Pack monitoring

See: https://www.elastic.co/guide/en/elasticsearch/reference/current/setup.html

---

## Resources

- [Serilog Elasticsearch Sink](https://github.com/serilog-contrib/serilog-sinks-elasticsearch)
- [Elasticsearch Documentation](https://www.elastic.co/guide/en/elasticsearch/reference/current/index.html)
- [Kibana User Guide](https://www.elastic.co/guide/en/kibana/current/index.html)
- [ELK Stack Best Practices](https://www.elastic.co/what-is/elk-stack)
