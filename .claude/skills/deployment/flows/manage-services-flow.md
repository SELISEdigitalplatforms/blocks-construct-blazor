# Flow: manage-services-flow

## Trigger

User wants to view, configure, or manage deployed services — including viewing deployment history and performing rollbacks.

> "list my services"
> "show service details"
> "scale my service to 3 replicas"
> "update environment variables"
> "view deployment history"
> "rollback to the previous version"

---

## Pre-flight Questions

Before starting, confirm:

1. Which service are you managing? (list available services if unknown)
2. What do you want to do? (view details / update config / view history / rollback)

---

## Flow Steps

### Step 1 — List Services

Get an overview of all services in the project:

```
Action: get-services
Input:
  projectKey = $PROJECT_SLUG
Output: list of services with name, status, replicas, repository, branch
```

Present the list and ask the user to select a service if not already specified.

---

### Step 2 — View Service Details

```
Action: get-service
Input:
  serviceId  = selected service ID
  projectKey = $PROJECT_SLUG
```

Display service name, status, replica count, last deployed time, and associated repository/branch.

---

### Step 3 — Update Configuration (if requested)

If the user wants to change replicas or environment variables:

```
Action: update-service-config
Input:
  serviceId  = selected service ID
  replicas   = desired replica count (e.g. 3)
  envVars    = { "KEY": "value" }
  projectKey = $PROJECT_SLUG
```

On `isSuccess: true` → confirm configuration updated.
On `isSuccess: false` → inspect `errors` and surface field messages.

---

### Step 4 — View Deployment History (if requested)

```
Action: get-deployment-history
Input:
  serviceId  = selected service ID
  projectKey = $PROJECT_SLUG
  page       = 1
  pageSize   = 20
```

Display deployment list with status, deployed time, version, and who deployed it.

---

### Step 5 — Rollback (if requested)

If the user wants to rollback to a previous deployment:

1. Show deployment history (Step 4) if not already displayed
2. Ask user to select the target deployment
3. Execute rollback:

```
Action: rollback-deployment
Input:
  serviceId    = selected service ID
  deploymentId = target deployment ID from history
  projectKey   = $PROJECT_SLUG
```

On `isSuccess: true` → confirm rollback initiated. Monitor service status.
On `isSuccess: false` → inspect `errors` and surface the issue.

After rollback, verify the service is running:

```
Action: get-service
Input:
  serviceId  = selected service ID
  projectKey = $PROJECT_SLUG
```

Confirm status is `Running`.

---

## Error Handling

| Error | Cause | Action |
|-------|-------|--------|
| Empty service list | No services deployed yet | Deploy code first using `deploy-code-flow` |
| `isSuccess: false` on config update | Invalid replicas or env var format | Check field constraints (replicas: 1-10) |
| `isSuccess: false` on rollback | Target deployment not found or not rollbackable | Verify deployment ID from history |
| Service status `Failed` after rollback | Rollback target has issues | Try an older deployment or redeploy |
| `401` | `ACCESS_TOKEN` expired | Re-run `get-token` |
| `403` | Missing `cloudadmin` role | Verify role in Cloud Portal |
| `404` | Service or deployment not found | Verify IDs from the list endpoints |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/deployment/pages/service-list/service-list-page.tsx` | Service overview with status and actions |
| `modules/deployment/pages/service-detail/service-detail-page.tsx` | Service detail with config editor and history |
| `modules/deployment/pages/deployment-history/deployment-history-page.tsx` | Paginated deployment history table |
| `modules/deployment/components/service-config-form/service-config-form.tsx` | Form for replicas and env var editing |
| `modules/deployment/components/rollback-dialog/rollback-dialog.tsx` | Confirmation dialog for rollback |
| `modules/deployment/hooks/use-deployment.tsx` | `useGetServices`, `useGetService`, `useUpdateServiceConfig`, `useGetDeploymentHistory`, `useRollbackDeployment` hooks |
