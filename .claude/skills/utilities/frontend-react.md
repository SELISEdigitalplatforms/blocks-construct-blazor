# Utilities Frontend

## Module Location

All utilities UI lives under `src/modules/utilities/`.

```
src/modules/utilities/
├── components/
│   ├── cron-expression-input/
│   │   ├── cron-expression-input.tsx       ← cron builder with preset options
│   │   └── cron-expression-input.test.tsx
│   └── config-setting-row/
│       ├── config-setting-row.tsx          ← single config key/value row with edit
│       └── config-setting-row.test.tsx
├── pages/
│   ├── scheduled-tasks/
│   │   ├── scheduled-tasks-page.tsx        ← list all tasks with status
│   │   └── scheduled-task-editor-page.tsx  ← create/edit task form
│   ├── webhooks/
│   │   ├── webhooks-page.tsx               ← list all webhooks
│   │   └── webhook-editor-page.tsx         ← create webhook form with event selection
│   └── config/
│       └── config-page.tsx                 ← list and edit configuration settings
├── hooks/
│   └── use-utilities.tsx                   ← all React Query hooks for this domain
├── services/
│   └── utilities.service.ts               ← raw API call functions
├── types/
│   └── utilities.type.ts                  ← TypeScript interfaces and enums
└── index.ts                               ← public exports
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
| Icons | Lucide React (`Clock`, `Webhook`, `Settings`, `Play`, `Pause`, `Trash2`, `Plus`, `RefreshCw`) |
| Forms | React Hook Form + Zod |
| State | React Query (TanStack Query) |

---

## Types (`utilities.type.ts`)

```ts
export interface ScheduledTask {
  taskId: string;
  name: string;
  cronExpression: string;
  targetUrl: string;
  httpMethod: string;
  isActive: boolean;
  lastRunAt: string;
  nextRunAt: string;
  lastRunStatus: string;
}

export interface CreateScheduledTaskPayload {
  name: string;
  cronExpression: string;
  targetUrl: string;
  httpMethod: string;
  headers?: Record<string, string>;
  body?: Record<string, unknown>;
  isActive?: boolean;
  projectKey: string;
}

export interface UpdateScheduledTaskPayload extends CreateScheduledTaskPayload {
  taskId: string;
}

export interface Webhook {
  webhookId: string;
  name: string;
  targetUrl: string;
  events: string[];
  isActive: boolean;
  createdDate: string;
}

export interface CreateWebhookPayload {
  name: string;
  targetUrl: string;
  events: string[];
  headers?: Record<string, string>;
  isActive?: boolean;
  projectKey: string;
}

export interface ConfigSetting {
  key: string;
  value: string;
  description: string;
}

export interface UpdateConfigPayload {
  settings: { key: string; value: string }[];
  projectKey: string;
}
```

---

## Service (`utilities.service.ts`)

```ts
import { axiosInstance } from '@/lib/axios';

const BASE = '/utilities/v1';

export const utilitiesService = {
  // ── Scheduled Tasks ──────────────────────────────────────────────────────
  createScheduledTask: (payload: CreateScheduledTaskPayload) =>
    axiosInstance.post(`${BASE}/ScheduledTask/Save`, payload),

  updateScheduledTask: (payload: UpdateScheduledTaskPayload) =>
    axiosInstance.post(`${BASE}/ScheduledTask/Save`, payload),

  getScheduledTasks: (projectKey: string) =>
    axiosInstance.get(`${BASE}/ScheduledTask/Gets`, { params: { projectKey } }),

  deleteScheduledTask: (taskId: string, projectKey: string) =>
    axiosInstance.delete(`${BASE}/ScheduledTask/Delete`, { params: { taskId, projectKey } }),

  // ── Webhooks ─────────────────────────────────────────────────────────────
  createWebhook: (payload: CreateWebhookPayload) =>
    axiosInstance.post(`${BASE}/Webhook/Save`, payload),

  getWebhooks: (projectKey: string) =>
    axiosInstance.get(`${BASE}/Webhook/Gets`, { params: { projectKey } }),

  deleteWebhook: (webhookId: string, projectKey: string) =>
    axiosInstance.delete(`${BASE}/Webhook/Delete`, { params: { webhookId, projectKey } }),

  // ── Config ───────────────────────────────────────────────────────────────
  getConfig: (projectKey: string) =>
    axiosInstance.get(`${BASE}/Config/Gets`, { params: { projectKey } }),

  updateConfig: (payload: UpdateConfigPayload) =>
    axiosInstance.post(`${BASE}/Config/Save`, payload),
};
```

---

## Hooks (`use-utilities.tsx`)

```tsx
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { utilitiesService } from '../services/utilities.service';

// ── Scheduled Tasks ──────────────────────────────────────────────────────────

export const useGetScheduledTasks = (projectKey: string) =>
  useQuery({
    queryKey: ['scheduled-tasks', projectKey],
    queryFn: () => utilitiesService.getScheduledTasks(projectKey),
    select: (res) => res.data,
  });

export const useCreateScheduledTask = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: utilitiesService.createScheduledTask,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['scheduled-tasks'] }),
  });
};

export const useUpdateScheduledTask = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: utilitiesService.updateScheduledTask,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['scheduled-tasks'] }),
  });
};

export const useDeleteScheduledTask = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ taskId, projectKey }: { taskId: string; projectKey: string }) =>
      utilitiesService.deleteScheduledTask(taskId, projectKey),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['scheduled-tasks'] }),
  });
};

// ── Webhooks ─────────────────────────────────────────────────────────────────

export const useGetWebhooks = (projectKey: string) =>
  useQuery({
    queryKey: ['webhooks', projectKey],
    queryFn: () => utilitiesService.getWebhooks(projectKey),
    select: (res) => res.data,
  });

export const useCreateWebhook = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: utilitiesService.createWebhook,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['webhooks'] }),
  });
};

export const useDeleteWebhook = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ webhookId, projectKey }: { webhookId: string; projectKey: string }) =>
      utilitiesService.deleteWebhook(webhookId, projectKey),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['webhooks'] }),
  });
};

// ── Config ───────────────────────────────────────────────────────────────────

export const useGetConfig = (projectKey: string) =>
  useQuery({
    queryKey: ['config', projectKey],
    queryFn: () => utilitiesService.getConfig(projectKey),
    select: (res) => res.data,
  });

export const useUpdateConfig = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: utilitiesService.updateConfig,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['config'] }),
  });
};
```

---

## Page: ScheduledTasks

List all scheduled tasks with status indicators and action buttons.

Key behaviors:
- Show task name, cron expression (with human-readable label), status badge (active/inactive), last run status, and next run time
- Row actions: Edit, Toggle Active/Inactive, Delete
- Delete shows confirmation dialog before calling delete action
- Status badges: green for active, gray for inactive; last run badges: green for success, red for failed, yellow for pending
- Empty state: "No scheduled tasks" with a "Create task" button

---

## Page: ScheduledTaskEditor

Form for creating or editing a scheduled task.

Key behaviors:
- Cron expression input with common preset buttons (every minute, hourly, daily, weekly, monthly)
- Show human-readable schedule description next to the cron input (e.g. "Runs every hour at minute 0")
- HTTP method selector (dropdown: GET, POST, PUT, DELETE)
- Target URL input with URL validation
- Collapsible headers section — key/value pair editor with add/remove rows
- Collapsible body section — JSON editor textarea (for POST/PUT only)
- Active toggle switch
- Show loading state on submit with spinner inside the Save button
- Show success toast on `isSuccess: true`; display field errors from `errors` on failure

---

## Page: Webhooks

List all webhooks with status and event subscriptions.

Key behaviors:
- Show webhook name, target URL, events (as tags/chips), status badge, and created date
- Row actions: Delete (with confirmation dialog)
- Empty state: "No webhooks registered" with a "Create webhook" button

---

## Page: WebhookEditor

Form for creating a new webhook.

Key behaviors:
- Webhook name input
- Target URL input with URL validation
- Events input — multi-select or tag input for event types
- Collapsible headers section — key/value pair editor with add/remove rows
- Active toggle switch
- Show loading state on submit
- Show success toast on `isSuccess: true`; display field errors from `errors` on failure

---

## Page: Config

Display and edit project configuration settings.

Key behaviors:
- List all settings as editable rows with key (read-only), value (editable), and description (helper text)
- Inline editing — click a value to edit, press Enter or click Save to commit
- Batch save — collect all modified values and submit in a single `update-config` call
- Show loading state during save
- Show success toast on `isSuccess: true`; display field errors from `errors` on failure
- Empty state: "No configuration settings found"

---

## Component: CronExpressionInput

Helper component for building cron expressions with preset options.

```tsx
// modules/utilities/components/cron-expression-input/cron-expression-input.tsx

import { Button } from '@/components/ui-kit/button';
import { Input } from '@/components/ui-kit/input';
import { Label } from '@/components/ui-kit/label';

interface CronExpressionInputProps {
  value: string;
  onChange: (value: string) => void;
}

const PRESETS = [
  { label: 'Every minute', value: '* * * * *' },
  { label: 'Every 5 minutes', value: '*/5 * * * *' },
  { label: 'Every hour', value: '0 * * * *' },
  { label: 'Every day at midnight', value: '0 0 * * *' },
  { label: 'Every Monday at 9 AM', value: '0 9 * * 1' },
  { label: 'First of every month', value: '0 0 1 * *' },
];

export function CronExpressionInput({ value, onChange }: CronExpressionInputProps) {
  return (
    <div className="space-y-2">
      <Label>Cron Expression</Label>
      <Input
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder="* * * * *"
        className="font-mono"
      />
      <div className="flex flex-wrap gap-2">
        {PRESETS.map((preset) => (
          <Button
            key={preset.value}
            type="button"
            variant="outline"
            size="sm"
            onClick={() => onChange(preset.value)}
            className={value === preset.value ? 'border-primary' : ''}
          >
            {preset.label}
          </Button>
        ))}
      </div>
    </div>
  );
}
```

---

## Rules

- Never hardcode `projectKey` — always pass from `import.meta.env.PROJECT_SLUG` or app config
- Cron expressions use standard 5-field format: `minute hour day-of-month month day-of-week`
- All pages must handle loading (`<Skeleton />`), error (`<ErrorAlert />`), and empty states
- Use `cn()` from `@/lib/utils` for all conditional classNames
- Use Tailwind semantic tokens — never hardcode colors
- Show confirmation dialogs before all delete operations
- JSON body editor should validate JSON syntax before submission
