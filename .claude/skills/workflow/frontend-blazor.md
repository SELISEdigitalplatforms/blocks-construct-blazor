# Workflow — Frontend Guide (Blazor)

This file extends `core/frontend-blazor.md` with workflow-specific patterns for the workflow skill.
Always read `core/frontend-blazor.md` first, then apply the overrides and additions here.

---

## Stack

| Layer | Technology |
|-------|-----------|
| Framework | .NET 10 Blazor WebAssembly (standalone) |
| Component library | MudBlazor |
| State management | Scoped services + event pattern |
| HTTP | `HttpClient` + `DelegatingHandler` |
| Validation | FluentValidation |
| i18n | `ILocalizationService` (remote JSON resources) |
| Icons | MudBlazor built-in icons (Material Design) |

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

Paginated workflow list using `MudDataGrid`.

Key behaviors:
- `MudDataGrid<WorkflowSummary>` with columns: Name, Description, Status, Created Date, Actions
- Status column: `MudChip` with green "Active" or gray "Inactive"
- Actions column: Edit (`MudIconButton` with `Icons.Material.Filled.Edit`), History (`Icons.Material.Filled.History`), Delete (`Icons.Material.Filled.Delete`)
- `MudSwitch` inline for activate/deactivate toggle
- Empty state: `MudAlert` with "No workflows yet" and a `MudButton` "Create Workflow"
- Toolbar: `MudButton` "Create Workflow" with `Icons.Material.Filled.Add`
- Loading: `MudProgressLinear` while fetching
- Pagination via `MudDataGrid` server-side paging

---

## Page: WorkflowEditorPage

Visual workflow builder with three-panel layout using `MudGrid`.

Key behaviors:
- Left panel (`MudPaper`): Node palette with `MudList` of draggable node types
- Center panel: Workflow canvas (custom Blazor component with SVG-based node rendering)
- Right panel (`MudDrawer`): Node configuration form that opens when a node is selected
- Toolbar: `MudButton` Save, `MudSwitch` Activate/Deactivate, `MudButton` Back
- Node config forms use `MudTextField`, `MudSelect`, `MudNumericField` based on node type
- Validation via FluentValidation before save
- Load existing workflow via query parameter `workflowId`

---

## Page: ExecutionHistoryPage

Paginated execution list using `MudDataGrid`.

Key behaviors:
- `MudDataGrid<Execution>` with columns: Execution ID, Status, Start Time, End Time, Duration
- Status column: `MudChip` with color-coded status (`completed` = Success, `failed` = Error, `running` = Info, `pending` = Default, `cancelled` = Warning)
- Click row to navigate to execution detail
- Loading: `MudProgressLinear`
- Empty state: `MudAlert` with "No executions yet"

---

## Page: ExecutionDetailPage

Detailed execution view showing per-node results.

Key behaviors:
- Overall status and duration in a `MudAlert` at top
- Each `NodeResult` rendered as a `MudCard` with:
  - `MudCardHeader`: node name + status `MudChip`
  - `MudCardContent`: duration, collapsible output JSON in `MudExpansionPanel`
- Failed nodes highlighted with `MudAlert Severity="Severity.Error"`
- JSON output rendered in `<pre>` with `MudPaper` background
- Back button to return to execution history

---

## Component: WorkflowCanvas

Custom Blazor canvas component for visual node editing. Since there is no direct React Flow equivalent in Blazor, implement a simplified canvas.

Key behaviors:
- Render nodes as positioned `MudPaper` cards using absolute positioning
- Draw edges as SVG lines between nodes
- Click a node to select it and open the `NodeEditor`
- Drag-and-drop from `NodePalette` to add new nodes
- Support connecting nodes by click-to-connect pattern

---

## Component: NodeEditor

Side panel for configuring the selected node's properties.

Key behaviors:
- Dynamically render the correct form based on `node.Type`
- Webhook: no config fields (auto-generated URL shown as read-only after save)
- Email Trigger: `MudTextField` for IMAP host, port, email, password; `MudNumericField` for poll interval
- AI Agent: `MudTextField` for agent ID
- Send Email: `MudTextField` for template ID, to (with expression helper tooltip); `MudTextField` pairs for bodyDataContext
- HTTP Request: `MudSelect` for method, `MudTextField` for URL, dynamic key/value pairs for headers/body/queryParams
- Expression helper: `MudTooltip` showing `{{$json.output.fieldName}}` syntax

---

## Rules

- Never hardcode `projectKey` — always resolve from app configuration
- Use `MudBlazor` components exclusively — do not mix with plain HTML for interactive elements
- All pages must handle loading (`MudProgressLinear` or `MudSkeleton`), error (`MudAlert`), and empty states
- Execution status colors must be consistent across all pages
- Register `IWorkflowService` as scoped in `Program.cs`
