# Flow: manage-webhooks-flow

## Trigger

User wants to create, view, or manage webhooks for event-driven notifications.

> "set up a webhook"
> "create a webhook for user events"
> "manage my webhooks"
> "register a webhook endpoint"
> "subscribe to order events via webhook"

---

## Pre-flight Questions

Before starting, confirm:

1. What is the purpose of this webhook? (What events should trigger it?)
2. What is the target URL that should receive webhook payloads?
3. Which events should this webhook subscribe to? (e.g. `user.created`, `order.completed`)
4. Does the target endpoint require custom headers (e.g. a verification token)?
5. Should the webhook be active immediately?

---

## Flow Steps

### Step 1 — Define Webhook Name

Choose a descriptive name for the webhook based on its purpose:

```
Input:
  name = "Order Event Webhook"
```

---

### Step 2 — Set Target URL

```
Input:
  targetUrl = "https://api.example.com/webhooks/orders"
```

Validate that:
- `targetUrl` is a fully qualified URL (starts with `https://`)
- The endpoint is accessible and can accept POST requests

---

### Step 3 — Select Events to Subscribe To

Determine which events the webhook should listen for:

```
Input:
  events = ["order.created", "order.completed", "order.cancelled"]
```

Events are string identifiers — consult the project's event documentation for available event types.

---

### Step 4 — Create the Webhook

```
Action: create-webhook
Input:
  name       = "Order Event Webhook"
  targetUrl  = "https://api.example.com/webhooks/orders"
  events     = ["order.created", "order.completed", "order.cancelled"]
  headers    = { "X-Webhook-Secret": "my-secret-token" }
  isActive   = true
  projectKey = $PROJECT_SLUG
```

On `isSuccess: true` → webhook registered. Run `get-webhooks` to confirm and retrieve the `webhookId`.
On `isSuccess: false` → inspect `errors` and correct the request.

---

### Step 5 — Verify Webhook

```
Action: get-webhooks
Input:
  projectKey = $PROJECT_SLUG
Output:
  webhooks[] → verify the new webhook appears with correct events and isActive: true
```

Confirm to the user:

> Webhook "{name}" created successfully. It will receive POST payloads at {targetUrl} when any of these events fire: {events}.

---

## Error Handling

| Error | Cause | Action |
|-------|-------|--------|
| `isSuccess: false` with `targetUrl` error | Invalid or unreachable URL | Check URL format and accessibility |
| `isSuccess: false` with `events` error | No events specified or invalid event names | Verify event identifiers |
| `isSuccess: false` with `name` error | Duplicate webhook name | Choose a unique webhook name |
| `401` | `ACCESS_TOKEN` expired | Re-run `get-token` |
| `403` | Missing `cloudadmin` role | Verify role in Cloud Portal → People |
| `404` | Wrong `API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/utilities/pages/webhooks/webhook-editor-page.tsx` | Form for creating a webhook with event selection |
| `modules/utilities/pages/webhooks/webhooks-page.tsx` | List all webhooks with status and row actions (Delete) |
| `modules/utilities/hooks/use-utilities.tsx` | `useCreateWebhook`, `useGetWebhooks`, `useDeleteWebhook` hooks |
| `modules/utilities/services/utilities.service.ts` | `createWebhook()`, `getWebhooks()`, `deleteWebhook()` functions |
| `modules/utilities/types/utilities.type.ts` | `CreateWebhookPayload`, `Webhook` interfaces |
