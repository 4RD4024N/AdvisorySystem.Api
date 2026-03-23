# Load Testing Plan - AdvisorySystem.Api
# Tool: k6 (https://k6.io/)

## Installation
```bash
# Windows (via Chocolatey)
choco install k6

# macOS
brew install k6

# Linux
sudo gpg -k
sudo gpg --no-default-keyring --keyring /usr/share/keyrings/k6-archive-keyring.gpg --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
sudo apt-get update
sudo apt-get install k6
```

## Test Scenarios

### 1. Baseline Test (Smoke Test)
**Purpose:** Verify system works under minimal load
**Duration:** 1 minute
**Virtual Users:** 1-5

```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '30s', target: 5 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'], // 95% of requests must complete below 500ms
  http_req_failed: ['rate<0.01'],   // Error rate must be below 1%
  },
};

const BASE_URL = 'http://localhost:5000';

export function setup() {
  // Register and login to get token
  const registerRes = http.post(`${BASE_URL}/api/auth/register`, JSON.stringify({
    email: `loadtest${Date.now()}@example.com`,
    password: 'Test@123456',
    fullName: 'Load Test User'
  }), {
    headers: { 'Content-Type': 'application/json' },
  });

  const loginRes = http.post(`${BASE_URL}/api/auth/login`, JSON.stringify({
    email: registerRes.json('email'),
password: 'Test@123456'
  }), {
    headers: { 'Content-Type': 'application/json' },
  });

  return { token: loginRes.json('token') };
}

export default function(data) {
  const headers = {
    'Authorization': `Bearer ${data.token}`,
    'Content-Type': 'application/json',
  };

  // Test endpoints
  let res = http.get(`${BASE_URL}/api/courses`, { headers });
  check(res, {
  'status is 200': (r) => r.status === 200,
    'response time < 500ms': (r) => r.timings.duration < 500,
  });

  sleep(1);
}
```

### 2. Load Test
**Purpose:** Test system under normal expected load
**Duration:** 10 minutes
**Peak Virtual Users:** 100

```javascript
export const options = {
  stages: [
    { duration: '2m', target: 50 },   // Ramp up to 50 users
    { duration: '5m', target: 100 },  // Stay at 100 users
    { duration: '2m', target: 50 },   // Ramp down to 50
    { duration: '1m', target: 0 },    // Ramp down to 0
  ],
  thresholds: {
    http_req_duration: ['p(95)<1000', 'p(99)<2000'],
    http_req_failed: ['rate<0.05'],
  http_reqs: ['rate>50'],         // At least 50 req/s
  },
};
```

### 3. Stress Test
**Purpose:** Find system breaking point
**Duration:** 15 minutes
**Peak Virtual Users:** 500

```javascript
export const options = {
  stages: [
    { duration: '2m', target: 100 },
    { duration: '5m', target: 300 },
    { duration: '2m', target: 500 },  // Push to limits
    { duration: '3m', target: 500 },  // Stay at limits
 { duration: '3m', target: 0 },    // Recovery
  ],
  thresholds: {
    http_req_duration: ['p(95)<3000'],
http_req_failed: ['rate<0.10'],
  },
};
```

### 4. Spike Test
**Purpose:** Test system behavior under sudden traffic spikes
**Duration:** 5 minutes

```javascript
export const options = {
  stages: [
    { duration: '30s', target: 50 },
    { duration: '30s', target: 500 }, // Sudden spike
    { duration: '1m', target: 500 },
    { duration: '30s', target: 50 },  // Return to normal
    { duration: '2m', target: 50 },
  ],
};
```

### 5. Soak Test (Endurance Test)
**Purpose:** Test system stability over extended period
**Duration:** 2 hours
**Virtual Users:** 50 (constant)

```javascript
export const options = {
  stages: [
    { duration: '5m', target: 50 },
    { duration: '110m', target: 50 }, // 2 hours constant load
    { duration: '5m', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<1000'],
    http_req_failed: ['rate<0.01'],
  },
};
```

## Specific Endpoint Tests

### Rate Limiting Test
```javascript
import { check } from 'k6';
import http from 'k6/http';

export const options = {
  scenarios: {
    rate_limit_test: {
      executor: 'constant-arrival-rate',
      rate: 10, // 10 requests per second
      duration: '1m',
      preAllocatedVUs: 20,
    },
  },
};

export default function() {
  const loginRes = http.post(`${BASE_URL}/api/auth/login`, JSON.stringify({
    email: 'test@example.com',
    password: 'wrongpassword'
  }), {
    headers: { 'Content-Type': 'application/json' },
  });

  check(loginRes, {
    'rate limited after 5 attempts': (r) => {
      // After 5 failed attempts per minute, should get 429
      return r.status === 429 || r.status === 401;
    },
  });
}
```

### Database Performance Test
```javascript
export default function(data) {
  const headers = { 'Authorization': `Bearer ${data.token}` };

  // Test various database operations
  group('Course Operations', () => {
    http.get(`${BASE_URL}/api/courses`, { headers });
    http.get(`${BASE_URL}/api/courses/categories`, { headers });
  http.get(`${BASE_URL}/api/courses/by-semester/1`, { headers });
  });

  group('Document Operations', () => {
    http.get(`${BASE_URL}/api/documents`, { headers });
    http.get(`${BASE_URL}/api/search/documents?query=test`, { headers });
  });

  group('Schedule Operations', () => {
    http.get(`${BASE_URL}/api/schedule/available`, { headers });
    http.get(`${BASE_URL}/api/student-courses/my-schedule`, { headers });
  });
}
```

## Running Tests

```bash
# Smoke test
k6 run loadtest-smoke.js

# Load test
k6 run loadtest-load.js

# Stress test
k6 run loadtest-stress.js

# Spike test
k6 run loadtest-spike.js

# Soak test (2 hours)
k6 run loadtest-soak.js

# With cloud results
k6 run --out cloud loadtest-load.js

# Generate HTML report
k6 run --out json=results.json loadtest-load.js
```

## Success Criteria

### Response Time
- **P95 < 1000ms** for all CRUD operations
- **P99 < 2000ms** for all endpoints
- **P95 < 3000ms** under stress

### Error Rate
- **< 0.1%** under normal load
- **< 1%** under stress
- **< 5%** during spike

### Throughput
- **Minimum 100 req/s** under normal load
- **No memory leaks** during soak test
- **Recovery time < 1 minute** after spike

### Rate Limiting
- **429 status code** correctly returned when limits exceeded
- **Retry-After header** present in 429 responses
- **No crashes** when rate limits are hit

## Monitoring During Tests

Use these queries/tools:
1. **Application Insights** - Track response times
2. **SQL Server Profiler** - Monitor database queries
3. **Windows Performance Monitor** - CPU, Memory, Disk I/O
4. **Network Monitor** - Bandwidth usage

## Expected Bottlenecks

1. **Database Connection Pool** - Monitor active connections
2. **File I/O** - Document uploads/downloads
3. **JWT Generation** - CPU intensive
4. **Rate Limiter Memory** - Track memory usage

## Post-Test Analysis

Check:
- [ ] Memory usage returned to baseline
- [ ] No database connection leaks
- [ ] No error rate spike after test
- [ ] Application logs for errors/warnings
- [ ] Database query performance
- [ ] Rate limiting effectiveness

## Optimization Targets

Based on test results, optimize:
1. **Caching** - Add Redis for frequently accessed data
2. **Connection Pooling** - Adjust pool size
3. **Query Optimization** - Add indexes, optimize N+1 queries
4. **CDN** - Serve static files from CDN
5. **Load Balancing** - Distribute traffic across instances
