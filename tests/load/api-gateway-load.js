// k6 Load Test: API Gateway
// Track A: Validate API p95 < 800ms for commands, p95 < 300ms for tasks

import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';

// Custom metrics
const commandLatency = new Trend('command_latency', true);
const taskLatency = new Trend('task_latency', true);
const apiErrors = new Rate('api_errors');

// Test configuration
export const options = {
  stages: [
    { duration: '2m', target: 100 },  // Ramp up to 100 VUs
    { duration: '5m', target: 100 },  // Stay at 100 VUs
    { duration: '2m', target: 200 },  // Ramp to 200 VUs
    { duration: '5m', target: 200 },  // Stay at 200 VUs
    { duration: '2m', target: 0 },    // Ramp down
  ],
  thresholds: {
    'command_latency': ['p(95)<800'],  // Command p95 < 800ms
    'task_latency': ['p(95)<300'],     // Task p95 < 300ms
    'http_req_duration': ['p(99)<2000'], // Overall p99 < 2s
    'api_errors': ['rate<0.01'],       // Error rate < 1%
    'http_req_failed': ['rate<0.01'],  // HTTP errors < 1%
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';
const API_TOKEN = __ENV.API_TOKEN || 'dev-token';
const SITE_ID = '7c9e6679-7425-40de-944b-e07fc1f90ae7';

const params = {
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${API_TOKEN}`,
  },
};

export default function () {
  // Mix of operations with realistic distribution
  const operations = [
    { name: 'getTasks', weight: 40 },
    { name: 'createTask', weight: 20 },
    { name: 'getSensorReadings', weight: 15 },
    { name: 'dispatchCommand', weight: 10 },
    { name: 'getAlerts', weight: 10 },
    { name: 'updateTask', weight: 5 },
  ];

  const operation = weightedRandom(operations);

  switch (operation) {
    case 'getTasks':
      testGetTasks();
      break;
    case 'createTask':
      testCreateTask();
      break;
    case 'getSensorReadings':
      testGetSensorReadings();
      break;
    case 'dispatchCommand':
      testDispatchCommand();
      break;
    case 'getAlerts':
      testGetAlerts();
      break;
    case 'updateTask':
      testUpdateTask();
      break;
  }

  sleep(Math.random() * 2 + 1); // 1-3 seconds between requests
}

function testGetTasks() {
  group('GET /api/v1/tasks', function () {
    const response = http.get(
      `${BASE_URL}/api/v1/tasks?site_id=${SITE_ID}&status=pending`,
      Object.assign({}, params, { tags: { endpoint: 'tasks' } })
    );

    const success = check(response, {
      'status is 200': (r) => r.status === 200,
      'response time < 300ms': (r) => r.timings.duration < 300,
    });

    taskLatency.add(response.timings.duration);
    apiErrors.add(!success);
  });
}

function testCreateTask() {
  group('POST /api/v1/tasks', function () {
    const payload = JSON.stringify({
      site_id: SITE_ID,
      title: `Load Test Task ${Date.now()}`,
      description: 'Auto-generated task for load testing',
      assignee_id: null,
      due_date: new Date(Date.now() + 86400000).toISOString(),
      priority: 'normal',
      category: 'cultivation',
    });

    const response = http.post(
      `${BASE_URL}/api/v1/tasks`,
      payload,
      Object.assign({}, params, { tags: { endpoint: 'tasks' } })
    );

    const success = check(response, {
      'status is 201': (r) => r.status === 201,
      'response time < 300ms': (r) => r.timings.duration < 300,
      'has task_id': (r) => JSON.parse(r.body).task_id !== undefined,
    });

    taskLatency.add(response.timings.duration);
    apiErrors.add(!success);
  });
}

function testGetSensorReadings() {
  group('GET /api/v1/telemetry/readings', function () {
    const now = new Date();
    const oneHourAgo = new Date(now.getTime() - 3600000);

    const response = http.get(
      `${BASE_URL}/api/v1/telemetry/readings?site_id=${SITE_ID}&start=${oneHourAgo.toISOString()}&end=${now.toISOString()}&rollup=5m`,
      Object.assign({}, params, { tags: { endpoint: 'telemetry' } })
    );

    check(response, {
      'status is 200': (r) => r.status === 200,
      'response time < 1000ms': (r) => r.timings.duration < 1000,
    });
  });
}

function testDispatchCommand() {
  group('POST /api/v1/commands/dispatch', function () {
    const payload = JSON.stringify({
      site_id: SITE_ID,
      command_type: 'irrigation_start',
      target_device_id: '550e8400-e29b-41d4-a716-446655440010',
      parameters: {
        duration_seconds: 300,
        group_id: '550e8400-e29b-41d4-a716-446655440020',
      },
      priority: 'normal',
    });

    const response = http.post(
      `${BASE_URL}/api/v1/commands/dispatch`,
      payload,
      Object.assign({}, params, { tags: { endpoint: 'commands' } })
    );

    const success = check(response, {
      'status is 202': (r) => r.status === 202,
      'response time < 800ms': (r) => r.timings.duration < 800,
      'has command_id': (r) => JSON.parse(r.body).command_id !== undefined,
    });

    commandLatency.add(response.timings.duration);
    apiErrors.add(!success);
  });
}

function testGetAlerts() {
  group('GET /api/v1/alerts', function () {
    const response = http.get(
      `${BASE_URL}/api/v1/alerts?site_id=${SITE_ID}&resolved=false`,
      Object.assign({}, params, { tags: { endpoint: 'alerts' } })
    );

    check(response, {
      'status is 200': (r) => r.status === 200,
      'response time < 500ms': (r) => r.timings.duration < 500,
    });
  });
}

function testUpdateTask() {
  group('PATCH /api/v1/tasks/:id', function () {
    const taskId = '550e8400-e29b-41d4-a716-446655440030'; // Mock task ID

    const payload = JSON.stringify({
      status: 'in_progress',
    });

    const response = http.patch(
      `${BASE_URL}/api/v1/tasks/${taskId}`,
      payload,
      Object.assign({}, params, { tags: { endpoint: 'tasks' } })
    );

    const success = check(response, {
      'status is 200 or 404': (r) => r.status === 200 || r.status === 404, // 404 OK for mock ID
      'response time < 300ms': (r) => r.timings.duration < 300,
    });

    taskLatency.add(response.timings.duration);
    apiErrors.add(!success && response.status !== 404);
  });
}

// Weighted random selection
function weightedRandom(items) {
  const totalWeight = items.reduce((sum, item) => sum + item.weight, 0);
  let random = Math.random() * totalWeight;

  for (const item of items) {
    if (random < item.weight) {
      return item.name;
    }
    random -= item.weight;
  }

  return items[0].name;
}

export function setup() {
  console.log(`Starting API Gateway load test against ${BASE_URL}`);
  console.log(`SLO Targets: Command p95 < 800ms, Task p95 < 300ms`);
  return { startTime: new Date().toISOString() };
}

export function teardown(data) {
  console.log(`Test completed. Started at: ${data.startTime}`);
}
