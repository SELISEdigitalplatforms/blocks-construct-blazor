# Deployment Frontend

## Module Location

All deployment UI lives under `src/modules/deployment/`.

```
src/modules/deployment/
├── components/
│   ├── build-status/
│   │   ├── build-status.tsx              ← status badge with color coding
│   │   └── build-status.test.tsx
│   ├── service-config-form/
│   │   ├── service-config-form.tsx       ← form for replicas and env vars
│   │   └── service-config-form.test.tsx
│   ├── rollback-dialog/
│   │   ├── rollback-dialog.tsx           ← confirmation dialog for rollback
│   │   └── rollback-dialog.test.tsx
│   └── env-var-editor/
│       ├── env-var-editor.tsx            ← key/value pair editor for env vars
│       └── env-var-editor.test.tsx
├── pages/
│   ├── build-list/
│   │   └── build-list-page.tsx           ← paginated build list with trigger button
│   ├── service-list/
│   │   └── service-list-page.tsx         ← service overview with status indicators
│   ├── service-detail/
│   │   └── service-detail-page.tsx       ← service config editor and deployment info
│   └── deployment-history/
│       └── deployment-history-page.tsx   ← paginated deployment history with rollback
├── hooks/
│   └── use-deployment.tsx                ← all React Query hooks for this domain
├── services/
│   └── deployment.service.ts             ← raw API call functions
├── types/
│   └── deployment.type.ts               ← TypeScript interfaces and enums
└── index.ts                              ← public exports
```

---

## Stack

Follows `skills/core/frontend-react.md` conventions:

| Layer | Technology |
|-------|-----------|
| Framework | React 19 + TypeScript |
| Build tool | Vite |
| Styling | Tailwind CSS 3.4 |
| Component primitives | Radix UI |
| Component system | shadcn/ui style |
| Icons | Lucide React (`Rocket`, `Server`, `GitBranch`, `RotateCcw`, `Play`, `Clock`, `CheckCircle2`, `XCircle`, `Loader2`) |
| Forms | React Hook Form + Zod |
| State | React Query (TanStack Query) |

---

## Types (`deployment.type.ts`)

```ts
export type BuildStatus = 'Queued' | 'InProgress' | 'Succeeded' | 'Failed' | 'Cancelled';
export type ServiceStatus = 'Running' | 'Stopped' | 'Deploying' | 'Failed';
export type DeploymentStatus = 'Pending' | 'InProgress' | 'Succeeded' | 'Failed' | 'RolledBack';

export interface Build {
  buildId: string;
  repositoryId: string;
  branch: string;
  status: BuildStatus;
  startTime: string;
  endTime: string;
  commitHash: string;
  commitMessage: string;
}

export interface Service {
  serviceId: string;
  name: string;
  status: ServiceStatus;
  replicas: number;
  lastDeployedAt: string;
  repository: string;
  branch: string;
}

export interface Deployment {
  deploymentId: string;
  serviceId: string;
  buildId: string;
  status: DeploymentStatus;
  deployedAt: string;
  deployedBy: string;
  version: string;
}

export interface TriggerBuildPayload {
  repositoryId: string;
  branch: string;
  projectKey: string;
}

export interface ServiceConfig {
  serviceId: string;
  replicas?: number;
  envVars?: Record<string, string>;
  projectKey: string;
}

export interface RollbackPayload {
  serviceId: string;
  deploymentId: string;
  projectKey: string;
}

export interface GetBuildsParams {
  projectKey: string;
  page: number;
  pageSize: number;
}

export interface GetDeploymentHistoryParams {
  serviceId: string;
  projectKey: string;
  page: number;
  pageSize: number;
}
```

---

## Service (`deployment.service.ts`)

```ts
import { axiosInstance } from '@/lib/axios';

const BASE = '/deployment/v1';

export const deploymentService = {
  triggerBuild: (payload: TriggerBuildPayload) =>
    axiosInstance.post(`${BASE}/Build/Trigger`, payload),

  getBuilds: (params: GetBuildsParams) =>
    axiosInstance.get(`${BASE}/Build/Gets`, { params }),

  getBuild: (buildId: string, projectKey: string) =>
    axiosInstance.get(`${BASE}/Build/Get`, { params: { buildId, projectKey } }),

  getServices: (projectKey: string) =>
    axiosInstance.get(`${BASE}/Service/Gets`, { params: { projectKey } }),

  getService: (serviceId: string, projectKey: string) =>
    axiosInstance.get(`${BASE}/Service/Get`, { params: { serviceId, projectKey } }),

  updateServiceConfig: (payload: ServiceConfig) =>
    axiosInstance.post(`${BASE}/Service/UpdateConfig`, payload),

  getDeploymentHistory: (params: GetDeploymentHistoryParams) =>
    axiosInstance.get(`${BASE}/Deployment/Gets`, { params }),

  rollbackDeployment: (payload: RollbackPayload) =>
    axiosInstance.post(`${BASE}/Deployment/Rollback`, payload),
};
```

---

## Hooks (`use-deployment.tsx`)

```tsx
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { deploymentService } from '../services/deployment.service';

// ── Builds ──────────────────────────────────────────────────────────────────

export const useGetBuilds = (params: GetBuildsParams) =>
  useQuery({
    queryKey: ['builds', params],
    queryFn: () => deploymentService.getBuilds(params),
    select: (res) => res.data,
  });

export const useGetBuild = (buildId: string, projectKey: string) =>
  useQuery({
    queryKey: ['builds', buildId],
    queryFn: () => deploymentService.getBuild(buildId, projectKey),
    enabled: !!buildId,
    select: (res) => res.data,
    refetchInterval: (query) => {
      const status = query.state.data?.data?.build?.status;
      // Poll while build is in progress
      return status === 'Queued' || status === 'InProgress' ? 10_000 : false;
    },
  });

export const useTriggerBuild = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: deploymentService.triggerBuild,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['builds'] }),
  });
};

// ── Services ────────────────────────────────────────────────────────────────

export const useGetServices = (projectKey: string) =>
  useQuery({
    queryKey: ['services', projectKey],
    queryFn: () => deploymentService.getServices(projectKey),
    select: (res) => res.data,
  });

export const useGetService = (serviceId: string, projectKey: string) =>
  useQuery({
    queryKey: ['services', serviceId],
    queryFn: () => deploymentService.getService(serviceId, projectKey),
    enabled: !!serviceId,
    select: (res) => res.data,
  });

export const useUpdateServiceConfig = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: deploymentService.updateServiceConfig,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['services'] }),
  });
};

// ── Deployments ─────────────────────────────────────────────────────────────

export const useGetDeploymentHistory = (params: GetDeploymentHistoryParams) =>
  useQuery({
    queryKey: ['deployments', params],
    queryFn: () => deploymentService.getDeploymentHistory(params),
    select: (res) => res.data,
  });

export const useRollbackDeployment = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: deploymentService.rollbackDeployment,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['deployments'] });
      queryClient.invalidateQueries({ queryKey: ['services'] });
    },
  });
};
```

---

## Component: BuildStatus

Status badge with color-coded indicators for build status.

```tsx
// modules/deployment/components/build-status/build-status.tsx

import { Badge } from '@/components/ui-kit/badge';
import { CheckCircle2, XCircle, Loader2, Clock, Ban } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { BuildStatus as BuildStatusType } from '../../types/deployment.type';

const statusConfig: Record<BuildStatusType, { icon: React.ElementType; variant: string; label: string }> = {
  Queued: { icon: Clock, variant: 'secondary', label: 'Queued' },
  InProgress: { icon: Loader2, variant: 'default', label: 'In Progress' },
  Succeeded: { icon: CheckCircle2, variant: 'default', label: 'Succeeded' },
  Failed: { icon: XCircle, variant: 'destructive', label: 'Failed' },
  Cancelled: { icon: Ban, variant: 'secondary', label: 'Cancelled' },
};

interface BuildStatusProps {
  status: BuildStatusType;
}

export function BuildStatus({ status }: BuildStatusProps) {
  const config = statusConfig[status];
  const Icon = config.icon;

  return (
    <Badge variant={config.variant as any} className="gap-1">
      <Icon className={cn('h-3 w-3', status === 'InProgress' && 'animate-spin')} />
      {config.label}
    </Badge>
  );
}
```

---

## Page: BuildListPage

Paginated build list with a trigger button and status badges.

Key behaviors:
- Fetch builds on mount and on page change
- "Trigger Build" button opens a form to select repository and branch
- Show `<Skeleton />` rows while loading
- Each row shows branch, commit hash (truncated), commit message, status badge, and timestamps
- Click a row to view full build details
- Empty state: "No builds found" with a trigger button

---

## Page: ServiceListPage

Overview of all services with status indicators and quick actions.

Key behaviors:
- Fetch services on mount
- Show status indicator (colored dot), replica count, last deployed time
- Click a service row to navigate to `service-detail-page`
- Empty state: "No services deployed yet"

---

## Page: ServiceDetailPage

Service configuration editor with deployment info.

Key behaviors:
- Display service name, status, repository, branch
- `ServiceConfigForm` component for editing replicas and environment variables
- `EnvVarEditor` for adding/removing/editing key-value environment variable pairs
- Show recent deployment history inline (last 5 deployments)
- Rollback button on each historical deployment row
- Show loading state on save with spinner inside the button
- Show success toast on `isSuccess: true`; display field errors on failure

---

## Page: DeploymentHistoryPage

Paginated deployment history table with rollback capability.

Key behaviors:
- Fetch deployment history for a specific service
- Show deployment ID (truncated), build ID, status badge, deployed time, deployed by, version
- Rollback button on `Succeeded` deployments — opens `RollbackDialog` for confirmation
- Show `<Skeleton />` rows while loading
- Empty state: "No deployments found"

---

## Build Status Polling Pattern

When monitoring a build, use `refetchInterval` to poll automatically:

```
Poll interval: 10 seconds (while status is Queued or InProgress)
Stop polling:  When status reaches Succeeded, Failed, or Cancelled
On Succeeded:  Show success toast, optionally navigate to deployment history
On Failed:     Show error toast with build failure details
```

React Query handles automatic polling via `refetchInterval` returning `false` to stop.

---

## Rules

- Never hardcode `projectKey` — always pass from `import.meta.env.PROJECT_SLUG` or app config
- Build status polling must stop when a terminal state is reached — do not poll indefinitely
- Rollback actions must always show a confirmation dialog before executing
- Environment variable values may contain secrets — mask them by default, reveal on click
- All pages must handle loading (`<Skeleton />`), error (`<ErrorAlert />`), and empty states
- Use `cn()` from `@/lib/utils` for all conditional classNames
- Use Tailwind semantic tokens — never hardcode colors
