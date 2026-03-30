# Action: get-execution

## Purpose

Get the full details of a specific workflow execution, including per-node results with input/output data and timing. Use this to debug failed executions or inspect what each node produced.

---

## Endpoint

```
GET $API_BASE_URL/workflow/v1/Execution/Get
```

---

## curl

```bash
curl --location "$API_BASE_URL/workflow/v1/Execution/Get?executionId=exec-456&projectKey=$PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| executionId | string | yes | ID of the execution to retrieve |
| projectKey | string | yes | Use `$PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "execution": {
    "executionId": "exec-456",
    "workflowId": "wf-123",
    "status": "completed",
    "startTime": "2024-06-01T10:00:00Z",
    "endTime": "2024-06-01T10:00:05Z",
    "nodeResults": [
      {
        "nodeId": "node-1",
        "nodeName": "Webhook Trigger",
        "status": "completed",
        "output": { "payload": { "orderId": "ORD-001", "email": "user@example.com" } },
        "startTime": "2024-06-01T10:00:00Z",
        "endTime": "2024-06-01T10:00:01Z"
      },
      {
        "nodeId": "node-2",
        "nodeName": "AI Agent",
        "status": "completed",
        "output": { "summary": "Order ORD-001 processed successfully", "sentiment": "positive" },
        "startTime": "2024-06-01T10:00:01Z",
        "endTime": "2024-06-01T10:00:05Z"
      }
    ]
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
    "executionId": "Execution not found"
  }
}
```

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | Execution not found for the given `executionId` | Verify the ID from `get-executions` |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Notes

- Each `nodeResults` entry shows what a specific node produced during execution.
- For failed nodes, the `output` field may contain error details.
- Use this data to debug workflow issues — check which node failed and what input it received.
- Display node outputs in a collapsible JSON viewer in the frontend for easy inspection.
