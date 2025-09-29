# Contract Testing Framework

**Track A Requirement** - API contract validation for REST and WebSocket endpoints

## 🎯 Purpose

Contract tests ensure that API contracts between services remain stable and backward-compatible. This prevents breaking changes from reaching production.

---

## 🔧 Tools Used

### For REST APIs: **Pact** or **OpenAPI Validator**
- **Pact**: Consumer-driven contract testing
- **OpenAPI Validator**: Schema-based validation

### For WebSocket/SSE: **Custom Deterministic Scenarios**
- Predefined message sequences
- State verification
- Timing assertions

---

## 📋 Contract Test Structure

```
tests/contract/
├── README.md
├── rest/
│   ├── api-gateway.pact.json
│   ├── telemetry-service.pact.json
│   └── workflow-service.pact.json
├── websocket/
│   ├── realtime-subscription.test.js
│   └── command-dispatch.test.js
├── schemas/
│   ├── openapi/
│   │   ├── api-gateway.yaml
│   │   ├── telemetry.yaml
│   │   └── workflow.yaml
│   └── messages/
│       ├── sensor-reading.schema.json
│       ├── alert.schema.json
│       └── command.schema.json
└── run-tests.sh
```

---

## 🌐 REST API Contract Tests

### Option 1: Pact (Consumer-Driven)

#### Consumer Test (Frontend)
```javascript
// tests/contract/rest/api-gateway.consumer.test.js
const { Pact } = require('@pact-foundation/pact');
const { like, term } = require('@pact-foundation/pact/dsl/matchers');

const provider = new Pact({
  consumer: 'frontend',
  provider: 'api-gateway',
  port: 1234,
  log: './logs/pact.log',
  dir: './pacts',
  logLevel: 'INFO',
});

describe('API Gateway Contract', () => {
  beforeAll(() => provider.setup());
  afterAll(() => provider.finalize());
  afterEach(() => provider.verify());

  describe('GET /api/v1/tasks', () => {
    beforeEach(() => {
      return provider.addInteraction({
        state: 'tasks exist',
        uponReceiving: 'a request for tasks',
        withRequest: {
          method: 'GET',
          path: '/api/v1/tasks',
          query: 'site_id=site-123&status=pending',
          headers: {
            'Authorization': term({
              matcher: 'Bearer .*',
              generate: 'Bearer token123',
            }),
          },
        },
        willRespondWith: {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
          body: {
            tasks: like([
              {
                task_id: like('task-uuid'),
                title: like('Task Title'),
                status: term({
                  matcher: 'pending|in_progress|completed',
                  generate: 'pending',
                }),
                due_date: like('2025-12-31T23:59:59Z'),
                assignee_id: like('user-uuid'),
              },
            ]),
            total: like(10),
            page: like(1),
          },
        },
      });
    });

    it('returns tasks for a site', async () => {
      const response = await fetch('http://localhost:1234/api/v1/tasks?site_id=site-123&status=pending', {
        headers: { 'Authorization': 'Bearer token123' },
      });
      
      expect(response.status).toBe(200);
      const data = await response.json();
      expect(data.tasks).toBeDefined();
      expect(data.tasks[0].task_id).toBeDefined();
    });
  });
});
```

#### Provider Verification (.NET)
```csharp
using PactNet;
using PactNet.Verifier;
using Xunit;

public class ApiGatewayProviderTests
{
    [Fact]
    public void EnsureApiGatewayHonorsPact()
    {
        var config = new PactVerifierConfig
        {
            Outputters = new[]
            {
                new XUnitOutput(outputHelper),
            },
            LogLevel = PactLogLevel.Information,
        };

        var verifier = new PactVerifier(config);
        verifier
            .ServiceProvider("api-gateway", "http://localhost:5000")
            .WithFileSource(new FileInfo("../pacts/frontend-api-gateway.json"))
            .WithProviderStateUrl("http://localhost:5000/provider-states")
            .Verify();
    }
}
```

### Option 2: OpenAPI Schema Validation

#### OpenAPI Schema
```yaml
# tests/contract/schemas/openapi/api-gateway.yaml
openapi: 3.0.0
info:
  title: Harvestry API Gateway
  version: 1.0.0
paths:
  /api/v1/tasks:
    get:
      summary: Get tasks
      parameters:
        - name: site_id
          in: query
          required: true
          schema:
            type: string
            format: uuid
        - name: status
          in: query
          schema:
            type: string
            enum: [pending, in_progress, completed]
      responses:
        '200':
          description: Successful response
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/TasksResponse'
components:
  schemas:
    TasksResponse:
      type: object
      required: [tasks, total, page]
      properties:
        tasks:
          type: array
          items:
            $ref: '#/components/schemas/Task'
        total:
          type: integer
        page:
          type: integer
    Task:
      type: object
      required: [task_id, title, status]
      properties:
        task_id:
          type: string
          format: uuid
        title:
          type: string
        status:
          type: string
          enum: [pending, in_progress, completed]
        due_date:
          type: string
          format: date-time
        assignee_id:
          type: string
          format: uuid
```

#### Validation Test (Jest + OpenAPI Validator)
```javascript
// tests/contract/rest/openapi-validation.test.js
const OpenAPIResponseValidator = require('openapi-response-validator').default;
const fs = require('fs');
const yaml = require('js-yaml');

const openAPISpec = yaml.load(fs.readFileSync('./schemas/openapi/api-gateway.yaml', 'utf8'));

describe('OpenAPI Contract Validation', () => {
  it('validates GET /api/v1/tasks response', async () => {
    const validator = new OpenAPIResponseValidator({
      responses: openAPISpec.paths['/api/v1/tasks'].get.responses,
      components: openAPISpec.components,
    });

    const actualResponse = {
      status: 200,
      headers: { 'content-type': 'application/json' },
      body: {
        tasks: [
          {
            task_id: '550e8400-e29b-41d4-a716-446655440000',
            title: 'Test Task',
            status: 'pending',
            due_date: '2025-12-31T23:59:59Z',
            assignee_id: '550e8400-e29b-41d4-a716-446655440001',
          },
        ],
        total: 1,
        page: 1,
      },
    };

    const errors = validator.validateResponse('200', actualResponse.body);
    
    expect(errors).toBeUndefined(); // No validation errors
  });
});
```

---

## 🔌 WebSocket Contract Tests

### Deterministic Scenario Testing

```javascript
// tests/contract/websocket/realtime-subscription.test.js
const WebSocket = require('ws');
const { v4: uuidv4 } = require('uuid');

describe('Realtime WebSocket Contract', () => {
  let ws;
  const TEST_SITE_ID = 'site-123';
  const TEST_TOKEN = 'test-token';

  beforeEach((done) => {
    ws = new WebSocket(`ws://localhost:5000/api/v1/realtime?site_id=${TEST_SITE_ID}&token=${TEST_TOKEN}`);
    ws.on('open', done);
  });

  afterEach(() => {
    if (ws.readyState === WebSocket.OPEN) {
      ws.close();
    }
  });

  it('should accept subscription request and respond with confirmation', (done) => {
    const subscribeMessage = {
      action: 'subscribe',
      channels: ['sensor_readings', 'alerts'],
      site_id: TEST_SITE_ID,
    };

    ws.on('message', (data) => {
      const message = JSON.parse(data);
      
      // Contract: Server acknowledges subscription
      expect(message).toHaveProperty('action', 'subscribed');
      expect(message).toHaveProperty('channels');
      expect(message.channels).toEqual(expect.arrayContaining(['sensor_readings', 'alerts']));
      
      done();
    });

    ws.send(JSON.stringify(subscribeMessage));
  });

  it('should receive sensor reading updates in expected format', (done) => {
    const subscribeMessage = {
      action: 'subscribe',
      channels: ['sensor_readings'],
      site_id: TEST_SITE_ID,
    };

    let subscribed = false;

    ws.on('message', (data) => {
      const message = JSON.parse(data);
      
      if (message.action === 'subscribed') {
        subscribed = true;
        // Trigger a sensor reading (in real test, this would be simulated)
        return;
      }

      if (subscribed && message.channel === 'sensor_readings') {
        // Contract: Sensor reading message format
        expect(message).toMatchObject({
          channel: 'sensor_readings',
          timestamp: expect.stringMatching(/^\d{4}-\d{2}-\d{2}T/), // ISO 8601
          data: {
            stream_id: expect.stringMatching(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i), // UUID
            value: expect.any(Number),
            unit: expect.any(String),
            site_id: TEST_SITE_ID,
          },
        });
        
        done();
      }
    });

    ws.send(JSON.stringify(subscribeMessage));
  }, 10000); // 10 second timeout

  it('should handle unsubscribe requests', (done) => {
    const unsubscribeMessage = {
      action: 'unsubscribe',
      channels: ['sensor_readings'],
      site_id: TEST_SITE_ID,
    };

    ws.on('message', (data) => {
      const message = JSON.parse(data);
      
      if (message.action === 'unsubscribed') {
        expect(message.channels).toEqual(['sensor_readings']);
        done();
      }
    });

    ws.send(JSON.stringify(unsubscribeMessage));
  });

  it('should reject subscription to unauthorized site', (done) => {
    const subscribeMessage = {
      action: 'subscribe',
      channels: ['sensor_readings'],
      site_id: 'unauthorized-site',
    };

    ws.on('message', (data) => {
      const message = JSON.parse(data);
      
      // Contract: Unauthorized access returns error
      expect(message).toHaveProperty('action', 'error');
      expect(message).toHaveProperty('code', 'UNAUTHORIZED');
      expect(message.message).toMatch(/not authorized/i);
      
      done();
    });

    ws.send(JSON.stringify(subscribeMessage));
  });
});
```

---

## 🚀 Running Contract Tests

### Local Development
```bash
# Install dependencies
npm install --save-dev @pact-foundation/pact jest ws

# Run contract tests
npm run test:contract

# Or use the script
./tests/contract/run-tests.sh --env dev
```

### CI/CD Integration
```yaml
# .github/workflows/contract-tests.yml
name: Contract Tests

on:
  pull_request:
    branches: [main]
  push:
    branches: [main]

jobs:
  contract-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '18'
      
      - name: Install dependencies
        run: npm ci
      
      - name: Start services
        run: docker compose up -d
      
      - name: Wait for services
        run: ./scripts/wait-for-services.sh
      
      - name: Run contract tests
        run: npm run test:contract
      
      - name: Publish Pact files
        if: github.ref == 'refs/heads/main'
        run: npm run pact:publish
```

---

## 📊 Contract Test Reports

Contract tests generate verification reports:

```
tests/contract/results/
├── pact-verification.json
├── openapi-validation.json
└── websocket-scenarios.json
```

View results in CI pipeline or locally:
```bash
open tests/contract/results/pact-verification.html
```

---

## ✅ Track A Objective

Ensure API stability and prevent breaking changes through automated contract validation at every PR.
