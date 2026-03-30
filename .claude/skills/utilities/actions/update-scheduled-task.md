# Action: update-scheduled-task

## Purpose

Update an existing scheduled task. Include `taskId` to identify the task to modify. All fields are replaced — send the complete task definition, not just the changed fields.

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
    "taskId": "task-id-123",
    "name": "Hourly Queue Processor v2",
    "cronExpression": "0 */2 * * *",
    "targetUrl": "https://api.example.com/process",
    "httpMethod": "POST",
    "headers": {
      "Authorization": "Bearer some-token"
    },
    "body": {
      "action": "process-queue",
      "batchSize": 200
    },
    "isActive": true,
    "projectKey": "'"$PROJECT_SLUG"'"
  }'
```

---

## Request Fields

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| taskId | string | yes | ID of the task to update — from `get-scheduled-tasks` |
| name | string | yes | Human-readable name for the task |
| cronExpression | string | yes | Standard 5-field cron expression |
| targetUrl | string | yes | Full URL to call when the task fires |
| httpMethod | string | yes | HTTP method — `GET`, `POST`, `PUT`, `DELETE` |
| headers | object | no | Key/value pairs sent as HTTP headers on the target request |
| body | object | no | Request body sent to the target URL (for POST/PUT) |
| isActive | boolean | no | Whether the task is active |
| projectKey | string | yes | Use `$PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "errors": {}
}
```

The scheduled task is updated. The new cron schedule takes effect immediately.

---

## On Failure

```json
{
  "isSuccess": false,
  "errors": {
    "taskId": "Task not found"
  }
}
```

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | Task not found or invalid fields | Verify `taskId` from `get-scheduled-tasks`; inspect `errors` |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Notes

- This uses the same `Save` endpoint as `create-scheduled-task`. The presence of `taskId` determines whether the operation is a create or update.
- All fields are replaced on update — always load the current task via `get-scheduled-tasks` first, modify the desired fields, and send the complete object.
- To deactivate a task without deleting it, set `isActive` to `false`.
