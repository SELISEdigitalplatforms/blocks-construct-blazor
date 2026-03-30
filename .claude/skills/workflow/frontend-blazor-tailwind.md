# Workflow — Frontend Guide (Blazor + Tailwind)

This file extends `core/frontend-blazor-tailwind.md` with workflow-specific patterns for the workflow skill.
Always read `core/frontend-blazor-tailwind.md` first, then apply the overrides and additions here.

---

## Stack

| Layer | Technology |
|-------|-----------|
| Framework | .NET 10 Blazor WebAssembly (standalone) |
| Styling | Tailwind CSS + `@tailwindcss/forms` |
| Component library | None — plain HTML + Tailwind utility classes |
| State management | Scoped services + event pattern |
| HTTP | `HttpClient` + `DelegatingHandler` |
| Validation | FluentValidation |
| i18n | `ILocalizationService` (remote JSON resources) |
| Icons | Heroicons (inline SVG) |
| Notifications | Custom `ToastService` |

---

## Module Structure

All workflow UI lives inside `Modules/Workflow/`:

```
Modules/Workflow/
├── Components/
│   ├── WorkflowCanvas.razor             ← visual node editor canvas
│   ├── NodeEditor.razor                 ← side panel for configuring selected node
│   ├── NodePalette.razor                ← draggable node type list
│   ├── TriggerNodeCard.razor            ← visual card for trigger nodes on canvas
│   ├── ActionNodeCard.razor             ← visual card for action nodes on canvas
│   ├── StatusBadge.razor                ← reusable status badge component
│   └── ExecutionNodeResult.razor        ← per-node result display in execution detail
├── Pages/
│   ├── WorkflowListPage.razor           ← paginated workflow list with status and actions
│   ├── WorkflowEditorPage.razor         ← visual workflow builder page
│   ├── ExecutionHistoryPage.razor       ← paginated execution list for a workflow
│   └── ExecutionDetailPage.razor        ← per-node execution results view
├── Services/
│   └── WorkflowService.cs              ← raw API calls (no state logic)
└── Models/
    ├── WorkflowModels.cs               ← request/response models
    └── WorkflowValidators.cs           ← FluentValidation validators
```

---

## C# Models (`WorkflowModels.cs`)

```csharp
// Modules/Workflow/Models/WorkflowModels.cs

// ── Node Types ───────────────────────────────────────────────────────────────

public class WorkflowNode
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // webhook, emailTrigger, aiAgent, sendEmail, httpRequest
    public NodePosition Position { get; set; } = new();
    public Dictionary<string, object> Config { get; set; } = new();
}

public class NodePosition
{
    public double X { get; set; }
    public double Y { get; set; }
}

public class EmailTriggerConfig
{
    public string ImapHost { get; set; } = string.Empty;
    public int ImapPort { get; set; } = 993;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int PollInterval { get; set; } = 60;
}

public class AIAgentConfig
{
    public string AgentId { get; set; } = string.Empty;
}

public class SendEmailConfig
{
    public string TemplateId { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public Dictionary<string, string>? BodyDataContext { get; set; }
}

public class HTTPRequestConfig
{
    public string Method { get; set; } = "GET";
    public string Url { get; set; } = string.Empty;
    public Dictionary<string, string>? Headers { get; set; }
    public string? Body { get; set; }
    public Dictionary<string, string>? QueryParams { get; set; }
}

// ── Edges ────────────────────────────────────────────────────────────────────

public class WorkflowEdge
{
    public string Id { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
}

// ── Workflow ─────────────────────────────────────────────────────────────────

public class Workflow
{
    public string WorkflowId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<WorkflowNode> Nodes { get; set; } = new();
    public List<WorkflowEdge> Edges { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastUpdatedDate { get; set; }
}

public class WorkflowSummary
{
    public string WorkflowId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastUpdatedDate { get; set; }
}

// ── Payloads ─────────────────────────────────────────────────────────────────

public class CreateWorkflowPayload
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<WorkflowNode> Nodes { get; set; } = new();
    public List<WorkflowEdge> Edges { get; set; } = new();
    public string ProjectKey { get; set; } = string.Empty;
}

public class UpdateWorkflowPayload : CreateWorkflowPayload
{
    public string WorkflowId { get; set; } = string.Empty;
}

public class ActivateDeactivatePayload
{
    public string WorkflowId { get; set; } = string.Empty;
    public string ProjectKey { get; set; } = string.Empty;
}

// ── Executions ───────────────────────────────────────────────────────────────

public class NodeResult
{
    public string NodeId { get; set; } = string.Empty;
    public string NodeName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, object>? Output { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

public class Execution
{
    public string ExecutionId { get; set; } = string.Empty;
    public string WorkflowId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<NodeResult> NodeResults { get; set; } = new();
}
```

---

## Service (`WorkflowService.cs`)

```csharp
// Modules/Workflow/Services/WorkflowService.cs

public interface IWorkflowService
{
    Task<ApiResponse<SaveWorkflowResponse>> CreateWorkflowAsync(CreateWorkflowPayload payload);
    Task<ApiResponse<SaveWorkflowResponse>> UpdateWorkflowAsync(UpdateWorkflowPayload payload);
    Task<ApiResponse<GetWorkflowsResponse>> GetWorkflowsAsync(string projectKey, int page, int pageSize);
    Task<ApiResponse<GetWorkflowResponse>> GetWorkflowAsync(string workflowId, string projectKey);
    Task<ApiResponse<BaseResponse>> DeleteWorkflowAsync(string workflowId, string projectKey);
    Task<ApiResponse<BaseResponse>> ActivateWorkflowAsync(ActivateDeactivatePayload payload);
    Task<ApiResponse<BaseResponse>> DeactivateWorkflowAsync(ActivateDeactivatePayload payload);
    Task<ApiResponse<GetExecutionsResponse>> GetExecutionsAsync(string workflowId, string projectKey, int page, int pageSize);
    Task<ApiResponse<GetExecutionResponse>> GetExecutionAsync(string executionId, string projectKey);
}

public class WorkflowService : IWorkflowService
{
    private readonly HttpClient _http;
    private const string Base = "/workflow/v1";

    public WorkflowService(HttpClient http) => _http = http;

    public async Task<ApiResponse<SaveWorkflowResponse>> CreateWorkflowAsync(CreateWorkflowPayload payload)
        => await _http.PostAsJsonAsync<SaveWorkflowResponse>($"{Base}/Workflow/Save", payload);

    public async Task<ApiResponse<SaveWorkflowResponse>> UpdateWorkflowAsync(UpdateWorkflowPayload payload)
        => await _http.PostAsJsonAsync<SaveWorkflowResponse>($"{Base}/Workflow/Save", payload);

    public async Task<ApiResponse<GetWorkflowsResponse>> GetWorkflowsAsync(string projectKey, int page, int pageSize)
        => await _http.GetFromJsonAsync<GetWorkflowsResponse>($"{Base}/Workflow/Gets?projectKey={projectKey}&page={page}&pageSize={pageSize}");

    public async Task<ApiResponse<GetWorkflowResponse>> GetWorkflowAsync(string workflowId, string projectKey)
        => await _http.GetFromJsonAsync<GetWorkflowResponse>($"{Base}/Workflow/Get?workflowId={workflowId}&projectKey={projectKey}");

    public async Task<ApiResponse<BaseResponse>> DeleteWorkflowAsync(string workflowId, string projectKey)
        => await _http.DeleteAsync<BaseResponse>($"{Base}/Workflow/Delete?workflowId={workflowId}&projectKey={projectKey}");

    public async Task<ApiResponse<BaseResponse>> ActivateWorkflowAsync(ActivateDeactivatePayload payload)
        => await _http.PostAsJsonAsync<BaseResponse>($"{Base}/Workflow/Activate", payload);

    public async Task<ApiResponse<BaseResponse>> DeactivateWorkflowAsync(ActivateDeactivatePayload payload)
        => await _http.PostAsJsonAsync<BaseResponse>($"{Base}/Workflow/Deactivate", payload);

    public async Task<ApiResponse<GetExecutionsResponse>> GetExecutionsAsync(string workflowId, string projectKey, int page, int pageSize)
        => await _http.GetFromJsonAsync<GetExecutionsResponse>($"{Base}/Execution/Gets?workflowId={workflowId}&projectKey={projectKey}&page={page}&pageSize={pageSize}");

    public async Task<ApiResponse<GetExecutionResponse>> GetExecutionAsync(string executionId, string projectKey)
        => await _http.GetFromJsonAsync<GetExecutionResponse>($"{Base}/Execution/Get?executionId={executionId}&projectKey={projectKey}");
}
```

---

## Page: WorkflowListPage

Paginated workflow list using plain HTML table with Tailwind styling.

Key behaviors:
- `<table>` with `divide-y divide-gray-200` styling
- Columns: Name, Description, Status, Created Date, Actions
- Status: `<span>` badge with `bg-green-100 text-green-800` for Active, `bg-gray-100 text-gray-800` for Inactive
- Actions: icon buttons for Edit, History, Delete using Heroicons SVGs
- Toggle switch for activate/deactivate: custom `<button>` with `role="switch"` and `aria-checked`
- Empty state: centered `<div>` with "No workflows yet" text and "Create Workflow" `<button>`
- Toolbar: "Create Workflow" `<button>` with `bg-primary-600 hover:bg-primary-700 text-white rounded-lg px-4 py-2`
- Loading: skeleton rows using `animate-pulse` with `bg-gray-200 rounded`
- Pagination: custom `<nav>` with Previous/Next buttons and page indicators

---

## Page: WorkflowEditorPage

Visual workflow builder with three-panel layout using CSS Grid.

```html
<div class="grid grid-cols-[240px_1fr_320px] h-[calc(100vh-64px)]">
  <!-- Left: Node Palette -->
  <!-- Center: Canvas -->
  <!-- Right: Node Config (conditional) -->
</div>
```

Key behaviors:
- Left panel: `NodePalette` with draggable node type cards
- Center panel: `WorkflowCanvas` with SVG-based node rendering and absolute-positioned node cards
- Right panel: `NodeEditor` that slides in when a node is selected
- Toolbar: `<div class="flex items-center gap-2 p-4 border-b">` with Save button, Activate/Deactivate toggle, Back link
- Node config forms use `<input>`, `<select>`, `<textarea>` with `@tailwindcss/forms` styling
- Validation feedback: `<p class="text-sm text-red-600">` under invalid fields

---

## Page: ExecutionHistoryPage

Paginated execution table with Tailwind styling.

Key behaviors:
- `<table>` with columns: Execution ID (truncated), Status, Start Time, End Time, Duration
- Status badges with Tailwind colors:
  - `completed`: `bg-green-100 text-green-800`
  - `failed`: `bg-red-100 text-red-800`
  - `running`: `bg-blue-100 text-blue-800`
  - `pending`: `bg-gray-100 text-gray-800`
  - `cancelled`: `bg-yellow-100 text-yellow-800`
- Click row to navigate to execution detail (entire `<tr>` is clickable with `cursor-pointer hover:bg-gray-50`)
- Loading: skeleton rows with `animate-pulse`
- Empty state: "No executions yet"

---

## Page: ExecutionDetailPage

Detailed execution view showing per-node results as cards.

Key behaviors:
- Overall status banner: `<div class="rounded-lg p-4">` with status-appropriate background color
- Each `NodeResult` as a card: `<div class="border rounded-lg p-4 mb-4">`
  - Header: node name + status badge
  - Duration display
  - Collapsible output JSON: `<details><summary>` with `<pre class="bg-gray-50 p-4 rounded text-sm overflow-x-auto">`
- Failed nodes: `border-red-300 bg-red-50` card styling with error message in `<p class="text-red-700">`
- Back button: `<a>` styled as link with left arrow Heroicon

---

## Component: WorkflowCanvas

Custom Blazor canvas for visual node editing using SVG and absolute positioning.

Key behaviors:
- Container: `<div class="relative w-full h-full overflow-auto bg-gray-50">` with dot grid background via CSS
- Nodes: `<div class="absolute">` cards positioned using inline `style="left: {x}px; top: {y}px"`
- Trigger nodes: `border-l-4 border-blue-500 bg-white rounded-lg shadow-sm p-3`
- Action nodes: `border-l-4 border-green-500 bg-white rounded-lg shadow-sm p-3`
- Edges: `<svg class="absolute inset-0 pointer-events-none">` with `<path>` elements for curves between nodes
- Click node to select (highlight with `ring-2 ring-primary-500`)
- Drag-and-drop from palette to add new nodes

---

## Component: NodeEditor

Side panel for configuring selected node properties.

Key behaviors:
- Panel: `<div class="border-l bg-white p-4 overflow-y-auto">`
- Header: node type name + close button
- Forms use Tailwind `@tailwindcss/forms` styled inputs:
  - `<input class="block w-full rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500 sm:text-sm">`
  - `<select>` with same styling for method dropdowns
  - `<textarea>` for body/JSON content
- Dynamic key/value pair editor for headers/queryParams: rows of two `<input>` fields with add/remove buttons
- Expression helper: `<span class="text-xs text-gray-500">` showing `{{$json.output.fieldName}}` syntax hint

---

## Component: StatusBadge

Reusable badge component for execution/workflow status.

```razor
@* Modules/Workflow/Components/StatusBadge.razor *@

<span class="@GetStatusClasses()">
    @Status
</span>

@code {
    [Parameter] public string Status { get; set; } = string.Empty;

    private string GetStatusClasses() => Status switch
    {
        "completed" => "inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium bg-green-100 text-green-800",
        "failed" => "inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium bg-red-100 text-red-800",
        "running" => "inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium bg-blue-100 text-blue-800",
        "pending" => "inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium bg-gray-100 text-gray-800",
        "cancelled" => "inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium bg-yellow-100 text-yellow-800",
        _ => "inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium bg-gray-100 text-gray-800",
    };
}
```

---

## Rules

- Never hardcode `projectKey` — always resolve from app configuration
- Use pure Tailwind utility classes — do not import any component library
- All interactive elements must have proper `aria-` attributes for accessibility
- All pages must handle loading (skeleton with `animate-pulse`), error (red alert banner), and empty states
- Execution status colors must be consistent across all pages — use the `StatusBadge` component
- Use `@tailwindcss/forms` plugin for all form inputs
- Register `IWorkflowService` as scoped in `Program.cs`
