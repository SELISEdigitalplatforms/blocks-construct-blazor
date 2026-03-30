# Deployment Contracts

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

## Build

### TriggerBuildRequest

Used to trigger a new build for a specific repository and branch.

```json
{
  "repositoryId": "string",
  "branch": "string",
  "projectKey": "string"
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| repositoryId | string | yes | ID of the repository attached in Cloud Portal |
| branch | string | yes | Git branch to build (e.g. `"main"`, `"develop"`) |
| projectKey | string | yes | Project identifier from `$PROJECT_SLUG` |

---

### GetBuildsResponse

Returned by `GET /Build/Gets`.

```json
{
  "builds": [
    {
      "buildId": "string",
      "repositoryId": "string",
      "branch": "string",
      "status": "string",
      "startTime": "2024-01-01T00:00:00Z",
      "endTime": "2024-01-01T00:05:00Z",
      "commitHash": "string",
      "commitMessage": "string"
    }
  ],
  "totalCount": 100,
  "isSuccess": true,
  "errors": {}
}
```

**Build status values:** `Queued`, `InProgress`, `Succeeded`, `Failed`, `Cancelled`

**Query parameters for `GetBuilds`:**

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| projectKey | string | yes | Project identifier |
| page | integer | yes | 1-based page number |
| pageSize | integer | yes | Records per page |

---

### GetBuildResponse

Returned by `GET /Build/Get`.

```json
{
  "build": {
    "buildId": "string",
    "repositoryId": "string",
    "branch": "string",
    "status": "string",
    "startTime": "2024-01-01T00:00:00Z",
    "endTime": "2024-01-01T00:05:00Z",
    "commitHash": "string",
    "commitMessage": "string"
  },
  "isSuccess": true,
  "errors": {}
}
```

**Query parameters for `GetBuild`:**

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| buildId | string | yes | ID of the build to retrieve |
| projectKey | string | yes | Project identifier |

---

## Service

### GetServicesResponse

Returned by `GET /Service/Gets`.

```json
{
  "services": [
    {
      "serviceId": "string",
      "name": "string",
      "status": "string",
      "replicas": 1,
      "lastDeployedAt": "2024-01-01T00:00:00Z",
      "repository": "string",
      "branch": "string"
    }
  ],
  "isSuccess": true,
  "errors": {}
}
```

**Service status values:** `Running`, `Stopped`, `Deploying`, `Failed`

**Query parameters for `GetServices`:**

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| projectKey | string | yes | Project identifier |

---

### GetServiceResponse

Returned by `GET /Service/Get`.

```json
{
  "service": {
    "serviceId": "string",
    "name": "string",
    "status": "string",
    "replicas": 1,
    "lastDeployedAt": "2024-01-01T00:00:00Z",
    "repository": "string",
    "branch": "string"
  },
  "isSuccess": true,
  "errors": {}
}
```

**Query parameters for `GetService`:**

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| serviceId | string | yes | ID of the service to retrieve |
| projectKey | string | yes | Project identifier |

---

### ServiceConfigRequest

Used to update service configuration such as replica count and environment variables.

```json
{
  "serviceId": "string",
  "replicas": 2,
  "envVars": {
    "NODE_ENV": "production",
    "LOG_LEVEL": "info"
  },
  "projectKey": "string"
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| serviceId | string | yes | ID of the service to configure |
| replicas | integer | no | Number of service replicas (1-10) |
| envVars | object | no | Key/value pairs for environment variables |
| projectKey | string | yes | Project identifier from `$PROJECT_SLUG` |

---

## Deployment

### DeploymentHistoryResponse

Returned by `GET /Deployment/Gets`.

```json
{
  "deployments": [
    {
      "deploymentId": "string",
      "serviceId": "string",
      "buildId": "string",
      "status": "string",
      "deployedAt": "2024-01-01T00:00:00Z",
      "deployedBy": "string",
      "version": "string"
    }
  ],
  "totalCount": 50,
  "isSuccess": true,
  "errors": {}
}
```

**Deployment status values:** `Pending`, `InProgress`, `Succeeded`, `Failed`, `RolledBack`

**Query parameters for `GetDeployments`:**

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| serviceId | string | yes | ID of the service to get history for |
| projectKey | string | yes | Project identifier |
| page | integer | yes | 1-based page number |
| pageSize | integer | yes | Records per page |

---

### RollbackRequest

Used to rollback a service to a previous deployment.

```json
{
  "serviceId": "string",
  "deploymentId": "string",
  "projectKey": "string"
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| serviceId | string | yes | ID of the service to rollback |
| deploymentId | string | yes | ID of the target deployment to rollback to |
| projectKey | string | yes | Project identifier from `$PROJECT_SLUG` |
