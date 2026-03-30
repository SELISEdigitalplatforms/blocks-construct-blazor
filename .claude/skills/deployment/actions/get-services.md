# Action: get-services

## Purpose

List all services in the project. Use this to see which services are running, their replica counts, and associated repositories.

---

## Endpoint

```
GET $API_BASE_URL/deployment/v1/Service/Gets?projectKey=$PROJECT_SLUG
```

---

## curl

```bash
curl --location "$API_BASE_URL/deployment/v1/Service/Gets?projectKey=$PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $X_BLOCKS_KEY" \
  --header "Content-Type: application/json"
```

---

## Query Parameters

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| projectKey | string | yes | Use `$PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "services": [
    {
      "serviceId": "string",
      "name": "identity-service",
      "status": "Running",
      "replicas": 2,
      "lastDeployedAt": "2024-01-15T10:30:00Z",
      "repository": "my-app-backend",
      "branch": "main"
    }
  ],
  "isSuccess": true,
  "errors": {}
}
```

**Service status values:** `Running`, `Stopped`, `Deploying`, `Failed`

---

## On Failure

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `API_BASE_URL` or project not found | Check environment URL in Cloud Portal |
