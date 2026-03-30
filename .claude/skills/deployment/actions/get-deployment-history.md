# Action: get-deployment-history

## Purpose

Retrieve paginated deployment history for a specific service. Use this to view past deployments, identify versions, and find a deployment ID for rollback.

---

## Endpoint

```
GET $API_BASE_URL/deployment/v1/Deployment/Gets?serviceId={id}&projectKey=$PROJECT_SLUG&page=1&pageSize=20
```

---

## curl

```bash
curl --location "$API_BASE_URL/deployment/v1/Deployment/Gets?serviceId=service-id-here&projectKey=$PROJECT_SLUG&page=1&pageSize=20" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $X_BLOCKS_KEY" \
  --header "Content-Type: application/json"
```

---

## Query Parameters

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| serviceId | string | yes | ID of the service to get history for |
| projectKey | string | yes | Use `$PROJECT_SLUG` |
| page | integer | yes | 1-based page number |
| pageSize | integer | yes | Records per page (max: 100) |

---

## On Success (200)

```json
{
  "deployments": [
    {
      "deploymentId": "string",
      "serviceId": "string",
      "buildId": "string",
      "status": "Succeeded",
      "deployedAt": "2024-01-15T10:30:00Z",
      "deployedBy": "user@example.com",
      "version": "1.2.3"
    }
  ],
  "totalCount": 50,
  "isSuccess": true,
  "errors": {}
}
```

**Deployment status values:** `Pending`, `InProgress`, `Succeeded`, `Failed`, `RolledBack`

---

## On Failure

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `API_BASE_URL` or service not found | Verify `serviceId` from `get-services` response |
