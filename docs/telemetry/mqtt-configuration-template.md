# MQTT Broker Configuration - FRP-05

**Status:** 🚧 Pending Configuration  
**Last Updated:** October 2, 2025  
**Owner:** Sensors Team + Telemetry & Controls Squad

---

## 📋 BROKER INFORMATION

### Connection Details

- **Broker URL:** `[TO BE CONFIGURED]`
- **Port:** `[1883 for TCP, 8883 for TLS]`
- **Protocol:** `MQTT v3.1.1` or `MQTT v5.0`
- **Transport Security:** `TLS 1.2+` (required for production)
- **Broker Type:** `[Mosquitto / EMQ / AWS IoT Core / Azure IoT Hub / Other]`

### Authentication

- **Method:** `[username/password / certificate / token]`
- **Username:** `[stored in secrets manager]`
- **Password/Token:** `[stored in secrets manager - DO NOT COMMIT]`
- **Client Certificate:** `[if using mTLS]`

### Connection Limits

- **Max Connections:** `[e.g., 1000]`
- **Keep-Alive Interval:** `60 seconds` (recommended)
- **Clean Session:** `false` (to resume on reconnect)
- **QoS Level:** `1` (at-least-once delivery recommended)

---

## 📡 TOPIC STRUCTURE

### Ingest Topics (Device → Platform)

#### Primary Telemetry Topic

```
site/{siteId}/equipment/{equipmentId}/telemetry
```

**Example:**

```
site/7c9e6679-7425-40de-944b-e07fc1f90ae7/equipment/a3f8c9e2-1234-5678-90ab-cdef12345678/telemetry
```

**Payload Format:** JSON

```json
{
  "timestamp": "2025-10-02T14:30:00Z",
  "readings": [
    {
      "stream_id": "550e8400-e29b-41d4-a716-446655440000",
      "value": 72.5,
      "unit": "degF",
      "quality_code": 0,
      "message_id": "msg_20251002_143000_001"
    },
    {
      "stream_id": "550e8400-e29b-41d4-a716-446655440001",
      "value": 65.2,
      "unit": "pct",
      "quality_code": 0,
      "message_id": "msg_20251002_143000_002"
    }
  ],
  "metadata": {
    "device_id": "HSES12-00123",
    "firmware_version": "1.2.3"
  }
}
```

#### Alternative: Per-Sensor Topics (if needed)

```
site/{siteId}/equipment/{equipmentId}/sensor/{streamId}
```

**Payload Format:** JSON (single reading)

```json
{
  "timestamp": "2025-10-02T14:30:00Z",
  "value": 72.5,
  "unit": "degF",
  "quality_code": 0,
  "message_id": "msg_20251002_143000_001"
}
```

### Control Topics (Platform → Device) [Future Phase]

```
site/{siteId}/equipment/{equipmentId}/command
site/{siteId}/equipment/{equipmentId}/config
```

### Status Topics

```
site/{siteId}/equipment/{equipmentId}/status    # Heartbeat, health
site/{siteId}/equipment/{equipmentId}/error     # Error reporting
```

---

## 📊 PAYLOAD SPECIFICATIONS

### Required Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `timestamp` | ISO-8601 String | Yes | Reading timestamp (UTC) |
| `readings[]` | Array | Yes | Array of sensor readings |
| `readings[].stream_id` | UUID | Yes | Sensor stream identifier |
| `readings[].value` | Number | Yes | Sensor value |
| `readings[].unit` | String | Yes | Unit of measurement (see units table) |
| `readings[].quality_code` | Integer | No | Quality code (0=good, 64=suspect, 192=bad) |
| `readings[].message_id` | String | Yes | Unique message ID for idempotency |

### Optional Fields

| Field | Type | Description |
|-------|------|-------------|
| `metadata.device_id` | String | Physical device identifier |
| `metadata.firmware_version` | String | Device firmware version |
| `metadata.signal_strength` | Number | Network signal strength (dBm) |
| `metadata.battery_level` | Number | Battery percentage (0-100) |

### Supported Units

| Unit | Code | Stream Types |
|------|------|--------------|
| Degrees Fahrenheit | `degF` | temperature, water_temp, soil_temp |
| Degrees Celsius | `degC` | temperature, water_temp, soil_temp |
| Percent | `pct` | humidity, soil_moisture, battery_level |
| Parts per million | `ppm` | co2, dissolved_oxygen |
| Kilopascals | `kPa` | vpd, pressure |
| Micromoles | `umol` | light_par, light_ppfd |
| Microsiemens/cm | `uS` | ec |
| pH units | `pH` | ph |
| Milligrams/liter | `mg_L` | dissolved_oxygen |
| Liters | `L` | water_level, flow_total |
| Gallons | `gal` | water_level, flow_total |
| Gallons per minute | `gpm` | flow_rate |
| Liters per minute | `lpm` | flow_rate |
| Watts | `W` | power_consumption |
| Kilowatt-hours | `kWh` | energy_consumption |

---

## 🔒 SECURITY REQUIREMENTS

### Production Requirements

- ✅ TLS 1.2 or higher (port 8883)
- ✅ Certificate validation enabled
- ✅ Username/password or certificate authentication
- ✅ Topic ACLs configured per site
- ✅ Message encryption in transit

### Development Environment

- 🟡 TLS optional (can use port 1883 for local testing)
- 🟡 Test credentials acceptable

---

## 🎯 PERFORMANCE TARGETS

### Throughput

- **Target:** 10,000 messages/second sustained
- **Per Device:** 1-10 messages/second (typical)
- **Batch Size:** 1-50 readings per message

### Latency

- **Publish to Broker:** < 100ms (p95)
- **Broker to Platform:** < 200ms (p95)
- **End-to-End (Device → Database):** < 1.0s (p95)

### Reliability

- **Message Delivery:** QoS 1 (at-least-once)
- **Idempotency:** Enforced via `message_id`
- **Retry Policy:** Exponential backoff (1s, 2s, 4s, 8s, 16s)

---

## 🧪 TEST DEVICES

### Test Sensor Streams

| Stream ID | Type | Unit | Expected Range |
|-----------|------|------|----------------|
| `550e8400-e29b-41d4-a716-446655440000` | temperature | degF | 65-85 |
| `550e8400-e29b-41d4-a716-446655440001` | humidity | pct | 40-80 |
| `550e8400-e29b-41d4-a716-446655440002` | vpd | kPa | 0.8-1.6 |
| `550e8400-e29b-41d4-a716-446655440003` | co2 | ppm | 800-1400 |
| `550e8400-e29b-41d4-a716-446655440004` | ec | uS | 1500-2500 |
| `550e8400-e29b-41d4-a716-446655440005` | ph | pH | 5.5-6.5 |

### Test Equipment

| Equipment ID | Site ID | Device Type |
|--------------|---------|-------------|
| `a3f8c9e2-1234-5678-90ab-cdef12345678` | `7c9e6679-7425-40de-944b-e07fc1f90ae7` | HSES12 Sensor Node |
| `b4e9d3f3-2345-6789-01bc-def123456789` | `7c9e6679-7425-40de-944b-e07fc1f90ae7` | Climate Monitor |

---

## 📝 EXAMPLE: Publishing Test Message

### Using mosquitto_pub (CLI)

```bash
mosquitto_pub \
  -h broker.example.com \
  -p 8883 \
  -t "site/7c9e6679-7425-40de-944b-e07fc1f90ae7/equipment/a3f8c9e2-1234-5678-90ab-cdef12345678/telemetry" \
  -u "username" \
  -P "password" \
  --cafile ca.crt \
  -m '{
    "timestamp": "2025-10-02T14:30:00Z",
    "readings": [
      {
        "stream_id": "550e8400-e29b-41d4-a716-446655440000",
        "value": 72.5,
        "unit": "degF",
        "quality_code": 0,
        "message_id": "test_msg_001"
      }
    ]
  }'
```

### Using Python (Paho MQTT)

```python
import paho.mqtt.client as mqtt
import json
from datetime import datetime

def on_connect(client, userdata, flags, rc):
    print(f"Connected with result code {rc}")

client = mqtt.Client()
client.username_pw_set("username", "password")
client.tls_set(ca_certs="ca.crt")
client.on_connect = on_connect

client.connect("broker.example.com", 8883, 60)

payload = {
    "timestamp": datetime.utcnow().isoformat() + "Z",
    "readings": [
        {
            "stream_id": "550e8400-e29b-41d4-a716-446655440000",
            "value": 72.5,
            "unit": "degF",
            "quality_code": 0,
            "message_id": "test_msg_001"
        }
    ]
}

topic = "site/7c9e6679-7425-40de-944b-e07fc1f90ae7/equipment/a3f8c9e2-1234-5678-90ab-cdef12345678/telemetry"
client.publish(topic, json.dumps(payload), qos=1)
print(f"Published to {topic}")

client.disconnect()
```

---

## ✅ VALIDATION CHECKLIST

### Day 0 Validation

- [ ] Broker URL and credentials confirmed
- [ ] TLS certificate obtained (if required)
- [ ] Test connection successful
- [ ] Test topic publish successful
- [ ] Test topic subscribe successful
- [ ] Payload schema validated
- [ ] Stream IDs created in database
- [ ] Throughput test completed (target: 100 msg/s minimum)

### Pre-Slice 1

- [ ] Environment variables set (`TELEMETRY_MQTT_BROKER_URL`, credentials)
- [ ] Secrets stored in secrets manager
- [ ] MQTT adapter code review
- [ ] Integration test with real device (if available)

---

## 📞 CONTACTS

- **Sensors Team Lead:** [Name/Email]
- **Telemetry & Controls Squad:** [Name/Email]
- **DevOps (Broker Infrastructure):** [Name/Email]

---

**Document Status:** 🚧 Template - Needs Configuration  
**Action Required:** Sensors Team to fill in broker details  
**Target Completion:** Before FRP-05 Day 0 session
