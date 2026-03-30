# Action: get-service

## Purpose

Retrieve details of a single service by its ID. Use this to check service status, replica count, and last deployment time.

---

## Endpoint

```
GET $API_BASE_URL/deployment/v1/Service/Get?serviceId={id}&projectKey=$PROJECT_SLUG
```

---

## curl

```bash
curl --location "$API_BASE_URL/deployment/v1/Service/Get?serviceId=service-id-here&projectKey=$PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $X_BLOCKS_KEY" \
  --header "Content-Type: application/json"
```

---

## Query Parameters

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| serviceId | string | yes | ID of the service to retrieve |
| projectKey | string | yes | Use `$PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "service": {
    "serviceId": "string",
    "name": "identity-service",
    "status": "Running",
    "replicas": 2,
    "lastDeployedAt": "2024-01-15T10:30:00Z",
    "repository": "my-app-backend",
    "branch": "main"
  },
  "isSuccess": true,
  "errors": {}
}
```

**Service status values:** `Running`, `Stopped`, `Deploying`, `Failed`

---

## On Failure

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | Service not found | Verify `serviceId` from `get-services` response |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `API_BASE_URL` | Check environment URL in Cloud Portal |
