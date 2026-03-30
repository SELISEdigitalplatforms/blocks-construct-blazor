# Flow: setup-scheduled-task-flow

## Trigger

User wants to create and activate a scheduled task (cron job) that calls an endpoint on a recurring schedule.

> "create a scheduled task"
> "set up a cron job to call my API"
> "schedule a task to run every hour"
> "automate a recurring API call"
> "create a periodic background job"

---

## Pre-flight Questions

Before starting, confirm:

1. What should the task do? (What endpoint should it call?)
2. How often should it run? (e.g. every hour, daily at midnight, every 5 minutes)
3. Which HTTP method should the task use? (GET, POST, PUT, DELETE)
4. Does the target endpoint require custom headers (e.g. API keys)?
5. Does the target endpoint expect a request body? (for POST/PUT)

---

## Flow Steps

### Step 1 — Define Task Name and Schedule

Determine the task name and cron expression based on the user's requirements.

Common cron expressions:
| Schedule | Cron Expression |
|----------|----------------|
| Every minute | `* * * * *` |
| Every 5 minutes | `*/5 * * * *` |
| Every hour | `0 * * * *` |
| Every day at midnight | `0 0 * * *` |
| Every Monday at 9 AM | `0 9 * * 1` |
| First day of every month | `0 0 1 * *` |

---

### Step 2 — Configure Target URL and HTTP Method

```
Input:
  targetUrl  = "https://api.example.com/process"
  httpMethod = "POST"
```

Validate that:
- `targetUrl` is a fully qualified URL (starts with `https://`)
- `httpMethod` is one of `GET`, `POST`, `PUT`, `DELETE`

---

### Step 3 — Set Headers and Body

If the target endpoint requires authentication or custom headers:

```
Input:
  headers = {
    "Authorization": "Bearer some-token",
    "Content-Type": "application/json"
  }
  body = {
    "action": "process-queue",
    "batchSize": 100
  }
```

- Headers are optional — leave as `{}` if the endpoint does not require them
- Body is optional — leave as `{}` for GET/DELETE requests

---

### Step 4 — Create the Scheduled Task

```
Action: create-scheduled-task
Input:
  name           = "Hourly Queue Processor"
  cronExpression = "0 * * * *"
  targetUrl      = "https://api.example.com/process"
  httpMethod     = "POST"
  headers        = { "Authorization": "Bearer token" }
  body           = { "action": "process-queue" }
  isActive       = true
  projectKey     = $PROJECT_SLUG
```

On `isSuccess: true` → task created and active. Run `get-scheduled-tasks` to confirm and retrieve the `taskId`.
On `isSuccess: false` → inspect `errors` and correct the request.

---

### Step 5 — Verify Activation

```
Action: get-scheduled-tasks
Input:
  projectKey = $PROJECT_SLUG
Output:
  tasks[] → verify the new task appears with isActive: true and correct nextRunAt
```

Confirm to the user:

> Scheduled task "{name}" created successfully. It will run on the schedule: `{cronExpression}`. Next run: {nextRunAt}.

---

## Error Handling

| Error | Cause | Action |
|-------|-------|--------|
| `isSuccess: false` with `cronExpression` error | Invalid cron expression | Verify cron syntax — use a 5-field standard cron format |
| `isSuccess: false` with `targetUrl` error | Invalid or unreachable URL | Check URL format and accessibility |
| `isSuccess: false` with `name` error | Duplicate task name | Choose a unique task name |
| `401` | `ACCESS_TOKEN` expired | Re-run `get-token` |
| `403` | Missing `cloudadmin` role | Verify role in Cloud Portal → People |
| `404` | Wrong `API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/utilities/pages/scheduled-tasks/scheduled-task-editor-page.tsx` | Form for creating/editing a scheduled task with cron expression helper |
| `modules/utilities/hooks/use-utilities.tsx` | `useCreateScheduledTask`, `useGetScheduledTasks` hooks |
| `modules/utilities/services/utilities.service.ts` | `createScheduledTask()`, `getScheduledTasks()` functions |
| `modules/utilities/types/utilities.type.ts` | `CreateScheduledTaskPayload`, `ScheduledTask` interfaces |
