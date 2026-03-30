# Action: get-webhooks

## Purpose

List all webhooks registered for the project. Use this to view webhook configurations and retrieve `webhookId` values for delete operations.

---

## Endpoint

```
GET $API_BASE_URL/utilities/v1/Webhook/Gets
```

---

## curl

```bash
curl --location "$API_BASE_URL/utilities/v1/Webhook/Gets?projectKey=$PROJECT_SLUG" \
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
  "webhooks": [
    {
      "webhookId": "webhook-id-123",
      "name": "Order Event Webhook",
      "targetUrl": "https://api.example.com/webhooks/orders",
      "events": ["order.created", "order.completed"],
      "isActive": true,
      "createdDate": "2024-01-01T00:00:00Z"
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

- All webhooks for the project are returned in a single response. Use `totalCount` for display purposes.
- Use the `webhookId` from each webhook to call `delete-webhook`.
