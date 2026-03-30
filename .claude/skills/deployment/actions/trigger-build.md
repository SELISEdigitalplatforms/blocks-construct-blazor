# Action: trigger-build

## Purpose

Trigger a new build for a specific repository and branch. The build is queued and processed asynchronously — poll `get-build` to monitor status.

---

## Endpoint

```
POST $API_BASE_URL/deployment/v1/Build/Trigger
```

---

## curl

```bash
curl --location "$API_BASE_URL/deployment/v1/Build/Trigger" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "repositoryId": "repo-id-here",
    "branch": "main",
    "projectKey": "'"$PROJECT_SLUG"'"
  }'
```

---

## Request Fields

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| repositoryId | string | yes | ID of the repository attached in Cloud Portal |
| branch | string | yes | Git branch to build (e.g. `"main"`, `"develop"`) |
| projectKey | string | yes | Use `$PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "buildId": "string",
  "isSuccess": true,
  "errors": {}
}
```

Build has been queued. Use `get-build` with the returned `buildId` to monitor progress.

---

## On Failure

```json
{
  "isSuccess": false,
  "errors": {
    "repositoryId": "Repository not found",
    "branch": "Branch does not exist"
  }
}
```

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | Validation error on one or more fields | Inspect `errors` dictionary and correct the request |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `API_BASE_URL` | Check environment URL in Cloud Portal |
