# Flow: manage-workflows-flow

## Trigger

User wants to list, view, activate, deactivate, or delete workflows, or view execution history.

> "show me all workflows"
> "list my workflows"
> "activate the order processing workflow"
> "deactivate a workflow"
> "show execution history"
> "delete a workflow"
> "what happened in the last workflow run?"

---

## Pre-flight Questions

Before starting, confirm:

1. Are you listing workflows, managing a specific workflow, or viewing execution history?
2. If managing a specific workflow — do you know the workflow ID, or should we list workflows first?
3. If viewing execution history — for which workflow?

---

## Flow Steps

### Step 1 — List All Workflows

Fetch the current workflow list to see what exists.

```
Action: get-workflows
Input:
  projectKey = $PROJECT_SLUG
  page       = 1
  pageSize   = 20
Output:
  workflows[]  → list of workflows with workflowId, name, description, isActive
  totalCount   → total number of workflows
```

Display workflows in a table with columns: Name, Description, Status (Active/Inactive), Created Date, Actions.

---

### Step 2 — View Workflow Details

When the user selects a workflow, load its full definition.

```
Action: get-workflow
Input:
  workflowId = "wf-123"
  projectKey = $PROJECT_SLUG
Output:
  workflow.name
  workflow.description
  workflow.nodes[]        → all nodes with their configurations
  workflow.edges[]        → all connections between nodes
  workflow.isActive
  workflow.createdDate
  workflow.lastUpdatedDate
```

Display the workflow in the visual editor canvas showing nodes and connections.

---

### Step 3 — Activate or Deactivate

**To activate:**

```
Action: activate-workflow
Input:
  workflowId = "wf-123"
  projectKey = $PROJECT_SLUG
```

On `isSuccess: true` → workflow is now active and listening for triggers.

**To deactivate:**

```
Action: deactivate-workflow
Input:
  workflowId = "wf-123"
  projectKey = $PROJECT_SLUG
```

On `isSuccess: true` → workflow is now inactive and will not respond to triggers.

After either action, refresh the workflow list to reflect the updated status.

---

### Step 4 — View Execution History

Fetch the execution history for a specific workflow.

```
Action: get-executions
Input:
  workflowId = "wf-123"
  projectKey = $PROJECT_SLUG
  page       = 1
  pageSize   = 20
Output:
  executions[]  → list of executions with executionId, status, startTime, endTime
  totalCount    → total number of executions
```

Display executions in a table with columns: Execution ID, Status, Start Time, End Time, Duration.

Color-code status: `completed` = green, `failed` = red, `running` = blue, `pending` = gray, `cancelled` = yellow.

---

### Step 5 — View Execution Detail

When the user clicks an execution, load the full details.

```
Action: get-execution
Input:
  executionId = "exec-456"
  projectKey  = $PROJECT_SLUG
Output:
  execution.executionId
  execution.workflowId
  execution.status
  execution.startTime
  execution.endTime
  execution.nodeResults[]  → per-node status, output, timing
```

Display each node's execution result with:
- Node name and status
- Input/output data (collapsible JSON viewer)
- Execution duration per node
- Error details for failed nodes

---

### Step 6 — Delete a Workflow

Show a confirmation dialog before proceeding.

```
Action: delete-workflow
Input:
  workflowId = "wf-123"
  projectKey = $PROJECT_SLUG
```

On `isSuccess: true` → workflow deleted. Invalidate workflow list cache and navigate back to list.
On `isSuccess: false` → workflow not found, or already deleted.

---

## Error Handling

| Error | Cause | Action |
|-------|-------|--------|
| `isSuccess: false` with `workflowId` error | Workflow not found | Verify the ID from `get-workflows` |
| `isSuccess: false` on activate | Invalid workflow configuration (missing trigger or misconfigured nodes) | Edit the workflow to fix configuration before activating |
| `isSuccess: false` on deactivate | Workflow not found or already inactive | Refresh workflow list |
| `401` | `ACCESS_TOKEN` expired | Re-run `get-token` |
| `403` | Missing `cloudadmin` role | Verify role in Cloud Portal → People |
| `404` | Wrong `API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/workflow/pages/workflow-list/workflow-list-page.tsx` | Paginated workflow list with status badges, activate/deactivate toggles, and row actions |
| `modules/workflow/pages/execution-history/execution-history-page.tsx` | Paginated execution history table with status badges and drill-down |
| `modules/workflow/pages/execution-history/execution-detail-page.tsx` | Detailed view of a single execution showing per-node results |
| `modules/workflow/hooks/use-workflow.tsx` | `useGetWorkflows`, `useGetWorkflow`, `useDeleteWorkflow`, `useGetExecutions`, `useGetExecution` hooks |
| `modules/workflow/services/workflow.service.ts` | `getWorkflows()`, `getWorkflow()`, `deleteWorkflow()`, `getExecutions()`, `getExecution()` |
| `modules/workflow/types/workflow.type.ts` | `Workflow`, `Execution`, `NodeResult` interfaces |
