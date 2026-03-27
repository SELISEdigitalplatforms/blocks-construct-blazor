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
│   │   ├── Shared/          ← reusable UI (LoadingSpinner, PageHeader, etc.)
│   │   └── Forms/           ← form-specific components
│   └── Pages/
│       └── {Feature}/       ← one folder per feature, e.g. Auth/, Dashboard/
│           └── {Feature}Page.razor
├── Server/
│   ├── Components/Layout/   ← App.razor, MainLayout, Routes (SSR only)
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

### HttpClient

- **Server project**: Use `IHttpClientFactory` (`builder.Services.AddHttpClient()`). Never register `HttpClient` with `NavigationManager.BaseUri` — it breaks during SSR.
- **Client project**: Register `HttpClient` with `builder.HostEnvironment.BaseAddress` for WASM-side API calls.
- For authenticated API calls, add a `DelegatingHandler` that injects the auth token.

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
- Use `PersistentComponentState` to survive SSR → WASM handoff when needed.

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
| Components | PascalCase under `Shared/` or `Forms/` | `Components/Shared/LoadingSpinner.razor` |
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
6. Add a corresponding test in `src/Test/Pages/` or `src/Test/Services/`

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
