# Flow: create-workflow-flow

## Trigger

User wants to create a new automated workflow or update an existing one with trigger and action nodes.

> "create a workflow that triggers on webhook"
> "build an automation that processes incoming emails"
> "set up a workflow with an AI agent node"
> "create a workflow that sends emails when a webhook fires"
> "add an HTTP request node to my workflow"

---

## Pre-flight Questions

Before starting, confirm:

1. What triggers the workflow — a **webhook** (HTTP request) or an **incoming email**?
2. What action nodes should execute? Options: **AI agent**, **send email**, **HTTP request** — or a combination.
3. How should data flow between nodes? (Which outputs from earlier nodes feed into later nodes?)
4. Should the workflow be activated immediately after creation?
5. If using an AI agent node — which agent ID? (from `ai-services` domain)
6. If using a send email node — which email template ID? (from `communication` domain)

---

## Flow Steps

### Step 1 — Define the Trigger Node

Every workflow starts with exactly one trigger node.

**Option A: Webhook Trigger**

```
Node:
  id       = "node-1"
  name     = "Webhook Trigger"
  type     = "webhook"
  position = { x: 100, y: 200 }
  config   = {}
```

The webhook URL is generated server-side after the workflow is saved and activated.

**Option B: Email Trigger**

```
Node:
  id       = "node-1"
  name     = "Email Trigger"
  type     = "emailTrigger"
  position = { x: 100, y: 200 }
  config   = {
    imapHost:     "imap.gmail.com"
    imapPort:     993
    email:        "inbox@example.com"
    password:     "app-password"
    pollInterval: 60
  }
```

Ask the user for IMAP connection details. For Gmail, recommend using an app-specific password.

---

### Step 2 — Add Action Nodes

Add one or more action nodes after the trigger. Position each node further right on the canvas.

**AI Agent Node**

```
Node:
  id       = "node-2"
  name     = "AI Agent"
  type     = "aiAgent"
  position = { x: 400, y: 200 }
  config   = { agentId: "agent-id-123" }
```

The agent receives input from the previous node's output via expressions.

**Send Email Node**

```
Node:
  id       = "node-3"
  name     = "Send Email"
  type     = "sendEmail"
  position = { x: 700, y: 200 }
  config   = {
    templateId:      "template-id-123"
    to:              "{{$json.output.email}}"
    bodyDataContext: {
      firstName: "{{$json.output.first_name}}"
      summary:   "{{$node[\"AI Agent\"].json.output.summary}}"
    }
  }
```

Use expressions to inject data from previous nodes into the email template.

**HTTP Request Node**

```
Node:
  id       = "node-4"
  name     = "HTTP Request"
  type     = "httpRequest"
  position = { x: 700, y: 400 }
  config   = {
    method:      "POST"
    url:         "https://api.example.com/callback"
    headers:     { "Content-Type": "application/json" }
    body:        "{\"result\": \"{{$json.output.result}}\"}"
    queryParams: {}
  }
```

---

### Step 3 — Connect Nodes with Edges

Define edges to connect the trigger to action nodes and chain action nodes together.

```
Edges:
  - { id: "edge-1", source: "node-1", target: "node-2" }
  - { id: "edge-2", source: "node-2", target: "node-3" }
```

Edges define execution order. The workflow engine executes nodes in topological order based on edges.

---

### Step 4 — Save the Workflow

```
Action: create-workflow
Input:
  name        = "Email Processing Workflow"
  description = "Triggers on webhook, processes with AI agent, sends summary email"
  nodes       = [trigger node, ...action nodes]
  edges       = [edge definitions]
  projectKey  = $PROJECT_SLUG
```

On `isSuccess: true` → workflow created. The response contains the `workflowId`.
On `isSuccess: false` → inspect `errors` and correct the request.

---

### Step 5 — Activate the Workflow

If the user wants the workflow active immediately:

```
Action: activate-workflow
Input:
  workflowId = "wf-123"   (from Step 4 response)
  projectKey = $PROJECT_SLUG
```

On `isSuccess: true` → workflow is now live and will respond to triggers.
On `isSuccess: false` → check that all nodes are properly configured.

---

### Step 6 — Confirm

After creation and optional activation:

> Workflow "{name}" created successfully.
> {If activated}: The workflow is now active and listening for triggers.
> {If webhook}: The webhook URL will be available in the workflow details.
> {If not activated}: Use the workflow management page to activate it when ready.

---

## Expression Guide

When configuring action nodes, use expressions to reference data from previous nodes:

| Pattern | Use when |
|---------|----------|
| `{{$json.output.fieldName}}` | Referencing the immediately preceding node's output |
| `{{$node["Node Name"].json.output.fieldName}}` | Referencing a specific named node's output |

**Example workflow data flow:**
1. Webhook Trigger receives `{ "email": "user@example.com", "message": "Help me" }`
2. AI Agent processes the message → outputs `{ "summary": "User needs help with...", "sentiment": "urgent" }`
3. Send Email uses `{{$node["Webhook Trigger"].json.output.email}}` for the recipient and `{{$node["AI Agent"].json.output.summary}}` for the body

---

## Error Handling

| Error | Cause | Action |
|-------|-------|--------|
| `isSuccess: false` with `nodes` error | Invalid node configuration or missing trigger | Verify each node has required config fields |
| `isSuccess: false` with `edges` error | Invalid edge references (node ID not found) | Verify all edge source/target IDs match node IDs |
| `isSuccess: false` with `name` error | Duplicate workflow name | Choose a unique name |
| `401` | `ACCESS_TOKEN` expired | Re-run `get-token` |
| `403` | Missing `cloudadmin` role | Verify role in Cloud Portal |
| `404` | Wrong `API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/workflow/pages/workflow-editor/workflow-editor-page.tsx` | Visual workflow editor with node canvas, node palette, and configuration panel |
| `modules/workflow/components/workflow-canvas/workflow-canvas.tsx` | React Flow canvas for drag-and-drop node editing |
| `modules/workflow/components/node-editor/node-editor.tsx` | Side panel for configuring selected node properties |
| `modules/workflow/hooks/use-workflow.tsx` | `useCreateWorkflow`, `useUpdateWorkflow`, `useActivateWorkflow`, `useDeactivateWorkflow` hooks |
| `modules/workflow/services/workflow.service.ts` | `createWorkflow()`, `updateWorkflow()`, `activateWorkflow()`, `deactivateWorkflow()` |
| `modules/workflow/types/workflow.type.ts` | `Workflow`, `WorkflowNode`, `WorkflowEdge`, `NodeConfig` interfaces |
