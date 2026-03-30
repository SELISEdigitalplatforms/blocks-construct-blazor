# Copilot Instructions — Blocks Construct Blazor

## Project Overview

This is a SELISE Blocks Blazor WASM application with the following architecture:

- **Client** (.NET 10 Blazor WASM): SPA frontend components and pages
- **Server** (.NET 10 Blazor Server): Backend hosting and API routes
- **Services** (.NET 10 class library): Shared business logic and service layer
- **Worker** (.NET 10 worker service): Background job processor
- **Test** (.NET 10 xUnit): Unit test projects

## Technology Stack

- **Frontend**: Blazor WASM (.NET 10) with Tailwind CSS v4 (only CSS framework — no other CSS libraries or scoped CSS)
- **Backend**: ASP.NET Core 10, GraphQL API, Swagger/OpenAPI
- **Authentication**: OIDC (SELISE Blocks identity)
- **Data**: GraphQL queries/mutations, S3 file uploads
- **CSS**: Tailwind CSS v4 (standalone CLI via MSBuild, source in `src/Server/wwwroot/app.tailwind.css`)
- **Deployment**: Docker (worker service)

---

## Render Mode — Interactive Auto (Per-Page)

This app uses **Interactive Auto with per-page rendering**. Follow these rules strictly:

### Rules

1. **Every `@page` component in `src/Client/Pages/`** MUST declare `@rendermode InteractiveAuto` at the top (line 2, after `@page`).
2. **Never set a global render mode** on `<Routes />` in `App.razor` or `Routes.razor` — the Router and MainLayout stay SSR.
3. **Non-page components** (child components, shared UI) should NOT declare `@rendermode` — they inherit from the page that uses them.
4. **Do not use `InteractiveServer` or `InteractiveWebAssembly` alone** unless there is a specific documented reason. Default is always `InteractiveAuto`.
5. **Prerendering** is on by default with `InteractiveAuto`. If a component uses `IJSRuntime` or browser-only APIs, guard calls inside `OnAfterRenderAsync(firstRender)` or disable prerendering with `@rendermode @(new InteractiveAutoRenderMode(prerender: false))`.
6. `HttpContext` is only available during static SSR — never access it in interactive components.

### Project-Level Render Mode Setup (already configured)

- `Server/Program.cs`: `AddInteractiveServerComponents()` + `AddInteractiveWebAssemblyComponents()`
- Endpoint mapping: `AddInteractiveServerRenderMode()` + `AddInteractiveWebAssemblyRenderMode()`
- Client assembly: `AddAdditionalAssemblies(typeof(Client._Imports).Assembly)`

---

## Architecture Conventions

### Folder Structure

```
src/
├── Client/
│   ├── Components/
│   │   ├── Shared/          ← reusable UI (ThemeToggle, LoadingSpinner, PageHeader, etc.)
│   │   └── Forms/           ← form-specific components
│   └── Pages/
│       └── {Feature}/       ← one folder per feature, e.g. Auth/, Dashboard/
│           └── {Feature}Page.razor
├── Server/
│   ├── Layout/              ← App.razor, MainLayout, Routes, ReconnectModal (SSR only)
│   ├── Controllers/         ← [ApiController] REST endpoints
│   └── Extensions/          ← DI registration (ServiceExtensions.cs)
├── Services/
│   └── {Feature}/           ← one folder per feature, e.g. SalesOrders/
│       ├── I{Feature}Service.cs
│       ├── {Feature}Service.cs
│       └── {Feature}.cs     ← domain model(s)
├── Test/
│   ├── Pages/               ← bUnit component tests
│   └── Services/            ← xUnit unit tests per feature
└── Worker/
    └── Jobs/                ← one class per background job
```

> **Why no `Components/` in Server?** The Server project only contains SSR shell files (`App.razor`, `MainLayout.razor`, `Routes.razor`, `ReconnectModal.razor`, `NotFound.razor`, `Error.razor`) — all placed directly in `src/Server/Layout/`. There is no `Components/` wrapper and no `Shared/` folder in Server. The `_Imports.razor` for the Server project lives at `src/Server/_Imports.razor` (project root level).

### HttpClient

- **Server project**: Use `IHttpClientFactory` (`builder.Services.AddHttpClient()`). Never register `HttpClient` with `NavigationManager.BaseUri` — it breaks during SSR.
- **Client project**: Register `HttpClient` with `builder.HostEnvironment.BaseAddress` for WASM-side API calls.
- For authenticated API calls, add a `DelegatingHandler` that injects the auth token.

#### SSR-compatible `HttpClient` (required for `InteractiveAuto` prerendering)

Client components that call APIs via `HttpClient` during `OnInitializedAsync` will run on the **server** during SSR prerender. The Client's `HttpClient` (registered with `HostEnvironment.BaseAddress`) does not resolve on the server and will silently fail.

Always register a scoped `HttpClient` in `Server/Program.cs` that uses the current request's host, so API calls work during SSR:

```csharp
// Server/Program.cs
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<HttpClient>(sp =>
{
    var ctx = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
    var baseAddress = ctx is not null
        ? $"{ctx.Request.Scheme}://{ctx.Request.Host}/"
        : "http://localhost/";
    return new HttpClient { BaseAddress = new Uri(baseAddress) };
});
```

This replaces the plain `builder.Services.AddHttpClient()` for the scoped `HttpClient` used by Client components.

### Services Layer (`src/Services/`)

- All shared business logic goes here. Referenced by both Server and Client projects.
- **Organised by feature**, not by type. Each feature gets its own folder containing the interface, implementation, and models together.
  - ✅ `Services/SalesOrders/ISalesOrderService.cs` + `SalesOrderService.cs` + `SalesOrder.cs`
  - ❌ `Services/Interfaces/` + `Services/Models/` (flat type-based layout — do not use)
- Namespace follows the folder: `Services.SalesOrders`, `Services.Invoices`, etc.
- **DI registration lives in `Server/Extensions/ServiceExtensions.cs`** (not in the Services library itself — keeps it framework-agnostic).
  - Add new registrations to `AddApplicationServices()`:
    ```csharp
    // Server/Extensions/ServiceExtensions.cs
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, string webRootPath)
    {
        services.AddScoped<ISalesOrderService>(_ => new SalesOrderService(webRootPath));
        // add more registrations here
        return services;
    }
    ```

### Test Project (`src/Test/`)

- References both `Client` and `Services` projects.
- Use xUnit + bUnit for Blazor component tests.
- **Mirror the feature structure**: `Test/Services/SalesOrderServiceTests.cs` tests `Services/SalesOrders/`.
- Every test must have at least one assertion — no empty test methods.

### Error Handling

- Wrap `@Body` in `<ErrorBoundary>` in `MainLayout.razor` (already done).
- Never expose `ex.Message` or stack traces to the user. Show generic messages; log details server-side.
- Use `[PersistentState]` on component properties to survive SSR → WASM handoff when needed.

---

## Component Placement Rules — Client vs Server

**All interactive UI components MUST live in `src/Client/`**, never in `src/Server/`.

This is required because `InteractiveAutoRenderMode` needs the component to exist in the WASM (Client) assembly. If a component with `@rendermode InteractiveAuto` is placed in the Server project, it works on the initial Interactive Server circuit but **crashes with an unhandled exception** when Blazor switches to WASM — the component cannot be found in the WASM bundle.

| What | Where |
|------|-------|
| All `@page` components | `src/Client/Pages/` |
| All shared/reusable UI components (ThemeToggle, etc.) | `src/Client/Components/Shared/` |
| Form components | `src/Client/Components/Forms/` |
| SSR shell only (App, MainLayout, Routes, ReconnectModal) | `src/Server/Layout/` |

To use a Client component from a Server layout file (e.g. `MainLayout.razor`), add `@using Client.Components.Shared` to `src/Server/_Imports.razor`. The Server project already references the Client project.

---

## [PersistentState] — SSR → WASM Handoff (.NET 10)

For any `@page` component using `@rendermode InteractiveAuto` that **loads data in `OnInitializedAsync`**, use the `[PersistentState]` attribute on a **nullable public property** to avoid a double API call and loading flash. The framework serializes and restores it automatically — no subscriptions, no `IDisposable`, no manual JSON keys.

### Rules

1. **Declare the persisted data as a nullable public property** decorated with `[PersistentState]`.
2. **Check for `null` in `OnInitializedAsync`** — `null` means SSR hasn't run yet (or failed), so fetch fresh data. Non-null means the value was restored from SSR.
3. **No `IDisposable`, no subscription, no `_fetchSucceeded` flag** — the attribute handles everything.
4. **Do not use the old `PersistentComponentState` service** — it is the low-level API that `[PersistentState]` replaces.

### Pattern

```razor
@code {
    [PersistentState]
    public List<MyModel>? Items { get; set; }

    private List<MyModel> _filteredItems = [];
    private bool _loading = true;

    protected override async Task OnInitializedAsync()
    {
        if (Items is null)
        {
            await LoadDataAsync();
        }
        else
        {
            _loading = false;
            ApplyFilter();
        }
    }

    private async Task LoadDataAsync()
    {
        _loading = true;
        try
        {
            Items = await Http.GetFromJsonAsync<List<MyModel>>("api/my-endpoint") ?? [];
        }
        catch { Items = []; }
        finally
        {
            _loading = false;
            ApplyFilter();
        }
    }
}
```

> Components that do **not** load data in `OnInitializedAsync` (e.g. pure UI, forms) do not need `[PersistentState]`.  
> Components with `@rendermode @(new InteractiveAutoRenderMode(prerender: false))` do not need it either — they never prerender.  
> For complex scenarios (fine-grained control over when/what is persisted), the low-level `PersistentComponentState` service is still available but should be a last resort.

---

## Security Checklist

When generating any code, verify:

- [ ] No secrets or tokens hardcoded — use `appsettings.json`, user secrets, or env vars
- [ ] No `ex.Message` or exception details leaked to UI — use generic error messages
- [ ] Forms use `<EditForm>` with `DataAnnotationsValidator` (not raw `<form>`) for validation + antiforgery
- [ ] Authentication middleware (`AddAuthentication`, `UseAuthentication`, `UseAuthorization`) is present before any `[Authorize]` pages
- [ ] Input is validated at system boundaries; output is HTML-encoded by default in Blazor (no `MarkupString` with user input)
- [ ] CORS is configured explicitly if APIs are consumed cross-origin

---

## Tailwind CSS v4 — Only CSS Approach

**Tailwind CSS is the only styling method in this project. Do not use any alternatives.**

### Strict Rules

1. **Use Tailwind utility classes directly in `.razor` markup** — e.g., `<div class="flex items-center gap-4 p-6 bg-white rounded-lg shadow">`
2. **Do NOT create `.razor.css` scoped CSS files** — all styling must be done via Tailwind classes
3. **Do NOT use inline `style="..."` attributes** — use Tailwind utilities instead
4. **Do NOT add other CSS frameworks** (Bootstrap, MudBlazor, Fluent UI, etc.)
5. **Do NOT write custom CSS classes** unless absolutely unavoidable — prefer `@apply` in the Tailwind source file if needed
6. **Delete any existing `.razor.css` files** when encountered — migrate styles to Tailwind classes

### Setup

- **Source file**: `src/Server/wwwroot/app.tailwind.css`
- **Output**: `src/Server/wwwroot/app.tailwind.css` (compiled in-place by MSBuild target `BuildTailwindCSS`)
- **Config**: Tailwind v4 uses CSS-based config (`@theme` block in the source file), not `tailwind.config.js`
- **Dev watch**: Run `npm run css:watch` for live rebuilds

### Tailwind Class Guidelines

- **Layout**: `flex`, `grid`, `container`, `mx-auto`, `gap-*`
- **Spacing**: `p-*`, `m-*`, `px-*`, `py-*`
- **Typography**: `text-sm`, `font-semibold`, `text-gray-700`, `leading-*`
- **Colors**: Use theme colors via `@theme` block; avoid hardcoded hex in markup
- **Responsive**: `sm:`, `md:`, `lg:`, `xl:` prefixes
- **Dark mode**: `dark:` prefix when needed
- **States**: `hover:`, `focus:`, `disabled:`, `group-hover:`

---

## API Controllers

- All REST endpoints live in `Server/Controllers/` using `[ApiController]` + `ControllerBase`.
- Routes follow kebab-case: `[Route("api/sales-orders")]`.
- Use constructor injection (primary constructor syntax) for dependencies.
- Return `ActionResult<T>` — use `Ok()`, `NotFound()`, `BadRequest()` helpers.
- Controllers are registered via `builder.Services.AddControllers()` and `app.MapControllers()` in `Server/Program.cs`.

```csharp
[ApiController]
[Route("api/sales-orders")]
public class SalesOrdersController(ISalesOrderService salesOrderService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SalesOrder>>> GetAll() =>
        Ok(await salesOrderService.GetAllAsync());
}
```

---

## Swagger / OpenAPI

- Available at `/swagger` in Development environment only
- Endpoints are exposed via `[ApiController]` classes in `Server/Controllers/`
- Swagger is registered with `builder.Services.AddSwaggerGen()` and served via `app.UseSwaggerUI()`

---

## File & Naming Conventions

| Item | Convention | Example |
|------|-----------|---------|
| Pages | PascalCase folder + `*Page.razor` | `Pages/Dashboard/DashboardPage.razor` |
| Components | PascalCase under `Client/Components/Shared/` or `Client/Components/Forms/` | `Components/Shared/ThemeToggle.razor` |
| Server layout | Flat in `Server/Layout/` (no `Components/` wrapper) | `Server/Layout/MainLayout.razor` |
| Services | Feature folder with interface + impl + model | `Services/SalesOrders/ISalesOrderService.cs` |
| Controllers | `{Feature}Controller.cs` in `Server/Controllers/` | `Controllers/SalesOrdersController.cs` |
| DI registration | `Server/Extensions/ServiceExtensions.cs` | `AddApplicationServices()` |
| CSS | Tailwind utility classes in markup only — no `.razor.css`, no inline styles | |
| API routes | `/api/kebab-case` | `/api/sales-orders` |
| Test files | Mirror feature path under `Test/Services/` or `Test/Pages/` | `Test/Services/SalesOrderServiceTests.cs` |

---

## Common Tasks

| Task | Related Skill / Workflow |
|------|----------|
| Create new data schema | `data-management` skill |
| Add login / MFA / SSO | `identity-access` skill |
| Send notifications / emails | `communication` skill |
| Query data via GraphQL | `data-management` skill |
| Set up AI agents / LLMs | `ai-services` skill |
| Configure translations | `localization` skill |
| View logs / traces | `lmt` skill |

See `.claude/skills/` for detailed workflows.

---

## Quick Reference — New Page Checklist

When creating a new page in `src/Client/Pages/`:

1. Add `@page "/route"` directive
2. Add `@rendermode InteractiveAuto` on line 2
3. Use Tailwind CSS utility classes for all styling (no `.razor.css`, no inline styles)
4. Use `<EditForm>` (not `<form>`) for any forms
5. Inject services via `@inject` — never `new` up services
6. If the page loads data in `OnInitializedAsync`, use `[PersistentState]` on a nullable public property (see pattern above) — check for `null` to distinguish first SSR render from WASM restore
7. Add a corresponding test in `src/Test/Pages/` or `src/Test/Services/`

## Quick Reference — New API Endpoint Checklist

When adding a new API feature:

1. Create `Services/{Feature}/` with `I{Feature}Service.cs`, `{Feature}Service.cs`, and model(s)
2. Use namespace `Services.{Feature}`
3. Register in `Server/Extensions/ServiceExtensions.cs` → `AddApplicationServices()`
4. Create `Server/Controllers/{Feature}Controller.cs` with `[ApiController]` + `[Route("api/{feature}")]`
5. Add tests in `Test/Services/{Feature}ServiceTests.cs`

---

## Additional Resources

- CLAUDE.md — On-session setup and prerequisites
- PROJECT.md — Auto-generated project context (login methods, roles, schemas)
- `.claude/skills/` — Domain-specific workflows and actions
