# Utilities Contracts

## Common Headers (all authenticated requests)

```
Authorization: Bearer $ACCESS_TOKEN
x-blocks-key: $X_BLOCKS_KEY
Content-Type: application/json
```

---

## Common Response: BaseResponse

```json
{
  "isSuccess": true,
  "errors": {
    "fieldName": "error message"
  }
}
```

> `errors` is a **dictionary** (key = field name, value = error message), not an array.
> When `isSuccess` is `false`, inspect `errors` to identify which field caused the failure.

---

## ScheduledTask

### CreateScheduledTaskRequest

Used to create a new scheduled task. Omit `taskId` for creation; include it for updates.

```json
{
  "name": "string",
  "cronExpression": "string",
  "targetUrl": "string",
  "httpMethod": "string",
  "headers": {},
  "body": {},
  "isActive": true,
  "projectKey": "string"
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| name | string | yes | Human-readable name for the task |
| cronExpression | string | yes | Standard cron expression (e.g. `"0 */1 * * *"` for every hour) |
| targetUrl | string | yes | Full URL to call when the task fires |
| httpMethod | string | yes | HTTP method — `GET`, `POST`, `PUT`, `DELETE` |
| headers | object | no | Key/value pairs sent as HTTP headers on the target request |
| body | object | no | Request body sent to the target URL (for POST/PUT) |
| isActive | boolean | no | Whether the task is active immediately; defaults to `true` |
| projectKey | string | yes | Project identifier from `$PROJECT_SLUG` |

---

### UpdateScheduledTaskRequest

Same as `CreateScheduledTaskRequest` but includes `taskId` to identify the task to update.

```json
{
  "taskId": "string",
  "name": "string",
  "cronExpression": "string",
  "targetUrl": "string",
  "httpMethod": "string",
  "headers": {},
  "body": {},
  "isActive": true,
  "projectKey": "string"
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| taskId | string | yes | ID of the task to update — from `get-scheduled-tasks` |
| name | string | yes | Human-readable name for the task |
| cronExpression | string | yes | Standard cron expression |
| targetUrl | string | yes | Full URL to call when the task fires |
| httpMethod | string | yes | HTTP method — `GET`, `POST`, `PUT`, `DELETE` |
| headers | object | no | Key/value pairs sent as HTTP headers on the target request |
| body | object | no | Request body sent to the target URL (for POST/PUT) |
| isActive | boolean | no | Whether the task is active |
| projectKey | string | yes | Project identifier |

---

### GetScheduledTasksResponse

Returned by `GET /ScheduledTask/Gets`.

```json
{
  "tasks": [
    {
      "taskId": "string",
      "name": "string",
      "cronExpression": "0 */1 * * *",
      "targetUrl": "https://api.example.com/process",
      "httpMethod": "POST",
      "isActive": true,
      "lastRunAt": "2024-01-01T00:00:00Z",
      "nextRunAt": "2024-01-01T01:00:00Z",
      "lastRunStatus": "success"
    }
  ],
  "totalCount": 5,
  "isSuccess": true,
  "errors": {}
}
```

**Query parameters for `Gets`:**

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| projectKey | string | yes | Project identifier |

---

## Webhook

### CreateWebhookRequest

Used to register a new webhook subscription.

```json
{
  "name": "string",
  "targetUrl": "string",
  "events": ["string"],
  "headers": {},
  "isActive": true,
  "projectKey": "string"
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| name | string | yes | Human-readable name for the webhook |
| targetUrl | string | yes | URL that receives webhook POST payloads |
| events | string[] | yes | List of event types to subscribe to (e.g. `["user.created", "order.completed"]`) |
| headers | object | no | Custom headers sent with each webhook delivery |
| isActive | boolean | no | Whether the webhook is active immediately; defaults to `true` |
| projectKey | string | yes | Project identifier from `$PROJECT_SLUG` |

---

### GetWebhooksResponse

Returned by `GET /Webhook/Gets`.

```json
{
  "webhooks": [
    {
      "webhookId": "string",
      "name": "Order Webhook",
      "targetUrl": "https://api.example.com/webhook",
      "events": ["order.created", "order.completed"],
      "isActive": true,
      "createdDate": "2024-01-01T00:00:00Z"
    }
  ],
  "totalCount": 3,
  "isSuccess": true,
  "errors": {}
}
```

**Query parameters for `Gets`:**

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| projectKey | string | yes | Project identifier |

---

## Config

### ConfigResponse

Returned by `GET /Config/Gets`.

```json
{
  "settings": [
    {
      "key": "string",
      "value": "string",
      "description": "string"
    }
  ],
  "isSuccess": true,
  "errors": {}
}
```

**Query parameters for `Gets`:**

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| projectKey | string | yes | Project identifier |

---

### UpdateConfigRequest

Used to update one or more configuration settings.

```json
{
  "settings": [
    {
      "key": "string",
      "value": "string"
    }
  ],
  "projectKey": "string"
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| settings | array | yes | List of key/value pairs to update |
| settings[].key | string | yes | Configuration key name |
| settings[].value | string | yes | New value for the key |
| projectKey | string | yes | Project identifier from `$PROJECT_SLUG` |
