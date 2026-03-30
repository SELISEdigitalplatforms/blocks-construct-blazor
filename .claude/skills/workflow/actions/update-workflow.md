# Action: update-workflow

## Purpose

Update an existing workflow's name, description, nodes, and edges. Uses the same endpoint as create — include `workflowId` to trigger an update instead of a create.

---

## Endpoint

```
POST $API_BASE_URL/workflow/v1/Workflow/Save
```

---

## curl

```bash
curl --location "$API_BASE_URL/workflow/v1/Workflow/Save" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "workflowId": "wf-123",
    "name": "Email Processing Workflow v2",
    "description": "Updated workflow with additional HTTP request node",
    "nodes": [
      {
        "id": "node-1",
        "name": "Webhook Trigger",
        "type": "webhook",
        "position": { "x": 100, "y": 200 },
        "config": {}
      },
      {
        "id": "node-2",
        "name": "AI Agent",
        "type": "aiAgent",
        "position": { "x": 400, "y": 200 },
        "config": { "agentId": "agent-id-123" }
      },
      {
        "id": "node-3",
        "name": "HTTP Callback",
        "type": "httpRequest",
        "position": { "x": 700, "y": 200 },
        "config": {
          "method": "POST",
          "url": "https://api.example.com/callback",
          "headers": { "Content-Type": "application/json" },
          "body": "{\"result\": \"{{$json.output.summary}}\"}",
          "queryParams": {}
        }
      }
    ],
    "edges": [
      { "id": "edge-1", "source": "node-1", "target": "node-2" },
      { "id": "edge-2", "source": "node-2", "target": "node-3" }
    ],
    "projectKey": "'"$PROJECT_SLUG"'"
  }'
```

---

## Request Fields

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| workflowId | string | yes | ID of the workflow to update — from `get-workflows` or `create-workflow` |
| name | string | yes | Updated workflow name |
| description | string | no | Updated description |
| nodes | array | yes | Full array of nodes (replaces all existing nodes) |
| edges | array | yes | Full array of edges (replaces all existing edges) |
| projectKey | string | yes | Use `$PROJECT_SLUG` |

> **Important:** The update replaces the entire node and edge set. Always send the complete workflow definition, not just the changed parts.

---

## On Success (200)

```json
{
  "workflowId": "wf-123",
  "isSuccess": true,
  "errors": {}
}
```

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
| 200 with `isSuccess: false` | Workflow not found, invalid nodes, or missing required fields | Inspect `errors` |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Notes

- Always fetch the current workflow via `get-workflow` before updating to avoid overwriting changes made by other users.
- If the workflow is currently active, the updated definition takes effect immediately.
- After updating, invalidate the `['workflows']` and `['workflow', workflowId]` query caches.
