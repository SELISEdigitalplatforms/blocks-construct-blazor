# Action: activate-workflow

## Purpose

Activate a workflow so it starts responding to its configured trigger (webhook or email). Only active workflows process incoming trigger events.

---

## Endpoint

```
POST $API_BASE_URL/workflow/v1/Workflow/Activate
```

---

## curl

```bash
curl --location "$API_BASE_URL/workflow/v1/Workflow/Activate" \
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
| workflowId | string | yes | ID of the workflow to activate |
| projectKey | string | yes | Use `$PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "errors": {}
}
```

The workflow is now active and will respond to its configured trigger.

---

## On Failure

```json
{
  "isSuccess": false,
  "errors": {
    "workflowId": "Workflow not found or invalid configuration"
  }
}
```

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | Workflow not found, already active, or misconfigured nodes | Inspect `errors`; verify workflow has a valid trigger and all nodes are configured |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Notes

- A workflow must have at least one trigger node and valid edge connections before it can be activated.
- For webhook triggers, the webhook URL becomes live after activation.
- For email triggers, the IMAP polling begins after activation.
- After successful activation, invalidate the `['workflows']` and `['workflow', workflowId]` query caches.
