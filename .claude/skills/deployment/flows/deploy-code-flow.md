# Flow: deploy-code-flow

## Trigger

User wants to deploy code — selecting a repository and branch, triggering a build, and deploying the result.

> "deploy my code"
> "build and deploy the main branch"
> "trigger a deployment"
> "push my code to production"
> "deploy the latest changes"

---

## Pre-flight Questions

Before starting, confirm:

1. Which repository should be built? (list available repositories if unknown)
2. Which branch should be built? (e.g. `main`, `develop`, `feature/xyz`)
3. Should the build auto-deploy on success? (yes/no — default: yes)

---

## Flow Steps

### Step 1 — Select Repository and Branch

Identify the target repository and branch. If the user hasn't specified:

- Use `get-services` to list available services and their associated repositories
- Ask the user to confirm the repository and branch

```
Action: get-services
Input:
  projectKey = $PROJECT_SLUG
Output: list of services with repository and branch info
```

---

### Step 2 — Trigger Build

```
Action: trigger-build
Input:
  repositoryId = selected repository ID
  branch       = selected branch name
  projectKey   = $PROJECT_SLUG
```

On `isSuccess: true` → capture the `buildId` and proceed to Step 3.
On `isSuccess: false` → inspect `errors` and surface field messages.

---

### Step 3 — Monitor Build Status

Poll the build status until it reaches a terminal state (`Succeeded`, `Failed`, `Cancelled`).

```
Action: get-build
Input:
  buildId    = buildId from Step 2
  projectKey = $PROJECT_SLUG
```

Poll every 10 seconds. Report status transitions to the user:
- `Queued` → "Build is queued..."
- `InProgress` → "Build is running..."
- `Succeeded` → proceed to Step 4
- `Failed` → stop and report failure details
- `Cancelled` → stop and inform user

---

### Step 4 — Deploy on Success

If auto-deploy was confirmed in pre-flight and the build succeeded, the platform automatically deploys the build artifact.

Verify the deployment was triggered by checking deployment history:

```
Action: get-deployment-history
Input:
  serviceId  = service associated with the repository
  projectKey = $PROJECT_SLUG
  page       = 1
  pageSize   = 5
```

Look for a new deployment entry with the matching `buildId`.

---

### Step 5 — Verify Deployment

Confirm the deployment reached `Succeeded` status:

```
Action: get-service
Input:
  serviceId  = target service ID
  projectKey = $PROJECT_SLUG
```

Check the service status is `Running` and `lastDeployedAt` is recent.

> Deployment verified. Service is running with the latest build.

---

## Error Handling

| Error | Cause | Action |
|-------|-------|--------|
| `isSuccess: false` on trigger-build | Missing repository or branch | Verify repository is attached in Cloud Portal |
| Build status `Failed` | Build errors (compilation, tests) | Check build logs for details |
| Build status `Cancelled` | Build was manually cancelled | Re-trigger if needed |
| Deployment not appearing in history | Auto-deploy disabled or platform delay | Wait and re-check, or trigger manually |
| Service status `Failed` after deploy | Runtime error in new version | Use `rollback-deployment` to revert |
| `401` | `ACCESS_TOKEN` expired | Re-run `get-token` |
| `403` | Missing `cloudadmin` role | Verify role in Cloud Portal |
| `404` | Wrong `API_BASE_URL` or project not found | Check environment URL in Cloud Portal |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/deployment/pages/build-list/build-list-page.tsx` | Build list with status badges and trigger button |
| `modules/deployment/components/build-status/build-status.tsx` | Real-time build status indicator |
| `modules/deployment/hooks/use-deployment.tsx` | `useTriggerBuild`, `useGetBuild`, `useGetBuilds` hooks |
| `modules/deployment/services/deployment.service.ts` | API calls for build operations |
| `modules/deployment/types/deployment.type.ts` | `Build`, `TriggerBuildPayload` interfaces |
