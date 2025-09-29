// k6 Load Test: Telemetry Ingest
// Track A: Validate ingest→store p95 < 1.0s SLO

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';

// Custom metrics
const ingestErrors = new Rate('ingest_errors');
const ingestLatency = new Trend('ingest_latency', true);

// Test configuration
export const options = {
  stages: [
    { duration: '2m', target: 50 },   // Ramp up to 50 VUs
    { duration: '5m', target: 50 },   // Stay at 50 VUs
    { duration: '2m', target: 100 },  // Ramp to 100 VUs
    { duration: '5m', target: 100 },  // Stay at 100 VUs
    { duration: '2m', target: 0 },    // Ramp down
  ],
  thresholds: {
    'http_req_duration{endpoint:ingest}': ['p(95)<1000'], // p95 < 1.0s
    'http_req_duration{endpoint:ingest}': ['p(99)<2500'], // p99 < 2.5s
    'ingest_errors': ['rate<0.01'],                        // Error rate < 1%
    'http_req_failed': ['rate<0.01'],                      // HTTP errors < 1%
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';
const API_TOKEN = __ENV.API_TOKEN || 'dev-token';

// Sample sensor data
const sensors = [
  { stream_id: '550e8400-e29b-41d4-a716-446655440000', type: 'temperature', unit: 'celsius' },
  { stream_id: '550e8400-e29b-41d4-a716-446655440001', type: 'humidity', unit: 'percent' },
  { stream_id: '550e8400-e29b-41d4-a716-446655440002', type: 'vpd', unit: 'kpa' },
  { stream_id: '550e8400-e29b-41d4-a716-446655440003', type: 'co2', unit: 'ppm' },
  { stream_id: '550e8400-e29b-41d4-a716-446655440004', type: 'ec', unit: 'ms_cm' },
  { stream_id: '550e8400-e29b-41d4-a716-446655440005', type: 'ph', unit: 'ph' },
];

export default function () {
  // Select random sensor
  const sensor = sensors[Math.floor(Math.random() * sensors.length)];
  
  // Generate realistic sensor value
  const value = generateSensorValue(sensor.type);
  
  // Build payload
  const payload = JSON.stringify({
    stream_id: sensor.stream_id,
    timestamp: new Date().toISOString(),
    value: value,
    unit: sensor.unit,
    quality_code: 0,
    site_id: '7c9e6679-7425-40de-944b-e07fc1f90ae7', // Test site
  });

  const params = {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${API_TOKEN}`,
    },
    tags: { endpoint: 'ingest' },
  };

  // Send ingest request
  const response = http.post(`${BASE_URL}/api/v1/telemetry/ingest`, payload, params);

  // Check response
  const success = check(response, {
    'status is 201': (r) => r.status === 201,
    'response time < 1000ms': (r) => r.timings.duration < 1000,
    'response time < 2500ms': (r) => r.timings.duration < 2500,
  });

  // Record metrics
  ingestErrors.add(!success);
  ingestLatency.add(response.timings.duration);

  // Simulate realistic sensor polling interval (every 5-10 seconds)
  sleep(Math.random() * 5 + 5);
}

// Generate realistic sensor values
function generateSensorValue(type) {
  switch (type) {
    case 'temperature':
      return 20 + Math.random() * 10; // 20-30°C
    case 'humidity':
      return 50 + Math.random() * 30; // 50-80%
    case 'vpd':
      return 0.8 + Math.random() * 0.8; // 0.8-1.6 kPa
    case 'co2':
      return 800 + Math.random() * 600; // 800-1400 ppm
    case 'ec':
      return 1.5 + Math.random() * 1.0; // 1.5-2.5 mS/cm
    case 'ph':
      return 5.5 + Math.random() * 1.0; // 5.5-6.5 pH
    default:
      return Math.random() * 100;
  }
}

// Handle test setup
export function setup() {
  console.log(`Starting telemetry ingest load test against ${BASE_URL}`);
  console.log(`SLO Target: p95 < 1.0s, p99 < 2.5s`);
  return { startTime: new Date().toISOString() };
}

// Handle test teardown
export function teardown(data) {
  console.log(`Test completed. Started at: ${data.startTime}`);
}
