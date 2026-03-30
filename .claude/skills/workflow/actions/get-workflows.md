# Action: get-workflows

## Purpose

Get a paginated list of all workflows for the project. Use this to populate the workflow management page and to find a workflow's `workflowId` before viewing, updating, activating, or deleting.

---

## Endpoint

```
GET $API_BASE_URL/workflow/v1/Workflow/Gets
```

---

## curl

```bash
curl --location "$API_BASE_URL/workflow/v1/Workflow/Gets?projectKey=$PROJECT_SLUG&page=1&pageSize=20" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| projectKey | string | yes | Use `$PROJECT_SLUG` |
| page | integer | yes | 1-based page number |
| pageSize | integer | yes | Records per page |

---

## On Success (200)

```json
{
  "workflows": [
    {
      "workflowId": "wf-123",
      "name": "Email Processing Workflow",
      "description": "Triggers on webhook, processes with AI agent",
      "isActive": true,
      "createdDate": "2024-05-01T08:00:00Z",
      "lastUpdatedDate": "2024-06-01T10:00:00Z"
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

- Use `totalCount` for pagination controls.
- Use the `workflowId` from each workflow to call `get-workflow`, `update-workflow`, `activate-workflow`, `deactivate-workflow`, or `delete-workflow`.
- The list response does not include `nodes` and `edges` — use `get-workflow` for the full definition.
