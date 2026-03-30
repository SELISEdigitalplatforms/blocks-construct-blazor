# Action: get-build

## Purpose

Retrieve details of a single build by its ID. Use this to check build status, view commit info, and determine if a build succeeded or failed.

---

## Endpoint

```
GET $API_BASE_URL/deployment/v1/Build/Get?buildId={id}&projectKey=$PROJECT_SLUG
```

---

## curl

```bash
curl --location "$API_BASE_URL/deployment/v1/Build/Get?buildId=build-id-here&projectKey=$PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $X_BLOCKS_KEY" \
  --header "Content-Type: application/json"
```

---

## Query Parameters

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| buildId | string | yes | ID of the build to retrieve |
| projectKey | string | yes | Use `$PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "build": {
    "buildId": "string",
    "repositoryId": "string",
    "branch": "main",
    "status": "Succeeded",
    "startTime": "2024-01-01T00:00:00Z",
    "endTime": "2024-01-01T00:05:00Z",
    "commitHash": "abc123def",
    "commitMessage": "fix: resolve login issue"
  },
  "isSuccess": true,
  "errors": {}
}
```

**Build status values:** `Queued`, `InProgress`, `Succeeded`, `Failed`, `Cancelled`

---

## On Failure

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | Build not found | Verify `buildId` from `get-builds` response |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `API_BASE_URL` | Check environment URL in Cloud Portal |
