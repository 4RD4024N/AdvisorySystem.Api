# ? Production Readiness Checklist - AdvisorySystem.Api

## ?? Pre-Deployment Checklist

### Code Quality
- [ ] All code reviewed and approved
- [ ] No console.log or debug statements in production code
- [ ] No TODO/FIXME comments left in critical paths
- [ ] Code follows .NET coding standards
- [ ] All warnings resolved

### Testing
- [ ] ? Unit tests written (target: 80% coverage)
- [ ] ? Integration tests passing
- [ ] Load tests completed successfully
- [ ] E2E tests passing
- [ ] Security scans completed
- [ ] Performance benchmarks met

### Security
- [ ] ? JWT authentication implemented
- [ ] ? Rate limiting configured
- [ ] ? CORS properly configured
- [ ] ? HTTPS enforced
- [ ] Input validation added (FluentValidation)
- [ ] XSS protection implemented
- [ ] SQL injection prevention verified
- [ ] File upload security validated
- [ ] Secrets moved to Azure Key Vault
- [ ] Security headers configured
- [ ] OWASP Top 10 verified

### Performance
- [ ] Database indexes added
- [ ] Response caching implemented
- [ ] Redis/distributed cache configured
- [ ] Pagination implemented
- [ ] AsNoTracking used for read queries
- [ ] N+1 query problems resolved
- [ ] Connection pooling optimized
- [ ] Response compression enabled
- [ ] CDN configured (if applicable)

### Database
- [ ] Migrations tested
- [ ] Backup strategy defined
- [ ] Rollback plan documented
- [ ] Connection string secured
- [ ] Indexes optimized
- [ ] Query performance validated

### Infrastructure
- [ ] Azure resources provisioned
- [ ] App Service configured
- [ ] SQL Server configured
- [ ] Blob Storage configured
- [ ] Application Insights enabled
- [ ] Autoscaling configured
- [ ] Health checks implemented

### Monitoring & Logging
- [ ] ? Application Insights configured
- [ ] Error logging implemented
- [ ] Audit logging added
- [ ] Alert rules configured
- [ ] Dashboard created
- [ ] Log retention policy set

### Documentation
- [ ] API documentation (Swagger)
- [ ] Deployment guide written
- [ ] Architecture document updated
- [ ] Runbook created
- [ ] Disaster recovery plan documented
- [ ] Security policy documented

### CI/CD
- [ ] Pipeline tested
- [ ] Automated tests in pipeline
- [ ] Deployment approval flow configured
- [ ] Rollback procedure tested
- [ ] Environment variables configured
- [ ] Secrets management verified

### Configuration
- [ ] appsettings.Production.json created
- [ ] Environment variables set
- [ ] Connection strings secured
- [ ] JWT keys rotated
- [ ] CORS origins updated for production
- [ ] Rate limits tuned for production load

### Compliance
- [ ] GDPR compliance reviewed
- [ ] Privacy policy published
- [ ] Terms of service published
- [ ] Data retention policy defined
- [ ] User consent mechanisms implemented

## ?? Deployment Day Checklist

### Pre-Deployment (T-2 hours)
- [ ] Notify users of planned maintenance
- [ ] Verify backup completed successfully
- [ ] Team members available
- [ ] Rollback plan reviewed
- [ ] Monitoring dashboard open

### Deployment (T-0)
- [ ] Trigger deployment pipeline
- [ ] Monitor deployment logs
- [ ] Watch for errors in real-time
- [ ] Verify health check passes

### Post-Deployment (T+15 minutes)
- [ ] Smoke tests passing
- [ ] Login functionality working
- [ ] Critical user flows tested
- [ ] Database migrations applied
- [ ] No errors in Application Insights

### Post-Deployment (T+1 hour)
- [ ] Monitor error rate (< 0.1%)
- [ ] Monitor response times (P95 < 1s)
- [ ] Check database performance
- [ ] Verify rate limiting working
- [ ] Check file uploads/downloads
- [ ] Verify notifications sending

### Post-Deployment (T+24 hours)
- [ ] No critical errors logged
- [ ] Performance metrics stable
- [ ] User feedback collected
- [ ] No rollback required
- [ ] Document any issues encountered

## ?? Configuration Verification

### Environment Variables Check

```bash
# Verify all required environment variables are set

# Azure App Service
az webapp config appsettings list \
--resource-group your-rg \
  --name your-app-name \
  | jq '.[] | select(.name | startswith("JWT_") or startswith("DB_") or startswith("AZURE_"))'
```

### Required Environment Variables

```bash
# Production
JWT_SECRET_KEY="..."
JWT_ISSUER="AdvisorySystem"
JWT_AUDIENCE="AdvisorySystem"
JWT_EXPIRES_MINUTES="60"

DB_CONNECTION_STRING="Server=...;Database=...;User=...;Password=...;"

AZURE_STORAGE_CONNECTION_STRING="DefaultEndpointsProtocol=https;..."
APPINSIGHTS_CONNECTION_STRING="InstrumentationKey=...;"

ASPNETCORE_ENVIRONMENT="Production"
ASPNETCORE_URLS="http://+:8080"
```

### Health Check Script

```bash
#!/bin/bash
# health-check.sh

BASE_URL="https://your-app.azurewebsites.net"

echo "?? Running health checks..."

# 1. Check health endpoint
echo -n "Health endpoint: "
if curl -f -s "$BASE_URL/health" > /dev/null; then
    echo "? PASS"
else
    echo "? FAIL"
    exit 1
fi

# 2. Check Swagger
echo -n "Swagger UI: "
if curl -f -s "$BASE_URL/swagger/index.html" > /dev/null; then
    echo "? PASS"
else
    echo "? FAIL"
fi

# 3. Check authentication
echo -n "Auth endpoint: "
RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" -X POST \
  "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"wrong"}')

if [ "$RESPONSE" == "401" ]; then
echo "? PASS (returns 401 as expected)"
else
    echo "? FAIL (unexpected status: $RESPONSE)"
fi

# 4. Check rate limiting
echo -n "Rate limiting: "
for i in {1..6}; do
    curl -s -X POST "$BASE_URL/api/auth/login" \
        -H "Content-Type: application/json" \
        -d '{"email":"test@test.com","password":"wrong"}' > /dev/null
done

RATE_LIMIT_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" -X POST \
  "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"wrong"}')

if [ "$RATE_LIMIT_RESPONSE" == "429" ]; then
    echo "? PASS (rate limited after 5 attempts)"
else
    echo "??  WARNING (no rate limit triggered)"
fi

echo ""
echo "? Health checks complete!"
```

### Load Test Script

```bash
#!/bin/bash
# load-test.sh

echo "?? Running load test..."

k6 run --vus 50 --duration 5m Tests/LoadTesting/loadtest-load.js

echo "?? Analyzing results..."

# Check if any errors occurred
if [ $? -ne 0 ]; then
    echo "? Load test failed!"
    exit 1
else
    echo "? Load test passed!"
fi
```

## ?? Success Metrics

### Performance
- **P50 Response Time**: < 200ms
- **P95 Response Time**: < 1000ms
- **P99 Response Time**: < 2000ms
- **Throughput**: > 100 req/s
- **Error Rate**: < 0.1%

### Availability
- **Uptime**: > 99.9%
- **Health Check**: Passing
- **Database Connectivity**: Stable

### Security
- **Rate Limiting**: Enforced
- **Authentication**: Working
- **Authorization**: Enforced
- **No Security Vulnerabilities**: Verified

## ?? Rollback Triggers

Rollback if any of these occur:

1. **Error Rate > 1%** for more than 5 minutes
2. **P95 Response Time > 3s** for more than 10 minutes
3. **Health Check Failures** for more than 2 minutes
4. **Database Connection Issues**
5. **Critical Security Vulnerability** discovered
6. **Data Corruption** detected

## ?? Emergency Contacts

```
Lead Developer: [Name] - [Phone] - [Email]
DevOps Engineer: [Name] - [Phone] - [Email]
DBA: [Name] - [Phone] - [Email]
On-Call Engineer: [Phone]
```

## ?? Post-Mortem Template

```markdown
# Post-Deployment Report

**Date**: YYYY-MM-DD
**Deployment**: v1.0.0
**Status**: ? Success / ?? Issues / ? Rollback

## Summary
Brief overview of deployment

## Metrics
- Deployment Duration: X minutes
- Downtime: X minutes
- Users Affected: X
- Error Rate: X%

## Issues Encountered
1. Issue 1: Description + Resolution
2. Issue 2: Description + Resolution

## Lessons Learned
- What went well
- What could be improved

## Action Items
- [ ] Action 1
- [ ] Action 2

## Approved By
- [Name], [Role]
```

## ?? Go/No-Go Decision Matrix

| Criteria | Threshold | Status | Go/No-Go |
|----------|-----------|--------|----------|
| All tests passing | 100% | ? | GO |
| Code coverage | > 80% | ? | GO |
| Security scan | No critical | ? | GO |
| Performance tests | All pass | ? | GO |
| Backup completed | Yes | ? | GO |
| Team available | Yes | ? | GO |
| Rollback tested | Yes | ? | GO |

**Final Decision**: ? **GO FOR PRODUCTION**

---

**Signed off by:**
- [ ] Lead Developer
- [ ] DevOps Engineer
- [ ] Project Manager
- [ ] QA Lead

**Date**: _____________
