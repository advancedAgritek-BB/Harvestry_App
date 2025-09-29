// k6 Load Test: Realtime Push (WebSocket/SSE)
// Track A: Validate store→client p95 < 1.5s SLO

import ws from 'k6/ws';
import { check, sleep } from 'k6';
import { Rate, Trend, Counter } from 'k6/metrics';

// Custom metrics
const pushLatency = new Trend('push_latency', true);
const messagesReceived = new Counter('messages_received');
const connectionErrors = new Rate('connection_errors');

// Test configuration
export const options = {
  stages: [
    { duration: '1m', target: 20 },   // Ramp up to 20 connections
    { duration: '3m', target: 20 },   // Stay at 20 connections
    { duration: '1m', target: 50 },   // Ramp to 50 connections
    { duration: '3m', target: 50 },   // Stay at 50 connections
    { duration: '1m', target: 0 },    // Ramp down
  ],
  thresholds: {
    'push_latency': ['p(95)<1500'], // p95 < 1.5s
    'push_latency': ['p(99)<3000'], // p99 < 3.0s
    'connection_errors': ['rate<0.05'], // Connection error rate < 5%
  },
};

const WS_URL = __ENV.WS_URL || 'ws://localhost:5000/api/v1/realtime';
const API_TOKEN = __ENV.API_TOKEN || 'dev-token';
const SITE_ID = '7c9e6679-7425-40de-944b-e07fc1f90ae7'; // Test site

export default function () {
  const url = `${WS_URL}?site_id=${SITE_ID}&token=${API_TOKEN}`;
  
  const response = ws.connect(url, {
    tags: { endpoint: 'realtime' },
  }, function (socket) {
    socket.on('open', function () {
      console.log('WebSocket connection established');
      
      // Subscribe to sensor updates
      socket.send(JSON.stringify({
        action: 'subscribe',
        channels: ['sensor_readings', 'alerts', 'task_events'],
        site_id: SITE_ID,
      }));
    });

    socket.on('message', function (data) {
      const message = JSON.parse(data);
      const clientReceivedAt = Date.now();
      
      // Check message structure
      check(message, {
        'has timestamp': (m) => m.timestamp !== undefined,
        'has channel': (m) => m.channel !== undefined,
        'has data': (m) => m.data !== undefined,
      });

      // Calculate end-to-end latency (server timestamp → client received)
      if (message.timestamp) {
        const serverTimestamp = new Date(message.timestamp).getTime();
        const latency = clientReceivedAt - serverTimestamp;
        
        pushLatency.add(latency);
        messagesReceived.add(1);
        
        // Log slow messages
        if (latency > 1500) {
          console.log(`Slow push: ${latency}ms for ${message.channel}`);
        }
      }
    });

    socket.on('error', function (e) {
      console.log('WebSocket error:', e);
      connectionErrors.add(1);
    });

    socket.on('close', function () {
      console.log('WebSocket connection closed');
    });

    // Keep connection alive for test duration
    socket.setTimeout(function () {
      console.log('Closing WebSocket connection');
      socket.close();
    }, 180000); // 3 minutes per connection

    // Send periodic heartbeat
    socket.setInterval(function () {
      socket.send(JSON.stringify({ action: 'ping' }));
    }, 30000); // Every 30 seconds
  });

  check(response, {
    'connection established': (r) => r && r.status === 101,
  });

  // Wait before creating next connection
  sleep(1);
}

export function setup() {
  console.log(`Starting realtime push load test against ${WS_URL}`);
  console.log(`SLO Target: p95 < 1.5s store→client latency`);
  return { startTime: new Date().toISOString() };
}

export function teardown(data) {
  console.log(`Test completed. Started at: ${data.startTime}`);
}
