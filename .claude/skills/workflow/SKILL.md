---
name: workflow
description: "Use this skill for creating, managing, and executing automated workflows with webhook/email triggers, AI agent nodes, HTTP request nodes, and email sending nodes on SELISE Blocks."
user-invocable: false
blocks-version: "1.0.5"
---

# Workflow Skill

## Purpose

Handles workflow automation on SELISE Blocks via the Workflow v1 API. Covers creating workflows with trigger nodes (webhook, email), action nodes (AI agent, send email, HTTP request), connecting nodes with edges, expression system for passing data between nodes, activation/deactivation, and execution history.

> **Developer Preview:** The Workflow service is currently in Developer Preview. APIs may change in future releases.

---

## When to Use

Example prompts that should route here:
- "Create an automated workflow that triggers on webhook"
- "Build a workflow with an AI agent node"
- "Set up an email-triggered automation"
- "List all my workflows"
- "Show execution history for a workflow"
- "Activate/deactivate a workflow"
- "Build a workflow editor page"

---

## Execution Context

Before executing any action or flow from this skill, read `../core/execution-context.md` for the required supporting files, load order, and cross-domain orchestration rules.

---

## Intent Mapping

Use this table to route user requests. Check `flows/` first — if a flow covers the request, use it. For single-action requests, go directly to the action.

| User wants to... | Use |
|------------------|-----|
| Create a new workflow | `flows/create-workflow-flow.md` → `actions/create-workflow.md` |
| List all workflows | `flows/manage-workflows-flow.md` → `actions/get-workflows.md` |
| View a specific workflow | `flows/manage-workflows-flow.md` → `actions/get-workflow.md` |
| Update an existing workflow | `flows/create-workflow-flow.md` → `actions/update-workflow.md` |
| Activate a workflow | `flows/manage-workflows-flow.md` → `actions/activate-workflow.md` |
| Deactivate a workflow | `flows/manage-workflows-flow.md` → `actions/deactivate-workflow.md` |
| Delete a workflow | `flows/manage-workflows-flow.md` → `actions/delete-workflow.md` |
| View execution history | `flows/manage-workflows-flow.md` → `actions/get-executions.md` |
| View execution detail | `flows/manage-workflows-flow.md` → `actions/get-execution.md` |

---

## Flows

| Flow | File | Description |
|------|------|-------------|
| create-workflow-flow | flows/create-workflow-flow.md | Define trigger, add action nodes, connect edges, save and activate a workflow |
| manage-workflows-flow | flows/manage-workflows-flow.md | List workflows, view details, activate/deactivate, view execution history, delete |

---

## Base Path

All endpoints are prefixed with: `$API_BASE_URL/workflow/v1`

---

## Action Index

### Workflow

| Action | File | Description |
|--------|------|-------------|
| create-workflow | actions/create-workflow.md | Create a new workflow with nodes and edges |
| get-workflows | actions/get-workflows.md | Get paginated list of workflows |
| get-workflow | actions/get-workflow.md | Get a specific workflow by ID |
| update-workflow | actions/update-workflow.md | Update an existing workflow (nodes, edges, name, description) |
| delete-workflow | actions/delete-workflow.md | Delete a workflow by ID |
| activate-workflow | actions/activate-workflow.md | Activate a workflow so it responds to triggers |
| deactivate-workflow | actions/deactivate-workflow.md | Deactivate a workflow to stop it from responding to triggers |

### Execution

| Action | File | Description |
|--------|------|-------------|
| get-executions | actions/get-executions.md | Get paginated execution history for a workflow |
| get-execution | actions/get-execution.md | Get details of a specific execution |
