# Workflow — Frontend Guide (React)

This file extends `core/frontend-react.md` with workflow-specific patterns for the workflow skill.
Always read `core/frontend-react.md` first, then apply the overrides and additions here.

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
| Icons | Lucide React (`Workflow`, `Play`, `Pause`, `Trash2`, `Plus`, `Settings`, `History`, `CheckCircle`, `XCircle`, `Clock`, `Loader2`) |
| Forms | React Hook Form + Zod |
| State | React Query (TanStack Query) |
| Node editor | React Flow (`@xyflow/react`) |

---

## Module Location

All workflow UI lives under `src/modules/workflow/`.

```
src/modules/workflow/
├── components/
│   ├── workflow-canvas/
│   │   ├── workflow-canvas.tsx          ← React Flow canvas for visual node editing
│   │   ├── workflow-canvas.test.tsx
│   │   ├── custom-nodes/
│   │   │   ├── trigger-node.tsx         ← webhook/email trigger node component
│   │   │   ├── action-node.tsx          ← AI agent/send email/HTTP request node component
│   │   │   └── node-types.ts            ← React Flow nodeTypes registry
│   │   └── node-palette.tsx             ← sidebar with draggable node types
│   └── node-editor/
│       ├── node-editor.tsx              ← side panel for configuring selected node
│       ├── webhook-config.tsx           ← webhook trigger config form
│       ├── email-trigger-config.tsx     ← email trigger config form
│       ├── ai-agent-config.tsx          ← AI agent config form
│       ├── send-email-config.tsx        ← send email config form
│       └── http-request-config.tsx      ← HTTP request config form
├── pages/
│   ├── workflow-list/
│   │   └── workflow-list-page.tsx       ← paginated workflow list with status and actions
│   ├── workflow-editor/
│   │   └── workflow-editor-page.tsx     ← visual workflow builder page
│   └── execution-history/
│       ├── execution-history-page.tsx   ← paginated execution list for a workflow
│       └── execution-detail-page.tsx    ← per-node execution results view
├── hooks/
│   └── use-workflow.tsx                 ← all React Query hooks for this domain
├── services/
│   └── workflow.service.ts              ← raw API call functions
├── types/
│   └── workflow.type.ts                 ← TypeScript interfaces and enums
└── index.ts                             ← public exports
```

---

## Types (`workflow.type.ts`)

```ts
export interface WorkflowNode {
  id: string;
  name: string;
  type: 'webhook' | 'emailTrigger' | 'aiAgent' | 'sendEmail' | 'httpRequest';
  position: { x: number; y: number };
  config: WebhookConfig | EmailTriggerConfig | AIAgentConfig | SendEmailConfig | HTTPRequestConfig;
}

export interface WebhookConfig {}

export interface EmailTriggerConfig {
  imapHost: string;
  imapPort: number;
  email: string;
  password: string;
  pollInterval?: number;
}

export interface AIAgentConfig {
  agentId: string;
}

export interface SendEmailConfig {
  templateId: string;
  to: string;
  bodyDataContext?: Record<string, string>;
}

export interface HTTPRequestConfig {
  method: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';
  url: string;
  headers?: Record<string, string>;
  body?: string;
  queryParams?: Record<string, string>;
}

export interface WorkflowEdge {
  id: string;
  source: string;
  target: string;
}

export interface Workflow {
  workflowId: string;
  name: string;
  description: string;
  nodes: WorkflowNode[];
  edges: WorkflowEdge[];
  isActive: boolean;
  createdDate: string;
  lastUpdatedDate: string;
}

export interface WorkflowSummary {
  workflowId: string;
  name: string;
  description: string;
  isActive: boolean;
  createdDate: string;
  lastUpdatedDate: string;
}

export interface CreateWorkflowPayload {
  name: string;
  description?: string;
  nodes: WorkflowNode[];
  edges: WorkflowEdge[];
  projectKey: string;
}

export interface UpdateWorkflowPayload extends CreateWorkflowPayload {
  workflowId: string;
}

export interface ActivateDeactivatePayload {
  workflowId: string;
  projectKey: string;
}

export interface NodeResult {
  nodeId: string;
  nodeName: string;
  status: ExecutionStatus;
  output: Record<string, unknown>;
  startTime: string;
  endTime: string;
}

export type ExecutionStatus = 'pending' | 'running' | 'completed' | 'failed' | 'cancelled';

export interface Execution {
  executionId: string;
  workflowId: string;
  status: ExecutionStatus;
  startTime: string;
  endTime: string;
  nodeResults: NodeResult[];
}

export interface GetWorkflowsParams {
  projectKey: string;
  page: number;
  pageSize: number;
}

export interface GetExecutionsParams {
  workflowId: string;
  projectKey: string;
  page: number;
  pageSize: number;
}
```

---

## Service (`workflow.service.ts`)

```ts
import { axiosInstance } from '@/lib/axios';

const BASE = '/workflow/v1';

export const workflowService = {
  createWorkflow: (payload: CreateWorkflowPayload) =>
    axiosInstance.post(`${BASE}/Workflow/Save`, payload),

  updateWorkflow: (payload: UpdateWorkflowPayload) =>
    axiosInstance.post(`${BASE}/Workflow/Save`, payload),

  getWorkflows: (params: GetWorkflowsParams) =>
    axiosInstance.get(`${BASE}/Workflow/Gets`, { params }),

  getWorkflow: (workflowId: string, projectKey: string) =>
    axiosInstance.get(`${BASE}/Workflow/Get`, { params: { workflowId, projectKey } }),

  deleteWorkflow: (workflowId: string, projectKey: string) =>
    axiosInstance.delete(`${BASE}/Workflow/Delete`, { params: { workflowId, projectKey } }),

  activateWorkflow: (payload: ActivateDeactivatePayload) =>
    axiosInstance.post(`${BASE}/Workflow/Activate`, payload),

  deactivateWorkflow: (payload: ActivateDeactivatePayload) =>
    axiosInstance.post(`${BASE}/Workflow/Deactivate`, payload),

  getExecutions: (params: GetExecutionsParams) =>
    axiosInstance.get(`${BASE}/Execution/Gets`, { params }),

  getExecution: (executionId: string, projectKey: string) =>
    axiosInstance.get(`${BASE}/Execution/Get`, { params: { executionId, projectKey } }),
};
```

---

## Hooks (`use-workflow.tsx`)

```tsx
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { workflowService } from '../services/workflow.service';

// ── Workflows ────────────────────────────────────────────────────────────────

export const useGetWorkflows = (params: GetWorkflowsParams) =>
  useQuery({
    queryKey: ['workflows', params],
    queryFn: () => workflowService.getWorkflows(params),
    select: (res) => res.data,
  });

export const useGetWorkflow = (workflowId: string, projectKey: string) =>
  useQuery({
    queryKey: ['workflow', workflowId],
    queryFn: () => workflowService.getWorkflow(workflowId, projectKey),
    enabled: !!workflowId,
    select: (res) => res.data,
  });

export const useCreateWorkflow = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: workflowService.createWorkflow,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['workflows'] }),
  });
};

export const useUpdateWorkflow = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: workflowService.updateWorkflow,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workflows'] });
      queryClient.invalidateQueries({ queryKey: ['workflow'] });
    },
  });
};

export const useDeleteWorkflow = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ workflowId, projectKey }: { workflowId: string; projectKey: string }) =>
      workflowService.deleteWorkflow(workflowId, projectKey),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['workflows'] }),
  });
};

export const useActivateWorkflow = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: workflowService.activateWorkflow,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workflows'] });
      queryClient.invalidateQueries({ queryKey: ['workflow'] });
    },
  });
};

export const useDeactivateWorkflow = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: workflowService.deactivateWorkflow,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workflows'] });
      queryClient.invalidateQueries({ queryKey: ['workflow'] });
    },
  });
};

// ── Executions ───────────────────────────────────────────────────────────────

export const useGetExecutions = (params: GetExecutionsParams) =>
  useQuery({
    queryKey: ['executions', params],
    queryFn: () => workflowService.getExecutions(params),
    select: (res) => res.data,
  });

export const useGetExecution = (executionId: string, projectKey: string) =>
  useQuery({
    queryKey: ['execution', executionId],
    queryFn: () => workflowService.getExecution(executionId, projectKey),
    enabled: !!executionId,
    select: (res) => res.data,
  });
```

---

## Component: WorkflowCanvas

The visual workflow editor built on React Flow. Renders nodes as custom components and supports drag-and-drop from the node palette.

```tsx
// modules/workflow/components/workflow-canvas/workflow-canvas.tsx

import { useCallback } from 'react';
import {
  ReactFlow,
  Background,
  Controls,
  MiniMap,
  useNodesState,
  useEdgesState,
  addEdge,
  type Connection,
  type Node,
  type Edge,
} from '@xyflow/react';
import '@xyflow/react/dist/style.css';
import { nodeTypes } from './custom-nodes/node-types';

interface WorkflowCanvasProps {
  initialNodes: Node[];
  initialEdges: Edge[];
  onNodesChange: (nodes: Node[]) => void;
  onEdgesChange: (edges: Edge[]) => void;
  onNodeSelect: (nodeId: string | null) => void;
}

export function WorkflowCanvas({
  initialNodes,
  initialEdges,
  onNodesChange,
  onEdgesChange,
  onNodeSelect,
}: WorkflowCanvasProps) {
  const [nodes, setNodes, handleNodesChange] = useNodesState(initialNodes);
  const [edges, setEdges, handleEdgesChange] = useEdgesState(initialEdges);

  const onConnect = useCallback(
    (connection: Connection) => {
      setEdges((eds) => addEdge(connection, eds));
    },
    [setEdges]
  );

  const onNodeClick = useCallback(
    (_: React.MouseEvent, node: Node) => {
      onNodeSelect(node.id);
    },
    [onNodeSelect]
  );

  const onPaneClick = useCallback(() => {
    onNodeSelect(null);
  }, [onNodeSelect]);

  return (
    <div className="h-full w-full">
      <ReactFlow
        nodes={nodes}
        edges={edges}
        onNodesChange={handleNodesChange}
        onEdgesChange={handleEdgesChange}
        onConnect={onConnect}
        onNodeClick={onNodeClick}
        onPaneClick={onPaneClick}
        nodeTypes={nodeTypes}
        fitView
      >
        <Background />
        <Controls />
        <MiniMap />
      </ReactFlow>
    </div>
  );
}
```

---

## Page: WorkflowListPage

Paginated list of all workflows with status badges, activate/deactivate toggles, and row actions.

Key behaviors:
- Fetch on mount and on page change
- Show `<Skeleton />` rows while loading
- Status badge: green "Active" or gray "Inactive"
- Row actions: Edit (navigate to editor), History (navigate to execution history), Delete (confirmation dialog)
- Toggle switch for activate/deactivate inline
- Empty state: "No workflows yet" with a "Create Workflow" button

---

## Page: WorkflowEditorPage

Visual workflow builder with three panels: node palette (left), canvas (center), node config (right).

Key behaviors:
- Drag nodes from the palette onto the canvas
- Click a node to open its configuration panel on the right
- Connect nodes by dragging from output handle to input handle
- Save button calls `createWorkflow` or `updateWorkflow` based on whether `workflowId` exists
- Toolbar with: Save, Activate/Deactivate toggle, Back to list
- Load existing workflow data when editing (via URL param `workflowId`)
- Show validation errors if trigger node is missing before save

---

## Page: ExecutionHistoryPage

Paginated table of executions for a specific workflow.

Key behaviors:
- Fetch on mount with `workflowId` from URL params
- Columns: Execution ID (truncated), Status (color-coded badge), Start Time, End Time, Duration
- Status colors: `completed` = green, `failed` = red, `running` = blue, `pending` = gray, `cancelled` = yellow
- Click a row to navigate to `execution-detail-page`
- Show `<Skeleton />` rows while loading
- Empty state: "No executions yet"

---

## Page: ExecutionDetailPage

Detailed view of a single execution showing per-node results.

Key behaviors:
- Fetch execution detail on mount with `executionId` from URL params
- Display overall execution status and duration at the top
- List each node result as a card showing: node name, status badge, duration, collapsible output JSON
- For failed nodes: highlight in red, show error message prominently
- Use `<pre>` or a JSON viewer component for output data
- Back button to return to execution history

---

## Node Palette

Sidebar component with draggable node type cards.

```
Available node types:
├── Triggers
│   ├── Webhook         ← type: "webhook"
│   └── Email Trigger   ← type: "emailTrigger"
└── Actions
    ├── AI Agent        ← type: "aiAgent"
    ├── Send Email      ← type: "sendEmail"
    └── HTTP Request    ← type: "httpRequest"
```

Each card shows the node type name and a brief description. Drag to canvas to add.

---

## Rules

- Never hardcode `projectKey` — always pass from `import.meta.env.PROJECT_SLUG` or app config
- Install `@xyflow/react` for the visual node editor — do not build a custom canvas from scratch
- Use expressions (`{{$json.output.fieldName}}`) in node config fields — render expression helper tooltips
- All pages must handle loading (`<Skeleton />`), error (`<ErrorAlert />`), and empty states
- Use `cn()` from `@/lib/utils` for all conditional classNames
- Use Tailwind semantic tokens — never hardcode colors
- Execution status badges use consistent colors across all pages
