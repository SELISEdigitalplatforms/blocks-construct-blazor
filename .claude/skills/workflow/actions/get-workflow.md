# Action: get-workflow

## Purpose

Get the full definition of a specific workflow including all nodes, edges, and configuration. Use this to load a workflow into the visual editor or to inspect its structure before updating.

---

## Endpoint

```
GET $API_BASE_URL/workflow/v1/Workflow/Get
```

---

## curl

```bash
curl --location "$API_BASE_URL/workflow/v1/Workflow/Get?workflowId=wf-123&projectKey=$PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| workflowId | string | yes | ID of the workflow to retrieve |
| projectKey | string | yes | Use `$PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "workflow": {
    "workflowId": "wf-123",
    "name": "Email Processing Workflow",
    "description": "Triggers on webhook, processes with AI agent, sends summary email",
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
      }
    ],
    "edges": [
      {
        "id": "edge-1",
        "source": "node-1",
        "target": "node-2"
      }
    ],
    "isActive": true,
    "createdDate": "2024-05-01T08:00:00Z",
    "lastUpdatedDate": "2024-06-01T10:00:00Z"
  },
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
| 200 with `isSuccess: false` | Workflow not found for the given `workflowId` | Verify the ID from `get-workflows` |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Notes

- This returns the full workflow definition including `nodes` and `edges`, unlike `get-workflows` which only returns summary data.
- Use the returned data to populate the workflow editor canvas.
- Check `isActive` to determine if the workflow is currently responding to triggers.
