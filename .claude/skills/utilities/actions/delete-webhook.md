# Action: delete-webhook

## Purpose

Permanently delete a webhook by its ID. This action is irreversible — confirm with the user before calling.

---

## Endpoint

```
DELETE $API_BASE_URL/utilities/v1/Webhook/Delete
```

---

## curl

```bash
curl --location --request DELETE "$API_BASE_URL/utilities/v1/Webhook/Delete?webhookId=webhook-id-123&projectKey=$PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| webhookId | string | yes | ID of the webhook to delete — from `get-webhooks` |
| projectKey | string | yes | Use `$PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "errors": {}
}
```

The webhook is permanently deleted and will no longer receive event payloads.

---

## On Failure

```json
{
  "isSuccess": false,
  "errors": {
    "webhookId": "Webhook not found"
  }
}
```

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | Webhook not found for the given `webhookId` | Verify the ID from `get-webhooks` |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Notes

- Deletion is permanent and cannot be undone. Always show a confirmation dialog before calling this action in the frontend.
- After successful deletion, invalidate the `['webhooks']` React Query cache to refresh the list.
- Any events that fire after deletion will not be delivered to the deleted webhook's target URL.
