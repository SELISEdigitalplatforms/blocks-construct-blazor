# Deployment — Frontend Guide (Blazor)

This file extends `core/frontend-blazor.md` with deployment-specific patterns for the deployment skill.
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

All deployment UI lives inside `Modules/Deployment/`:

```
Modules/Deployment/
├── Components/
│   ├── BuildStatusBadge.razor            ← status badge with color coding
│   ├── ServiceConfigForm.razor           ← form for replicas and env vars
│   ├── EnvVarEditor.razor                ← key/value pair editor for env vars
│   ├── RollbackDialog.razor              ← confirmation dialog for rollback
│   └── ServiceStatusIndicator.razor      ← colored dot indicator for service status
├── Pages/
│   ├── BuildListPage.razor               ← paginated build list with trigger button
│   ├── ServiceListPage.razor             ← service overview with status indicators
│   ├── ServiceDetailPage.razor           ← service config editor and deployment info
│   └── DeploymentHistoryPage.razor       ← paginated deployment history with rollback
├── Services/
│   └── DeploymentService.cs              ← raw API calls (no state logic)
└── Models/
    ├── DeploymentModels.cs               ← request/response models
    └── DeploymentValidators.cs           ← FluentValidation validators
```

---

## C# Models (`DeploymentModels.cs`)

```csharp
// Modules/Deployment/Models/DeploymentModels.cs

// ── Build ────────────────────────────────────────────────────────────────────

public class Build
{
    public string BuildId { get; set; } = string.Empty;
    public string RepositoryId { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string CommitHash { get; set; } = string.Empty;
    public string CommitMessage { get; set; } = string.Empty;
}

public class TriggerBuildPayload
{
    public string RepositoryId { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
    public string ProjectKey { get; set; } = string.Empty;
}

public class GetBuildsResponse
{
    public List<Build> Builds { get; set; } = new();
    public int TotalCount { get; set; }
    public bool IsSuccess { get; set; }
    public Dictionary<string, string> Errors { get; set; } = new();
}

public class GetBuildResponse
{
    public Build? Build { get; set; }
    public bool IsSuccess { get; set; }
    public Dictionary<string, string> Errors { get; set; } = new();
}

public class TriggerBuildResponse
{
    public string BuildId { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public Dictionary<string, string> Errors { get; set; } = new();
}

// ── Service ──────────────────────────────────────────────────────────────────

public class ServiceInfo
{
    public string ServiceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Replicas { get; set; }
    public DateTime LastDeployedAt { get; set; }
    public string Repository { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
}

public class ServiceConfigPayload
{
    public string ServiceId { get; set; } = string.Empty;
    public int? Replicas { get; set; }
    public Dictionary<string, string>? EnvVars { get; set; }
    public string ProjectKey { get; set; } = string.Empty;
}

public class GetServicesResponse
{
    public List<ServiceInfo> Services { get; set; } = new();
    public bool IsSuccess { get; set; }
    public Dictionary<string, string> Errors { get; set; } = new();
}

public class GetServiceResponse
{
    public ServiceInfo? Service { get; set; }
    public bool IsSuccess { get; set; }
    public Dictionary<string, string> Errors { get; set; } = new();
}

// ── Deployment ───────────────────────────────────────────────────────────────

public class DeploymentRecord
{
    public string DeploymentId { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;
    public string BuildId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DeployedAt { get; set; }
    public string DeployedBy { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

public class RollbackPayload
{
    public string ServiceId { get; set; } = string.Empty;
    public string DeploymentId { get; set; } = string.Empty;
    public string ProjectKey { get; set; } = string.Empty;
}

public class DeploymentHistoryResponse
{
    public List<DeploymentRecord> Deployments { get; set; } = new();
    public int TotalCount { get; set; }
    public bool IsSuccess { get; set; }
    public Dictionary<string, string> Errors { get; set; } = new();
}
```

---

## Validators (`DeploymentValidators.cs`)

```csharp
// Modules/Deployment/Models/DeploymentValidators.cs

using FluentValidation;

public class TriggerBuildValidator : AbstractValidator<TriggerBuildPayload>
{
    public TriggerBuildValidator()
    {
        RuleFor(x => x.RepositoryId).NotEmpty().WithMessage("Repository is required");
        RuleFor(x => x.Branch).NotEmpty().WithMessage("Branch is required");
        RuleFor(x => x.ProjectKey).NotEmpty();
    }
}

public class ServiceConfigValidator : AbstractValidator<ServiceConfigPayload>
{
    public ServiceConfigValidator()
    {
        RuleFor(x => x.ServiceId).NotEmpty().WithMessage("Service is required");
        RuleFor(x => x.Replicas)
            .InclusiveBetween(1, 10)
            .When(x => x.Replicas.HasValue)
            .WithMessage("Replicas must be between 1 and 10");
        RuleFor(x => x.ProjectKey).NotEmpty();
    }
}

public class RollbackValidator : AbstractValidator<RollbackPayload>
{
    public RollbackValidator()
    {
        RuleFor(x => x.ServiceId).NotEmpty().WithMessage("Service is required");
        RuleFor(x => x.DeploymentId).NotEmpty().WithMessage("Deployment is required");
        RuleFor(x => x.ProjectKey).NotEmpty();
    }
}
```

---

## Service (`DeploymentService.cs`)

```csharp
// Modules/Deployment/Services/DeploymentService.cs

using System.Net.Http.Json;

public class DeploymentService
{
    private readonly HttpClient _http;
    private const string Base = "/deployment/v1";

    public DeploymentService(HttpClient http) => _http = http;

    // ── Build ────────────────────────────────────────────────────────────

    public Task<TriggerBuildResponse?> TriggerBuildAsync(TriggerBuildPayload payload) =>
        _http.PostAsJsonAsync($"{Base}/Build/Trigger", payload)
             .ContinueWith(t => t.Result.Content.ReadFromJsonAsync<TriggerBuildResponse>()).Unwrap();

    public Task<GetBuildsResponse?> GetBuildsAsync(string projectKey, int page, int pageSize) =>
        _http.GetFromJsonAsync<GetBuildsResponse>(
            $"{Base}/Build/Gets?projectKey={projectKey}&page={page}&pageSize={pageSize}");

    public Task<GetBuildResponse?> GetBuildAsync(string buildId, string projectKey) =>
        _http.GetFromJsonAsync<GetBuildResponse>(
            $"{Base}/Build/Get?buildId={buildId}&projectKey={projectKey}");

    // ── Service ──────────────────────────────────────────────────────────

    public Task<GetServicesResponse?> GetServicesAsync(string projectKey) =>
        _http.GetFromJsonAsync<GetServicesResponse>(
            $"{Base}/Service/Gets?projectKey={projectKey}");

    public Task<GetServiceResponse?> GetServiceAsync(string serviceId, string projectKey) =>
        _http.GetFromJsonAsync<GetServiceResponse>(
            $"{Base}/Service/Get?serviceId={serviceId}&projectKey={projectKey}");

    public Task<BaseResponse?> UpdateServiceConfigAsync(ServiceConfigPayload payload) =>
        _http.PostAsJsonAsync($"{Base}/Service/UpdateConfig", payload)
             .ContinueWith(t => t.Result.Content.ReadFromJsonAsync<BaseResponse>()).Unwrap();

    // ── Deployment ───────────────────────────────────────────────────────

    public Task<DeploymentHistoryResponse?> GetDeploymentHistoryAsync(
        string serviceId, string projectKey, int page, int pageSize) =>
        _http.GetFromJsonAsync<DeploymentHistoryResponse>(
            $"{Base}/Deployment/Gets?serviceId={serviceId}&projectKey={projectKey}&page={page}&pageSize={pageSize}");

    public Task<BaseResponse?> RollbackDeploymentAsync(RollbackPayload payload) =>
        _http.PostAsJsonAsync($"{Base}/Deployment/Rollback", payload)
             .ContinueWith(t => t.Result.Content.ReadFromJsonAsync<BaseResponse>()).Unwrap();
}
```

---

## Page: BuildListPage

Uses `<MudTable>` with server-side pagination. Trigger build button opens a `<MudDialog>` with repository and branch selection.

Key behaviors:
- `<MudTable ServerData="@LoadBuilds">` for paginated data
- Build status shown as `<MudChip>` with color mapped to status
- "Trigger Build" button in toolbar opens dialog
- Click row navigates to build detail view
- Loading state: `<MudProgressLinear>` above table
- Empty state: `<MudAlert>` with "No builds found"

---

## Page: ServiceListPage

Uses `<MudTable>` to list services with status indicators.

Key behaviors:
- `<MudChip>` color-coded by service status (Running=Success, Stopped=Default, Failed=Error, Deploying=Info)
- Replica count displayed as `<MudText>`
- Click row navigates to `ServiceDetailPage`
- Empty state: `<MudAlert>` with "No services deployed"

---

## Page: ServiceDetailPage

Service detail with config editing and deployment history tabs.

Key behaviors:
- `<MudTabs>` with "Configuration" and "Deployment History" tabs
- Configuration tab: `ServiceConfigForm` with replica slider and env var editor
- Deployment History tab: `<MudTable>` with rollback buttons
- `<MudButton OnClick="SaveConfig">` with `<MudProgressCircular>` during save
- Success/error shown via `ISnackbar`

---

## Page: DeploymentHistoryPage

Uses `<MudTable>` with server-side pagination for deployment history.

Key behaviors:
- Deployment status as `<MudChip>` with color mapping
- Rollback button (icon: `@Icons.Material.Filled.Restore`) on `Succeeded` rows
- Rollback opens `<MudDialog>` confirmation before executing
- Loading state: `<MudProgressLinear>`

---

## Build Status Polling

Use `System.Timers.Timer` for polling build status:

```csharp
private Timer? _pollTimer;

private void StartPolling(string buildId)
{
    _pollTimer = new Timer(10_000); // 10 seconds
    _pollTimer.Elapsed += async (_, _) =>
    {
        var result = await DeploymentService.GetBuildAsync(buildId, ProjectKey);
        if (result?.Build?.Status is "Succeeded" or "Failed" or "Cancelled")
        {
            _pollTimer?.Stop();
            _pollTimer?.Dispose();
        }
        await InvokeAsync(StateHasChanged);
    };
    _pollTimer.Start();
}
```

---

## Rules

- Never hardcode `projectKey` — always inject from configuration
- Build status polling must stop on terminal states — dispose the timer
- Rollback must always show a `<MudDialog>` confirmation before executing
- Environment variable values should be masked by default — use `<MudTextField InputType="InputType.Password">` with a toggle
- All pages must handle loading (`<MudProgressLinear>`), error (`<MudAlert Severity="Severity.Error">`), and empty states
- Use `ISnackbar` for success/error toasts after mutations
- Register `DeploymentService` as scoped in DI
