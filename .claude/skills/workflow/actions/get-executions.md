# Action: get-executions

## Purpose

Get a paginated list of execution history for a specific workflow. Use this to display the execution history page and monitor workflow runs.

---

## Endpoint

```
GET $API_BASE_URL/workflow/v1/Execution/Gets
```

---

## curl

```bash
curl --location "$API_BASE_URL/workflow/v1/Execution/Gets?workflowId=wf-123&projectKey=$PROJECT_SLUG&page=1&pageSize=20" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| workflowId | string | yes | ID of the workflow to get executions for |
| projectKey | string | yes | Use `$PROJECT_SLUG` |
| page | integer | yes | 1-based page number |
| pageSize | integer | yes | Records per page |

---

## On Success (200)

```json
{
  "executions": [
    {
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
          "output": {},
          "startTime": "2024-06-01T10:00:00Z",
          "endTime": "2024-06-01T10:00:01Z"
        }
      ]
    }
  ],
  "totalCount": 50,
  "isSuccess": true,
  "errors": {}
}
```

---

## On Failure

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | Missing `workflowId` or `projectKey` | Inspect `errors` |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Notes

- Use `totalCount` for pagination controls.
- Status values: `pending`, `running`, `completed`, `failed`, `cancelled`.
- Click an execution to call `get-execution` for the full per-node breakdown.
- Results are ordered by `startTime` descending (most recent first).
