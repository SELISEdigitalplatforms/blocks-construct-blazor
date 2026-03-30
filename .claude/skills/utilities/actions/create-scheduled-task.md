# Action: create-scheduled-task

## Purpose

Create a new scheduled task (cron job) that calls a specified endpoint on a recurring schedule.

---

## Endpoint

```
POST $API_BASE_URL/utilities/v1/ScheduledTask/Save
```

---

## curl

```bash
curl --location "$API_BASE_URL/utilities/v1/ScheduledTask/Save" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "name": "Hourly Queue Processor",
    "cronExpression": "0 * * * *",
    "targetUrl": "https://api.example.com/process",
    "httpMethod": "POST",
    "headers": {
      "Authorization": "Bearer some-token"
    },
    "body": {
      "action": "process-queue"
    },
    "isActive": true,
    "projectKey": "'"$PROJECT_SLUG"'"
  }'
```

---

## Request Fields

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| name | string | yes | Human-readable name for the task |
| cronExpression | string | yes | Standard 5-field cron expression (e.g. `"0 * * * *"` for every hour) |
| targetUrl | string | yes | Full URL to call when the task fires |
| httpMethod | string | yes | HTTP method — `GET`, `POST`, `PUT`, `DELETE` |
| headers | object | no | Key/value pairs sent as HTTP headers on the target request |
| body | object | no | Request body sent to the target URL (for POST/PUT) |
| isActive | boolean | no | Whether the task is active immediately; defaults to `true` |
| projectKey | string | yes | Use `$PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "errors": {}
}
```

The scheduled task is created and will begin firing on the defined cron schedule.

---

## On Failure

```json
{
  "isSuccess": false,
  "errors": {
    "cronExpression": "Invalid cron expression",
    "targetUrl": "URL is not reachable"
  }
}
```

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | Invalid fields — inspect `errors` for details | Fix the flagged fields |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Notes

- Omit `taskId` to create a new task. To update an existing task, use `update-scheduled-task` which includes `taskId`.
- The cron expression uses standard 5-field format: `minute hour day-of-month month day-of-week`.
- After successful creation, use `get-scheduled-tasks` to verify the task was created and retrieve its `taskId`.
