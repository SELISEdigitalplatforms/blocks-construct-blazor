# Action: update-service-config

## Purpose

Update a service's configuration including replica count and environment variables. Use this to scale services or change runtime configuration.

---

## Endpoint

```
POST $API_BASE_URL/deployment/v1/Service/UpdateConfig
```

---

## curl

```bash
curl --location "$API_BASE_URL/deployment/v1/Service/UpdateConfig" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "serviceId": "service-id-here",
    "replicas": 3,
    "envVars": {
      "NODE_ENV": "production",
      "LOG_LEVEL": "info"
    },
    "projectKey": "'"$PROJECT_SLUG"'"
  }'
```

---

## Request Fields

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| serviceId | string | yes | ID of the service to configure |
| replicas | integer | no | Number of service replicas (1-10) |
| envVars | object | no | Key/value pairs for environment variables |
| projectKey | string | yes | Use `$PROJECT_SLUG` |

> At least one of `replicas` or `envVars` must be provided. Omitted fields are left unchanged.

---

## On Success (200)

```json
{
  "isSuccess": true,
  "errors": {}
}
```

Configuration has been applied. Changes to replicas take effect immediately. Environment variable changes may trigger a service restart.

---

## On Failure

```json
{
  "isSuccess": false,
  "errors": {
    "replicas": "Replicas must be between 1 and 10",
    "serviceId": "Service not found"
  }
}
```

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | Validation error on one or more fields | Inspect `errors` dictionary and correct the request |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `API_BASE_URL` | Check environment URL in Cloud Portal |
