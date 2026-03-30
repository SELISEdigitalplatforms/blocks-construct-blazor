# Action: delete-workflow

## Purpose

Permanently delete a workflow by its ID. This action is irreversible — confirm with the user before calling. Active workflows should be deactivated before deletion.

---

## Endpoint

```
DELETE $API_BASE_URL/workflow/v1/Workflow/Delete
```

---

## curl

```bash
curl --location --request DELETE "$API_BASE_URL/workflow/v1/Workflow/Delete?workflowId=wf-123&projectKey=$PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| workflowId | string | yes | ID of the workflow to delete — from `get-workflows` |
| projectKey | string | yes | Use `$PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "errors": {}
}
```

The workflow is permanently deleted along with its execution history.

---

## On Failure

```json
{
  "isSuccess": false,
  "errors": {
    "workflowId": "Workflow not found"
  }
}
```

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | Workflow not found for the given `workflowId` | Verify the ID from `get-workflows` |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Notes

- Deletion is permanent and cannot be undone. Always show a confirmation dialog before calling this action in the frontend.
- Deactivate the workflow before deleting to avoid processing triggers during deletion.
- After successful deletion, invalidate the `['workflows']` React Query cache to refresh the list.
- Execution history for the deleted workflow is also removed.
