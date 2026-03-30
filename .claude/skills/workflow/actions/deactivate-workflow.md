# Action: deactivate-workflow

## Purpose

Deactivate a workflow so it stops responding to its configured trigger. The workflow definition is preserved and can be reactivated later.

---

## Endpoint

```
POST $API_BASE_URL/workflow/v1/Workflow/Deactivate
```

---

## curl

```bash
curl --location "$API_BASE_URL/workflow/v1/Workflow/Deactivate" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "workflowId": "wf-123",
    "projectKey": "'"$PROJECT_SLUG"'"
  }'
```

---

## Request Fields

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| workflowId | string | yes | ID of the workflow to deactivate |
| projectKey | string | yes | Use `$PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "errors": {}
}
```

The workflow is now inactive and will not respond to triggers.

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
| 200 with `isSuccess: false` | Workflow not found or already inactive | Inspect `errors` |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Notes

- Deactivation does not delete the workflow or its execution history.
- Any currently running executions will complete — deactivation only prevents new triggers from starting.
- After successful deactivation, invalidate the `['workflows']` and `['workflow', workflowId]` query caches.
