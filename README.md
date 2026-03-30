# Blocks Construct — Blazor WASM

A production-ready enterprise application template built on **SELISE Blocks**, featuring .NET 10, Blazor WASM with Interactive Auto per-page rendering, Tailwind CSS v4, and OIDC authentication. Designed for scalability, maintainability, and strict adherence to software engineering best practices.

## Overview

This is a comprehensive full-stack .NET 10 application showcasing enterprise patterns:

- **Blazor WASM Frontend** — Interactive Auto rendering mode with per-page granularity
- **ASP.NET Core Backend** — Single unified host for UI and REST APIs
- **Shared Services Layer** — Feature-based architecture with dependency injection and separation of concerns
- **Tailwind CSS v4** — Utility-first styling as the only CSS approach (no CSS frameworks or scoped styles)
- **OIDC Authentication** — Integrated with SELISE Blocks identity platform
- **Comprehensive Testing** — Unit tests (xUnit) and component integration tests (bUnit)
- **Worker Service** — Background job processing for async operations
- **Docker & Kubernetes Ready** — Multi-environment containerized deployment support

This template exemplifies production-quality application architecture with federated feature teams, strict architectural conventions, and proven scalability patterns.

---

## Technology Stack

| Layer | Technology | Notes |
|-------|-----------|-------|
| **Frontend** | Blazor WASM (.NET 10) | Interactive Auto rendering with per-page granularity and prerendering |
| **UI Framework** | Tailwind CSS v4 | Utility-first, compiled via MSBuild, no other CSS libraries permitted |
| **Backend** | ASP.NET Core 10 | Unified host for UI and REST APIs with automatic HTTPS |
| **API** | REST (ApiController) + Swagger/OpenAPI | Auto-generated, kebab-case routes, OpenAPI spec in dev mode |
| **Authentication** | OIDC | SELISE Blocks identity integration with token-based auth |
| **Data** | GraphQL + S3 | Platform-native APIs for queries, mutations, and file operations |
| **Testing** | xUnit + bUnit | Unit tests and Blazor component integration testing |
| **Dependency Injection** | .NET Core DI Container | Feature-based service registration in `ServiceExtensions.cs` |
| **Deployment** | Docker, Kubernetes, Cloud | Multi-environment configuration with secrets management |

---

## Project Structure Overview

Feature-based architecture organized by business capability:

```
src/
├── Client/              # Blazor WASM Frontend
├── Server/              # ASP.NET Core Host
├── Services/            # Shared business logic (feature-based)
├── Test/                # Test suite (xUnit + bUnit)
└── Worker/              # Background service
```

**Complete structure with detailed organization** is documented in [Architecture & Conventions](#architecture--conventions) section below.

---

## Quick Start

### 1. Prerequisites

- .NET 10 SDK — [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- Git
- SELISE Blocks credentials (API URL and key)

### 2. Clone & Setup

```bash
git clone https://github.com/SELISEdigitalplatforms/blocks-construct-blazor.git
cd blocks-construct-blazor
dotnet restore
npm install
```

### 3. Configure & Run

**Option A: Using .env file (Recommended)**

Copy the example file to `src/Server/`:
```bash
cp .env.example src/Server/.env
```

Edit `src/Server/.env` with your credentials:
```
MICROSERVICE_API_BASE_URL=https://api.blocks.com
X_BLOCKS_KEY=your-secret-key
PROJECT_SLUG=my-project
```

Load and run:
```bash
# Linux/macOS
cd src/Server && export $(cat .env | xargs) && cd ../..
dotnet watch --project src/Server

# Windows (PowerShell)
Get-Content src/Server/.env | ForEach-Object {
    $key, $value = $_ -split '=',2
    [Environment]::SetEnvironmentVariable($key, $value)
}
dotnet watch --project src/Server
```

**Option B: Command-line arguments**

```bash
dotnet watch --project src/Server -- \
  --MICROSERVICE_API_BASE_URL=https://api.blocks.com \
  --X_BLOCKS_KEY=your-secret-key \
  --PROJECT_SLUG=my-project
```

**Option C: Environment variables**

```bash
export MICROSERVICE_API_BASE_URL=https://api.blocks.com
export X_BLOCKS_KEY=your-secret-key
export PROJECT_SLUG=my-project

dotnet watch --project src/Server
```

**Optional: Watch Tailwind CSS for changes**

In a separate terminal:
```bash
npm run css:watch
```

Access the app at **https://localhost:7075**

---

## Architecture & Conventions

### Rendering: Interactive Auto (Per-Page)

Every page component **must declare** `@rendermode InteractiveAuto`:

```razor
@page "/sales"
@rendermode InteractiveAuto

<h1>Sales Orders</h1>
```

**Rules:**
- Line 2: `@rendermode InteractiveAuto` (after `@page` directive)
- Child components inherit render mode (don't repeat)
- Layout components stay SSR-only (no render mode directive)
- Prerendering enabled by default for SEO + performance

### Naming Conventions

| Item | Pattern | Example |
|------|---------|---------|
| Pages | `{Feature}Page.razor` | `SalesPage.razor`, `InventoryListPage.razor` |
| Components | `{Name}.razor` | `LoadingSpinner.razor`, `PageHeader.razor` |
| Services | `I{Feature}Service.cs`, `{Feature}Service.cs` | `ISalesOrderService.cs`, `SalesOrderService.cs` |
| Models | `{Entity}.cs` | `SalesOrder.cs`, `InventoryItem.cs` |
| Controllers | `{Feature}Controller.cs` | `SalesOrdersController.cs` |
| API Routes | `/api/kebab-case` | `/api/sales-orders`, `/api/inventory-items` |
| Namespaces | `Services.{Feature}` | `Services.SalesOrders` |

### Feature-Based Service Architecture

Services organized by **business domain**, not by technical type:

```
Services/SalesOrders/          # Feature folder
├── ISalesOrderService.cs      # Interface
├── SalesOrderService.cs       # Implementation
└── SalesOrder.cs              # Domain model
```

**Registration**: All services registered in `Server/Extensions/ServiceExtensions.cs`:

```csharp
public static IServiceCollection AddApplicationServices(
    this IServiceCollection services, string webRootPath)
{
    services.AddScoped<ISalesOrderService>(
        _ => new SalesOrderService(webRootPath));
    return services;
}
```

### REST API Controllers

- **Location**: `src/Server/Controllers/`
- **Naming**: `{Feature}Controller.cs`
- **Routes**: `[Route("api/{feature}")]` (kebab-case)
- **DI**: Constructor injection
- **Returns**: `ActionResult<T>`

**Example:**
```csharp
[ApiController]
[Route("api/sales-orders")]
public class SalesOrdersController(ISalesOrderService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SalesOrder>>> GetAll() =>
        Ok(await service.GetAllAsync());
}
```

### Styling: Tailwind CSS v4 Only

**Rules (Strictly Enforced):**

✅ **DO:** Use Tailwind utility classes: `<div class="flex items-center gap-4 p-6 bg-white rounded-lg">`

❌ **DON'T:**
- Create `.razor.css` files
- Write inline `style="..."`
- Use other CSS frameworks

**Source:** [src/Server/wwwroot/app.tailwind.css.src](src/Server/wwwroot/app.tailwind.css.src)

```css
@import "tailwindcss";

@theme {
  --color-primary: #15969B;
  --color-secondary: #5194B8;
}

@layer components {
  .btn-primary {
    @apply px-4 py-2 bg-primary text-white rounded hover:opacity-90;
  }
}
```

**Build:** `dotnet build` (compiled to `app.tailwind.css` via MSBuild)

### Error Handling & Security

**Don't expose exception details:**

❌ **Bad:**
```csharp
catch (Exception ex)
{
    return new Response { Error = ex.Message };
}
```

✅ **Good:**
```csharp
catch (Exception ex)
{
    Logger.LogError(ex, "Failed to retrieve orders");
    return new Response { Error = "An error occurred. Please try again." };
}
```

**Security Checklist:**
- No hardcoded secrets (use environment variables)
- No exception details in API responses
- Forms use `<EditForm>` with `DataAnnotationsValidator`
- Input validated at system boundaries
- Output HTML-encoded by default
- Authentication middleware before `[Authorize]`

---

## Configuration

Configuration is resolved in priority order:

1. Command-line args: `--MICROSERVICE_API_BASE_URL=...`
2. Environment variables: `MICROSERVICE_API_BASE_URL=...`
3. appsettings.{Environment}.json
4. appsettings.json

### Required Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `MICROSERVICE_API_BASE_URL` | SELISE Blocks microservice API endpoint | `https://api.blocks.com` |
| `X_BLOCKS_KEY` | SELISE Blocks API authentication key | `sk_live_abc123def456` |
| `PROJECT_SLUG` | Project slug in SELISE Blocks | `my-project` |

### Configuration Methods

#### Method 1: Command-Line Arguments (Recommended)

```bash
dotnet watch --project src/Server -- \
  --MICROSERVICE_API_BASE_URL=https://api.blocks.com \
  --X_BLOCKS_KEY=your-secret-key \
  --PROJECT_SLUG=my-project
```

#### Method 2: Environment Variables

**Linux/macOS:**
```bash
export MICROSERVICE_API_BASE_URL=https://api.blocks.com
export X_BLOCKS_KEY=your-secret-key
export PROJECT_SLUG=my-project

dotnet watch --project src/Server
```

**Windows (PowerShell):**
```powershell
$env:MICROSERVICE_API_BASE_URL = "https://api.blocks.com"
$env:X_BLOCKS_KEY = "your-secret-key"
$env:PROJECT_SLUG = "my-project"

dotnet watch --project src/Server
```

**Using .env file:**
Copy `.env.example` to `src/Server/.env` and populate:
```bash
cp .env.example src/Server/.env
# Edit src/Server/.env with your values
```

**src/Server/.env contents:**
```
MICROSERVICE_API_BASE_URL=https://api.blocks.com
X_BLOCKS_KEY=your-secret-key
PROJECT_SLUG=my-project
```

Load in your shell:
```bash
cd src/Server && export $(cat .env | xargs) && cd ../..
```

#### Method 3: appsettings.Development.json

Edit `src/Server/appsettings.Development.json`:

```json
{
  "Config": {
    "MicroserviceApiBaseUrl": "https://api.blocks.com",
    "XBlocksKey": "your-secret-key",
    "ProjectSlug": "my-project"
  }
}
```

⚠️ **Never commit secrets** — this method is for local development only.

#### Method 4: User Secrets (Secure Dev)

```bash
# Initialize user secrets (one-time)
dotnet user-secrets init --project src/Server

# Set secrets
dotnet user-secrets set Config:XBlocksKey "your-secret-key" --project src/Server
dotnet user-secrets set Config:MicroserviceApiBaseUrl "https://api.blocks.com" --project src/Server
dotnet user-secrets set Config:ProjectSlug "my-project" --project src/Server

# View secrets
dotnet user-secrets list --project src/Server
```

User secrets stored locally (never in repo): `~/.microsoft/usersecrets/<project-id>/secrets.json`

### Deployment

#### Docker

```bash
docker run \
  -e MICROSERVICE_API_BASE_URL=https://api.blocks.com \
  -e X_BLOCKS_KEY=your-secret-key \
  -e PROJECT_SLUG=my-project \
  -p 8080:8080 \
  blocks-construct
```

#### Docker Compose

```yaml
version: '3.8'
services:
  app:
    image: blocks-construct
    environment:
      MICROSERVICE_API_BASE_URL: ${MICROSERVICE_API_BASE_URL}
      X_BLOCKS_KEY: ${X_BLOCKS_KEY}
      PROJECT_SLUG: ${PROJECT_SLUG}
    ports:
      - "8080:8080"
```

#### Kubernetes

**ConfigMap:**
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: blocks-config
data:
  MICROSERVICE_API_BASE_URL: "https://api.blocks.com"
  PROJECT_SLUG: "my-project"
```

**Secret:**
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: blocks-secret
type: Opaque
stringData:
  X_BLOCKS_KEY: "your-secret-key"
```

**Deployment:**
```yaml
spec:
  containers:
  - name: app
    image: blocks-construct
    env:
    - name: MICROSERVICE_API_BASE_URL
      valueFrom:
        configMapKeyRef:
          name: blocks-config
          key: MICROSERVICE_API_BASE_URL
    - name: X_BLOCKS_KEY
      valueFrom:
        secretKeyRef:
          name: blocks-secret
          key: X_BLOCKS_KEY
    - name: PROJECT_SLUG
      valueFrom:
        configMapKeyRef:
          name: blocks-config
          key: PROJECT_SLUG
```

---

## API Documentation

### Swagger

- **Dev:** https://localhost:7075/swagger
- **Production:** Disabled

### Example: Sales Orders

```
GET    /api/sales-orders         # List all
GET    /api/sales-orders/{id}    # Get one
POST   /api/sales-orders         # Create
```

---

## Testing

```bash
dotnet test
```

**Structure:**
- `Test/Services/` — Unit tests (xUnit)
- `Test/Pages/` — Component tests (bUnit)

**Example xUnit test:**
```csharp
[Fact]
public async Task GetById_WithValidId_ReturnsSalesOrder()
{
    var service = new SalesOrderService();
    var result = await service.GetByIdAsync("ORD-001");
    Assert.NotNull(result);
}
```

---

## Building & Deployment

### Local Development

```bash
dotnet watch --project src/Server
```

### Release Build

```bash
dotnet publish -c Release -o ./publish src/Server
```

### Docker

```bash
docker build -t blocks-construct .
docker run -e MICROSERVICE_API_BASE_URL=<url> -e X_BLOCKS_KEY=<key> -p 8080:8080 blocks-construct
```

### Docker Compose

```yaml
version: '3.8'
services:
  app:
    image: blocks-construct
    environment:
      MICROSERVICE_API_BASE_URL: ${API_URL}
      X_BLOCKS_KEY: ${API_KEY}
    ports:
      - "8080:8080"
```

### Kubernetes

Use ConfigMap for config and Secret for sensitive values:

```yaml
# ConfigMap
apiVersion: v1
kind: ConfigMap
metadata:
  name: blocks-config
data:
  MICROSERVICE_API_BASE_URL: "https://api.blocks.com"

---
# Secret
apiVersion: v1
kind: Secret
metadata:
  name: blocks-secret
type: Opaque
stringData:
  X_BLOCKS_KEY: "your-key"

---
# Deployment
apiVersion: apps/v1
kind: Deployment
metadata:
  name: blocks-construct
spec:
  replicas: 2
  selector:
    matchLabels:
      app: blocks-construct
  template:
    metadata:
      labels:
        app: blocks-construct
    spec:
      containers:
      - name: app
        image: blocks-construct
        env:
        - name: MICROSERVICE_API_BASE_URL
          valueFrom:
            configMapKeyRef:
              name: blocks-config
              key: MICROSERVICE_API_BASE_URL
        - name: X_BLOCKS_KEY
          valueFrom:
            secretKeyRef:
              name: blocks-secret
              key: X_BLOCKS_KEY
```

---

## Creating a New Feature

1. **Create service**: `Services/MyFeature/`
   - `IMyFeatureService.cs`
   - `MyFeatureService.cs`
   - `MyFeature.cs` (model)

2. **Register**: Add to `Server/Extensions/ServiceExtensions.cs`

3. **API**: Create `Server/Controllers/MyFeatureController.cs`

4. **UI**: Create `Client/Pages/MyFeature/MyFeaturePage.razor` with `@rendermode InteractiveAuto`

5. **Tests**:
   - `Test/Services/MyFeatureServiceTests.cs`
   - `Test/Pages/MyFeature/MyFeaturePageTests.cs`

---

## Best Practices

- **One concern per class** — Follow Single Responsibility Principle
- **Inject dependencies** — Never instantiate services with `new`
- **Write tests** — Test-driven development
- **Keep pages simple** — Move logic to services
- **Use Tailwind** — No custom CSS
- **Handle errors** — No exception details to users
- **Log properly** — Full exceptions server-side only

---

## Resources

- **[CLAUDE.md](CLAUDE.md)** — Claude AI assistant instructions
- **[copilot-instructions.md](.github/copilot-instructions.md)** — GitHub Copilot guidelines
- **[ASP.NET Core Docs](https://learn.microsoft.com/aspnet/core/)** — Official docs
- **[Blazor Docs](https://learn.microsoft.com/aspnet/core/blazor/)** — Blazor reference
- **[Tailwind CSS](https://tailwindcss.com/docs)** — Utility class reference
- **[SELISE Blocks](https://blocks.selise.com/docs)** — Platform docs

---

## License

[LICENSE](LICENSE)
