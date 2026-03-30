# Action: get-builds

## Purpose

Retrieve a paginated list of builds for the project. Use this to view build history and find specific builds by status.

---

## Endpoint

```
GET $API_BASE_URL/deployment/v1/Build/Gets?projectKey=$PROJECT_SLUG&page=1&pageSize=20
```

---

## curl

```bash
curl --location "$API_BASE_URL/deployment/v1/Build/Gets?projectKey=$PROJECT_SLUG&page=1&pageSize=20" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $X_BLOCKS_KEY" \
  --header "Content-Type: application/json"
```

---

## Query Parameters

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| projectKey | string | yes | Use `$PROJECT_SLUG` |
| page | integer | yes | 1-based page number |
| pageSize | integer | yes | Records per page (max: 100) |

---

## On Success (200)

```json
{
  "builds": [
    {
      "buildId": "string",
      "repositoryId": "string",
      "branch": "main",
      "status": "Succeeded",
      "startTime": "2024-01-01T00:00:00Z",
      "endTime": "2024-01-01T00:05:00Z",
      "commitHash": "abc123def",
      "commitMessage": "fix: resolve login issue"
    }
  ],
  "totalCount": 100,
  "isSuccess": true,
  "errors": {}
}
```

**Build status values:** `Queued`, `InProgress`, `Succeeded`, `Failed`, `Cancelled`

---

## On Failure

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `API_BASE_URL` or project not found | Check environment URL in Cloud Portal |
