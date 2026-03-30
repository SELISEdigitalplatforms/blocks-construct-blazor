# Utilities — Frontend Guide (Blazor)

This file extends `core/frontend-blazor.md` with utilities-specific patterns for the utilities skill.
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

All utilities UI lives inside `Modules/Utilities/`:

```
Modules/Utilities/
├── Components/
│   ├── CronExpressionInput.razor          ← cron builder with preset options
│   └── ConfigSettingRow.razor             ← single config key/value row with edit
├── Pages/
│   ├── ScheduledTasksPage.razor           ← list all tasks with status
│   ├── ScheduledTaskEditorPage.razor      ← create/edit task form
│   ├── WebhooksPage.razor                 ← list all webhooks
│   ├── WebhookEditorPage.razor            ← create webhook form
│   └── ConfigPage.razor                   ← list and edit configuration settings
├── Services/
│   └── UtilitiesService.cs               ← raw API calls (no state logic)
└── Models/
    ├── UtilitiesModels.cs                 ← request/response models
    └── UtilitiesValidators.cs             ← FluentValidation validators
```

---

## C# Models (`UtilitiesModels.cs`)

```csharp
// Modules/Utilities/Models/UtilitiesModels.cs

// ── Scheduled Tasks ──────────────────────────────────────────────────────

public class ScheduledTask
{
    public string TaskId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public string TargetUrl { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public string LastRunStatus { get; set; } = string.Empty;
}

public class CreateScheduledTaskPayload
{
    public string Name { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public string TargetUrl { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = "POST";
    public Dictionary<string, string>? Headers { get; set; }
    public Dictionary<string, object>? Body { get; set; }
    public bool IsActive { get; set; } = true;
    public string ProjectKey { get; set; } = string.Empty;
}

public class UpdateScheduledTaskPayload : CreateScheduledTaskPayload
{
    public string TaskId { get; set; } = string.Empty;
}

// ── Webhooks ─────────────────────────────────────────────────────────────

public class Webhook
{
    public string WebhookId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TargetUrl { get; set; } = string.Empty;
    public List<string> Events { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateWebhookPayload
{
    public string Name { get; set; } = string.Empty;
    public string TargetUrl { get; set; } = string.Empty;
    public List<string> Events { get; set; } = new();
    public Dictionary<string, string>? Headers { get; set; }
    public bool IsActive { get; set; } = true;
    public string ProjectKey { get; set; } = string.Empty;
}

// ── Config ───────────────────────────────────────────────────────────────

public class ConfigSetting
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateConfigPayload
{
    public List<ConfigSettingUpdate> Settings { get; set; } = new();
    public string ProjectKey { get; set; } = string.Empty;
}

public class ConfigSettingUpdate
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

// ── Responses ────────────────────────────────────────────────────────────

public class ScheduledTasksResponse
{
    public List<ScheduledTask> Tasks { get; set; } = new();
    public int TotalCount { get; set; }
    public bool IsSuccess { get; set; }
    public Dictionary<string, string> Errors { get; set; } = new();
}

public class WebhooksResponse
{
    public List<Webhook> Webhooks { get; set; } = new();
    public int TotalCount { get; set; }
    public bool IsSuccess { get; set; }
    public Dictionary<string, string> Errors { get; set; } = new();
}

public class ConfigResponse
{
    public List<ConfigSetting> Settings { get; set; } = new();
    public bool IsSuccess { get; set; }
    public Dictionary<string, string> Errors { get; set; } = new();
}
```

---

## Validators (`UtilitiesValidators.cs`)

```csharp
// Modules/Utilities/Models/UtilitiesValidators.cs

using FluentValidation;

public class CreateScheduledTaskValidator : AbstractValidator<CreateScheduledTaskPayload>
{
    public CreateScheduledTaskValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Task name is required");
        RuleFor(x => x.CronExpression).NotEmpty().WithMessage("Cron expression is required");
        RuleFor(x => x.TargetUrl).NotEmpty().Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("A valid target URL is required");
        RuleFor(x => x.HttpMethod).NotEmpty().Must(m => new[] { "GET", "POST", "PUT", "DELETE" }.Contains(m))
            .WithMessage("HTTP method must be GET, POST, PUT, or DELETE");
        RuleFor(x => x.ProjectKey).NotEmpty();
    }
}

public class CreateWebhookValidator : AbstractValidator<CreateWebhookPayload>
{
    public CreateWebhookValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Webhook name is required");
        RuleFor(x => x.TargetUrl).NotEmpty().Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("A valid target URL is required");
        RuleFor(x => x.Events).NotEmpty().WithMessage("At least one event must be selected");
        RuleFor(x => x.ProjectKey).NotEmpty();
    }
}
```

---

## Service (`UtilitiesService.cs`)

```csharp
// Modules/Utilities/Services/UtilitiesService.cs

using System.Net.Http.Json;

public class UtilitiesService
{
    private readonly HttpClient _http;
    private const string Base = "/utilities/v1";

    public UtilitiesService(HttpClient http) => _http = http;

    // ── Scheduled Tasks ──────────────────────────────────────────────────

    public Task<HttpResponseMessage> CreateScheduledTask(CreateScheduledTaskPayload payload) =>
        _http.PostAsJsonAsync($"{Base}/ScheduledTask/Save", payload);

    public Task<HttpResponseMessage> UpdateScheduledTask(UpdateScheduledTaskPayload payload) =>
        _http.PostAsJsonAsync($"{Base}/ScheduledTask/Save", payload);

    public Task<ScheduledTasksResponse?> GetScheduledTasks(string projectKey) =>
        _http.GetFromJsonAsync<ScheduledTasksResponse>($"{Base}/ScheduledTask/Gets?projectKey={projectKey}");

    public Task<HttpResponseMessage> DeleteScheduledTask(string taskId, string projectKey) =>
        _http.DeleteAsync($"{Base}/ScheduledTask/Delete?taskId={taskId}&projectKey={projectKey}");

    // ── Webhooks ─────────────────────────────────────────────────────────

    public Task<HttpResponseMessage> CreateWebhook(CreateWebhookPayload payload) =>
        _http.PostAsJsonAsync($"{Base}/Webhook/Save", payload);

    public Task<WebhooksResponse?> GetWebhooks(string projectKey) =>
        _http.GetFromJsonAsync<WebhooksResponse>($"{Base}/Webhook/Gets?projectKey={projectKey}");

    public Task<HttpResponseMessage> DeleteWebhook(string webhookId, string projectKey) =>
        _http.DeleteAsync($"{Base}/Webhook/Delete?webhookId={webhookId}&projectKey={projectKey}");

    // ── Config ───────────────────────────────────────────────────────────

    public Task<ConfigResponse?> GetConfig(string projectKey) =>
        _http.GetFromJsonAsync<ConfigResponse>($"{Base}/Config/Gets?projectKey={projectKey}");

    public Task<HttpResponseMessage> UpdateConfig(UpdateConfigPayload payload) =>
        _http.PostAsJsonAsync($"{Base}/Config/Save", payload);
}
```

---

## Page: ScheduledTasksPage

Uses `<MudDataGrid>` to list scheduled tasks.

Key behaviors:
- Columns: Name, Cron Expression, Status (chip: green Active / gray Inactive), HTTP Method, Last Run (relative time), Next Run, Last Run Status (chip: green/red/yellow)
- Row actions via `<MudIconButton>`: Edit (pencil icon), Toggle Active (play/pause), Delete (trash, with `<MudDialog>` confirmation)
- Empty state: `<MudAlert>` with "No scheduled tasks" and a "Create Task" button
- FAB: `<MudFab>` in bottom-right to create new task

---

## Page: ScheduledTaskEditorPage

Form for creating/editing a scheduled task using `<MudForm>` and `<MudTextField>`.

Key behaviors:
- `<CronExpressionInput>` component with preset buttons using `<MudChipSet>`
- `<MudSelect>` for HTTP method (GET, POST, PUT, DELETE)
- `<MudTextField>` for target URL with URL validation
- Expandable `<MudExpansionPanel>` for headers — key/value rows with add/remove
- Expandable `<MudExpansionPanel>` for body — `<MudTextField Multiline>` with JSON validation
- `<MudSwitch>` for active toggle
- `<MudButton>` with loading spinner on save

---

## Page: WebhooksPage

Uses `<MudDataGrid>` to list webhooks.

Key behaviors:
- Columns: Name, Target URL, Events (as `<MudChip>` tags), Status (chip), Created Date
- Row actions: Delete with `<MudDialog>` confirmation
- Empty state: `<MudAlert>` with "No webhooks" and a "Create Webhook" button

---

## Page: ConfigPage

Display and edit configuration settings using `<MudSimpleTable>`.

Key behaviors:
- Rows: Key (read-only `<MudText>`), Value (editable `<MudTextField>`), Description (helper text)
- "Save Changes" button collects all modified values into a single `update-config` call
- `<MudProgressLinear>` during save
- `<MudSnackbar>` on success or failure

---

## Rules

- Never hardcode `projectKey` — always inject from `IConfiguration` or app state
- All pages must handle loading (`<MudProgressCircular>`), error (`<MudAlert Severity="Error">`), and empty states
- Register `UtilitiesService` as scoped in `Program.cs`
- Use `<MudDialog>` for all delete confirmations
- Use `@bind-Value` for two-way binding on all form inputs
