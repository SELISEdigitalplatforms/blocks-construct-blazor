# Deployment — Frontend Guide (Blazor + Tailwind)

This file extends `core/frontend-blazor-tailwind.md` with deployment-specific patterns for the deployment skill.
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

All deployment UI lives inside `Modules/Deployment/`:

```
Modules/Deployment/
├── Components/
│   ├── BuildStatusBadge.razor            ← status badge with color coding
│   ├── ServiceConfigForm.razor           ← form for replicas and env vars
│   ├── EnvVarEditor.razor                ← key/value pair editor for env vars
│   ├── RollbackModal.razor               ← confirmation modal for rollback
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

## Component: BuildStatusBadge

Status badge with Tailwind color classes mapped to build status.

```razor
@* Modules/Deployment/Components/BuildStatusBadge.razor *@

@code {
    [Parameter] public string Status { get; set; } = string.Empty;

    private string BadgeClasses => Status switch
    {
        "Queued" => "inline-flex items-center gap-1 rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-medium text-gray-800",
        "InProgress" => "inline-flex items-center gap-1 rounded-full bg-blue-100 px-2.5 py-0.5 text-xs font-medium text-blue-800",
        "Succeeded" => "inline-flex items-center gap-1 rounded-full bg-green-100 px-2.5 py-0.5 text-xs font-medium text-green-800",
        "Failed" => "inline-flex items-center gap-1 rounded-full bg-red-100 px-2.5 py-0.5 text-xs font-medium text-red-800",
        "Cancelled" => "inline-flex items-center gap-1 rounded-full bg-yellow-100 px-2.5 py-0.5 text-xs font-medium text-yellow-800",
        _ => "inline-flex items-center gap-1 rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-medium text-gray-800"
    };
}

<span class="@BadgeClasses">
    @if (Status == "InProgress")
    {
        <svg class="h-3 w-3 animate-spin" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
        </svg>
    }
    @Status
</span>
```

---

## Component: ServiceStatusIndicator

Colored dot indicator for service status.

```razor
@* Modules/Deployment/Components/ServiceStatusIndicator.razor *@

@code {
    [Parameter] public string Status { get; set; } = string.Empty;

    private string DotColor => Status switch
    {
        "Running" => "bg-green-400",
        "Stopped" => "bg-gray-400",
        "Deploying" => "bg-blue-400 animate-pulse",
        "Failed" => "bg-red-400",
        _ => "bg-gray-400"
    };
}

<span class="inline-flex items-center gap-2">
    <span class="h-2.5 w-2.5 rounded-full @DotColor"></span>
    <span class="text-sm text-gray-700">@Status</span>
</span>
```

---

## Page: BuildListPage

Custom table with Tailwind styling, pagination controls, and a trigger build button.

Key behaviors:
- Table with `divide-y divide-gray-200` styling
- Build status shown as `<BuildStatusBadge>`
- "Trigger Build" button opens a modal with repository and branch selection
- Click row navigates to build detail
- Loading state: skeleton rows with `animate-pulse`
- Empty state: centered text with trigger button

---

## Page: ServiceListPage

Card-based or table layout for service overview.

Key behaviors:
- Status indicator with `<ServiceStatusIndicator>`
- Replica count and last deployed time displayed
- Click card/row navigates to `ServiceDetailPage`
- Empty state: centered "No services deployed" message

---

## Page: ServiceDetailPage

Tabbed layout with configuration and deployment history.

Key behaviors:
- Tab navigation using Tailwind-styled tab buttons with `border-b-2` active indicator
- Configuration tab: form with number input for replicas, `<EnvVarEditor>` for env vars
- Deployment history tab: table with rollback buttons
- Save button with spinner SVG during loading
- Success/error shown via `ToastService`

---

## Page: DeploymentHistoryPage

Paginated table for deployment history with rollback capability.

Key behaviors:
- Table with `divide-y` styling and deployment status badges
- Rollback button (Heroicon `arrow-uturn-left`) on `Succeeded` rows
- Rollback opens `<RollbackModal>` confirmation
- Pagination controls with previous/next buttons
- Loading state: skeleton rows with `animate-pulse`
- Empty state: "No deployments found"

---

## Build Status Polling

Use `System.Timers.Timer` for polling build status:

```csharp
private Timer? _pollTimer;

private void StartPolling(string buildId)
{
    _pollTimer = new Timer(10_000);
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
- Rollback must always show a modal confirmation before executing
- Environment variable values should be masked by default — use `type="password"` input with a toggle button
- All pages must handle loading (skeleton with `animate-pulse`), error (red alert div), and empty states
- Use `ToastService` for success/error notifications after mutations
- Register `DeploymentService` as scoped in DI
- Use Tailwind semantic color classes — avoid hardcoded hex values
- All interactive elements must have `focus:ring-2 focus:ring-offset-2` focus styles
