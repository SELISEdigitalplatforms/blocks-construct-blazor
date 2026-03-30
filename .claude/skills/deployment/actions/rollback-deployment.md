# Action: rollback-deployment

## Purpose

Rollback a service to a previous deployment. Use this when a new deployment introduces issues and you need to revert to a known-good version.

---

## Endpoint

```
POST $API_BASE_URL/deployment/v1/Deployment/Rollback
```

---

## curl

```bash
curl --location "$API_BASE_URL/deployment/v1/Deployment/Rollback" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "serviceId": "service-id-here",
    "deploymentId": "deployment-id-here",
    "projectKey": "'"$PROJECT_SLUG"'"
  }'
```

---

## Request Fields

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| serviceId | string | yes | ID of the service to rollback |
| deploymentId | string | yes | ID of the target deployment to rollback to (from `get-deployment-history`) |
| projectKey | string | yes | Use `$PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "errors": {}
}
```

Rollback has been initiated. The service will redeploy the artifact from the target deployment. Use `get-service` to verify the service returns to `Running` status.

---

## On Failure

```json
{
  "isSuccess": false,
  "errors": {
    "deploymentId": "Deployment not found or not eligible for rollback"
  }
}
```

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | Validation error — deployment not rollbackable | Verify `deploymentId` from `get-deployment-history`; only `Succeeded` deployments can be rolled back to |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `API_BASE_URL` | Check environment URL in Cloud Portal |
