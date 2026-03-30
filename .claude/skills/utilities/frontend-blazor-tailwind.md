# Utilities — Frontend Guide (Blazor + Tailwind)

This file extends `core/frontend-blazor-tailwind.md` with utilities-specific patterns for the utilities skill.
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

All utilities UI lives inside `Modules/Utilities/`:

```
Modules/Utilities/
├── Components/
│   ├── CronExpressionInput.razor          ← cron builder with preset options
│   ├── ConfigSettingRow.razor             ← single config key/value row with edit
│   └── DeleteConfirmModal.razor           ← reusable delete confirmation modal
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

Plain HTML table styled with Tailwind.

Key behaviors:
- Table columns: Name, Cron Expression, Status (badge: `bg-green-100 text-green-800` for active, `bg-gray-100 text-gray-800` for inactive), HTTP Method, Last Run, Next Run, Last Run Status
- Row actions: Edit button, Toggle active/inactive button, Delete button (opens `DeleteConfirmModal`)
- Empty state: centered text with "No scheduled tasks" and a primary button to create
- Loading state: skeleton rows with `animate-pulse` placeholder divs

```html
<!-- Status badge pattern -->
<span class="inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium bg-green-100 text-green-800">
  Active
</span>
```

---

## Page: ScheduledTaskEditorPage

Form styled with Tailwind `@tailwindcss/forms`.

Key behaviors:
- `<CronExpressionInput>` with preset buttons as `rounded-full` pills
- HTTP method as `<select>` dropdown
- Target URL as `<input type="url">`
- Collapsible headers section using `<details>/<summary>` — key/value pair rows
- Collapsible body section — `<textarea>` for JSON input
- Toggle switch for active state using custom Tailwind toggle pattern
- Submit button with spinner SVG during loading

```html
<!-- Tailwind toggle pattern -->
<button type="button" role="switch" aria-checked="true"
  class="relative inline-flex h-6 w-11 flex-shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out bg-primary focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2">
  <span class="translate-x-5 pointer-events-none relative inline-block h-5 w-5 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out"></span>
</button>
```

---

## Page: WebhooksPage

Plain HTML table styled with Tailwind.

Key behaviors:
- Table columns: Name, Target URL, Events (as inline tags), Status (badge), Created Date
- Row action: Delete with `DeleteConfirmModal`
- Event tags: `<span class="inline-flex items-center rounded-md bg-blue-50 px-2 py-1 text-xs font-medium text-blue-700 ring-1 ring-inset ring-blue-700/10">`
- Empty state and loading state follow the same patterns as ScheduledTasksPage

---

## Page: ConfigPage

Editable settings table with inline editing.

Key behaviors:
- Table rows: Key (read-only text), Value (editable `<input>`), Description (small muted text below value)
- "Save Changes" button at the top, disabled when no changes detected
- Loading bar during save: `<div class="h-1 w-full bg-primary/20"><div class="h-full bg-primary animate-[grow_1s_ease-in-out_infinite]"></div></div>`
- Toast notification via `ToastService` on success or failure

---

## Component: CronExpressionInput

```razor
@* Modules/Utilities/Components/CronExpressionInput.razor *@

<div class="space-y-2">
    <label class="block text-sm font-medium text-gray-700">Cron Expression</label>
    <input type="text" @bind="Value" @bind:event="oninput"
           class="block w-full rounded-md border-gray-300 shadow-sm focus:border-primary focus:ring-primary sm:text-sm font-mono"
           placeholder="* * * * *" />
    <div class="flex flex-wrap gap-2">
        @foreach (var preset in _presets)
        {
            <button type="button" @onclick="() => SetPreset(preset.Value)"
                    class="@GetPresetClass(preset.Value)">
                @preset.Label
            </button>
        }
    </div>
</div>

@code {
    [Parameter] public string Value { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> ValueChanged { get; set; }

    private readonly (string Label, string Value)[] _presets = new[]
    {
        ("Every minute", "* * * * *"),
        ("Every 5 min", "*/5 * * * *"),
        ("Every hour", "0 * * * *"),
        ("Daily midnight", "0 0 * * *"),
        ("Monday 9 AM", "0 9 * * 1"),
        ("Monthly", "0 0 1 * *"),
    };

    private async Task SetPreset(string value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
    }

    private string GetPresetClass(string presetValue) =>
        $"inline-flex items-center rounded-full px-3 py-1 text-xs font-medium cursor-pointer transition-colors " +
        (Value == presetValue
            ? "bg-primary text-white"
            : "bg-gray-100 text-gray-700 hover:bg-gray-200");
}
```

---

## Component: DeleteConfirmModal

```razor
@* Modules/Utilities/Components/DeleteConfirmModal.razor *@

@if (IsOpen)
{
    <div class="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
        <div class="bg-white rounded-lg shadow-xl max-w-md w-full mx-4 p-6">
            <h3 class="text-lg font-semibold text-gray-900">Confirm Deletion</h3>
            <p class="mt-2 text-sm text-gray-600">@Message</p>
            <div class="mt-4 flex justify-end gap-3">
                <button @onclick="OnCancel" class="rounded-md bg-white px-3 py-2 text-sm font-semibold text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 hover:bg-gray-50">
                    Cancel
                </button>
                <button @onclick="OnConfirm" disabled="@IsDeleting"
                        class="rounded-md bg-red-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-red-500 disabled:opacity-50">
                    @(IsDeleting ? "Deleting..." : "Delete")
                </button>
            </div>
        </div>
    </div>
}

@code {
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public string Message { get; set; } = "Are you sure? This action cannot be undone.";
    [Parameter] public bool IsDeleting { get; set; }
    [Parameter] public EventCallback OnConfirm { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
}
```

---

## Rules

- Never hardcode `projectKey` — always inject from `IConfiguration` or app state
- All pages must handle loading (skeleton with `animate-pulse`), error (red alert banner), and empty states
- Register `UtilitiesService` as scoped in `Program.cs`
- Use `DeleteConfirmModal` for all delete confirmations
- Use semantic Tailwind tokens via CSS variables for theme colors (e.g. `bg-primary`, `text-primary`)
- All form inputs use `@tailwindcss/forms` plugin styling
