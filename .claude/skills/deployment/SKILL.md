---
name: deployment
description: "Use this skill for deploying code, managing builds, monitoring services, and configuring runtime environments on SELISE Blocks."
user-invocable: false
blocks-version: "1.0.5"
---

# Deployment Skill

## Purpose

Handles CI/CD deployment, build management, service configuration, runtime scaling, and deployment status monitoring for SELISE Blocks via the Deployment v1 API.

Covers three sub-domains: Build (trigger and monitor builds), Service (list and configure services), and Deployment (history and rollback).

---

## When to Use

Example prompts that should route here:
- "Deploy my code"
- "Check build status"
- "List my services"
- "Scale my service"
- "View deployment history"
- "Rollback to the previous deployment"
- "Trigger a build on the main branch"
- "Show me the status of my last build"

---

## Execution Context

Before executing any action or flow from this skill, read `../core/execution-context.md` for the required supporting files, load order, and cross-domain orchestration rules.

---

## Intent Mapping

Use this table to route user requests. Check `flows/` first — if a flow covers the request, use it. For single-action requests, go directly to the action.

| User wants to... | Use |
|------------------|-----|
| Deploy code end-to-end | `flows/deploy-code-flow.md` |
| Trigger a build | `flows/deploy-code-flow.md` → `actions/trigger-build.md` |
| Check build status | `actions/get-build.md` |
| List all builds | `actions/get-builds.md` |
| List services | `flows/manage-services-flow.md` → `actions/get-services.md` |
| View service details | `flows/manage-services-flow.md` → `actions/get-service.md` |
| Update service configuration | `flows/manage-services-flow.md` → `actions/update-service-config.md` |
| Scale a service | `flows/manage-services-flow.md` → `actions/update-service-config.md` |
| View deployment history | `flows/manage-services-flow.md` → `actions/get-deployment-history.md` |
| Rollback a deployment | `flows/manage-services-flow.md` → `actions/rollback-deployment.md` |

---

## Flows

| Flow | File | Description |
|------|------|-------------|
| deploy-code-flow | flows/deploy-code-flow.md | Select repo/branch, trigger build, monitor, deploy, and verify |
| manage-services-flow | flows/manage-services-flow.md | List services, view details, update config, view history, rollback |

---

## Base Path

All endpoints are prefixed with: `$API_BASE_URL/deployment/v1`

---

## Action Index

### Build

| Action | File | Description |
|--------|------|-------------|
| trigger-build | actions/trigger-build.md | Trigger a new build for a repository and branch |
| get-builds | actions/get-builds.md | Get paginated list of builds for the project |
| get-build | actions/get-build.md | Get a single build by ID |

### Service

| Action | File | Description |
|--------|------|-------------|
| get-services | actions/get-services.md | List all services in the project |
| get-service | actions/get-service.md | Get details of a single service |
| update-service-config | actions/update-service-config.md | Update service configuration (replicas, env vars) |

### Deployment

| Action | File | Description |
|--------|------|-------------|
| get-deployment-history | actions/get-deployment-history.md | Get paginated deployment history for a service |
| rollback-deployment | actions/rollback-deployment.md | Rollback a service to a previous deployment |
