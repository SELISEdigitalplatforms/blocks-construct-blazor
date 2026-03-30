# Action: create-webhook

## Purpose

Register a new webhook that will receive POST payloads when specified events occur in the project.

---

## Endpoint

```
POST $API_BASE_URL/utilities/v1/Webhook/Save
```

---

## curl

```bash
curl --location "$API_BASE_URL/utilities/v1/Webhook/Save" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "name": "Order Event Webhook",
    "targetUrl": "https://api.example.com/webhooks/orders",
    "events": ["order.created", "order.completed"],
    "headers": {
      "X-Webhook-Secret": "my-secret-token"
    },
    "isActive": true,
    "projectKey": "'"$PROJECT_SLUG"'"
  }'
```

---

## Request Fields

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| name | string | yes | Human-readable name for the webhook |
| targetUrl | string | yes | URL that receives webhook POST payloads |
| events | string[] | yes | List of event types to subscribe to (e.g. `["user.created", "order.completed"]`) |
| headers | object | no | Custom headers sent with each webhook delivery (e.g. secret tokens) |
| isActive | boolean | no | Whether the webhook is active immediately; defaults to `true` |
| projectKey | string | yes | Use `$PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "errors": {}
}
```

The webhook is registered and will begin receiving payloads when subscribed events fire.

---

## On Failure

```json
{
  "isSuccess": false,
  "errors": {
    "targetUrl": "URL is not valid",
    "events": "At least one event must be specified"
  }
}
```

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | Invalid fields — inspect `errors` for details | Fix the flagged fields |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Notes

- After successful creation, use `get-webhooks` to verify the webhook was registered and retrieve its `webhookId`.
- Webhook deliveries are POST requests containing a JSON payload describing the event. The target endpoint should return a 2xx status to acknowledge receipt.
- Use custom headers (e.g. `X-Webhook-Secret`) to verify that incoming requests are genuine webhook deliveries.
