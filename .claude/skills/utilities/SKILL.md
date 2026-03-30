---
name: utilities
description: "Use this skill for managing scheduled tasks, webhooks, configuration settings, and other utility operations on SELISE Blocks."
user-invocable: false
blocks-version: "1.0.5"
---

# Utilities Skill

## Purpose

Handles utility operations — scheduled tasks (cron jobs), webhook management, and project configuration settings for SELISE Blocks via the Utilities v1 API.

Covers three sub-domains: ScheduledTask (create, update, delete, and list cron-based tasks), Webhook (register and manage event-driven webhooks), and Config (read and update project configuration settings).

---

## When to Use

Example prompts that should route here:
- "Create a scheduled task to call my endpoint every hour"
- "Set up a webhook for order events"
- "Manage project config settings"
- "List all scheduled jobs"
- "Delete a webhook"
- "Update a cron schedule"

---

## Execution Context

Before executing any action or flow from this skill, read `../core/execution-context.md` for the required supporting files, load order, and cross-domain orchestration rules.

---

## Intent Mapping

Use this table to route user requests. Check `flows/` first — if a flow covers the request, use it. For single-action requests, go directly to the action.

| User wants to... | Use |
|------------------|-----|
| Create a scheduled task | `flows/setup-scheduled-task-flow.md` → `actions/create-scheduled-task.md` |
| List scheduled tasks | `actions/get-scheduled-tasks.md` |
| Update a scheduled task | `actions/update-scheduled-task.md` |
| Delete a scheduled task | `actions/delete-scheduled-task.md` |
| Create a webhook | `flows/manage-webhooks-flow.md` → `actions/create-webhook.md` |
| List webhooks | `actions/get-webhooks.md` |
| Delete a webhook | `actions/delete-webhook.md` |
| View project config | `actions/get-config.md` |
| Update project config | `actions/update-config.md` |

---

## Flows

| Flow | File | Description |
|------|------|-------------|
| setup-scheduled-task-flow | flows/setup-scheduled-task-flow.md | Define and activate a cron-based scheduled task |
| manage-webhooks-flow | flows/manage-webhooks-flow.md | Register, configure, and test event-driven webhooks |

---

## Base Path

All endpoints are prefixed with: `$API_BASE_URL/utilities/v1`

---

## Action Index

### ScheduledTask

| Action | File | Description |
|--------|------|-------------|
| create-scheduled-task | actions/create-scheduled-task.md | Create a new scheduled task with cron expression |
| get-scheduled-tasks | actions/get-scheduled-tasks.md | List all scheduled tasks for the project |
| update-scheduled-task | actions/update-scheduled-task.md | Update an existing scheduled task (with taskId) |
| delete-scheduled-task | actions/delete-scheduled-task.md | Delete a scheduled task by ID |

### Webhook

| Action | File | Description |
|--------|------|-------------|
| create-webhook | actions/create-webhook.md | Register a new webhook for specific events |
| get-webhooks | actions/get-webhooks.md | List all webhooks for the project |
| delete-webhook | actions/delete-webhook.md | Delete a webhook by ID |

### Config

| Action | File | Description |
|--------|------|-------------|
| get-config | actions/get-config.md | Get all configuration settings for the project |
| update-config | actions/update-config.md | Update one or more configuration settings |
