# Action: delete-scheduled-task

## Purpose

Permanently delete a scheduled task by its ID. This action is irreversible — confirm with the user before calling.

---

## Endpoint

```
DELETE $API_BASE_URL/utilities/v1/ScheduledTask/Delete
```

---

## curl

```bash
curl --location --request DELETE "$API_BASE_URL/utilities/v1/ScheduledTask/Delete?taskId=task-id-123&projectKey=$PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| taskId | string | yes | ID of the task to delete — from `get-scheduled-tasks` |
| projectKey | string | yes | Use `$PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "errors": {}
}
```

The scheduled task is permanently deleted and will no longer fire.

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
| 200 with `isSuccess: false` | Task not found for the given `taskId` | Verify the ID from `get-scheduled-tasks` |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Notes

- Deletion is permanent and cannot be undone. Always show a confirmation dialog before calling this action in the frontend.
- After successful deletion, invalidate the `['scheduled-tasks']` React Query cache to refresh the list.
- To temporarily stop a task without deleting it, use `update-scheduled-task` with `isActive: false` instead.
