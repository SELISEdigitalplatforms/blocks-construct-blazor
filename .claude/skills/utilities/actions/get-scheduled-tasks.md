# Action: get-scheduled-tasks

## Purpose

List all scheduled tasks for the project. Use this to view task status, check next run times, and retrieve `taskId` values for update or delete operations.

---

## Endpoint

```
GET $API_BASE_URL/utilities/v1/ScheduledTask/Gets
```

---

## curl

```bash
curl --location "$API_BASE_URL/utilities/v1/ScheduledTask/Gets?projectKey=$PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| projectKey | string | yes | Use `$PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "tasks": [
    {
      "taskId": "task-id-123",
      "name": "Hourly Queue Processor",
      "cronExpression": "0 * * * *",
      "targetUrl": "https://api.example.com/process",
      "httpMethod": "POST",
      "isActive": true,
      "lastRunAt": "2024-01-01T12:00:00Z",
      "nextRunAt": "2024-01-01T13:00:00Z",
      "lastRunStatus": "success"
    }
  ],
  "totalCount": 1,
  "isSuccess": true,
  "errors": {}
}
```

---

## On Failure

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | Missing `projectKey` | Inspect `errors` |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Notes

- All tasks for the project are returned in a single response. Use `totalCount` for display purposes.
- Use the `taskId` from each task to call `update-scheduled-task` or `delete-scheduled-task`.
- `lastRunStatus` indicates the outcome of the most recent execution — values include `success`, `failed`, and `pending`.
