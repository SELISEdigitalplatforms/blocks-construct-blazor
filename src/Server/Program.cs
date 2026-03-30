using Blocks.Genesis;
using Blazored.LocalStorage;
using Client.Services;
using Client.State;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Server.Layout;
using Server.Extensions;
using System.Threading.RateLimiting;

#region Bootstrap
const string ServiceName = "blocks-construct-blazor-server";

await ApplicationConfigurations.ConfigureLogAndSecretsAsync(ServiceName, VaultType.Azure);

var builder = WebApplication.CreateBuilder(args);

// Blocks: loads API/environment configuration conventions used by the platform.
ApplicationConfigurations.ConfigureApiEnv(builder, args);
#endregion

#region Configuration Values
// Read runtime values once and reuse for server-side and client bootstrap config.
var apiBase = GetRequiredConfigurationValue(
    builder.Configuration,
    "MICROSERVICE_API_BASE_URL",
    "MicroserviceApiBaseUrl");

if (!Uri.TryCreate(apiBase, UriKind.Absolute, out var apiBaseUri))
{
    throw new InvalidOperationException("Missing or invalid API base URL. Configure MICROSERVICE_API_BASE_URL or MicroserviceApiBaseUrl.");
}

var xBlocksKey = GetRequiredConfigurationValue(
    builder.Configuration,
    "X_BLOCKS_KEY",
    "XBlocksKey");

var projectSlug = GetRequiredConfigurationValue(
    builder.Configuration,
    "PROJECT_SLUG",
    "ProjectSlug");
#endregion

#region Service Registration
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddBlazoredLocalStorage();
builder.Services
    .AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, AppAuthStateProvider>();
builder.Services.AddScoped<AppAuthStateProvider>();
builder.Services.AddScoped<SidebarState>();
builder.Services.AddScoped<LanguageState>();
builder.Services.AddTransient<AuthTokenHandler>();

builder.Services.AddHttpClient();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("api-per-ip", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 120,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 20,
                AutoReplenishment = true
            }));
});

builder.Services.AddSingleton(new RuntimeClientConfig
{
    MicroserviceApiBaseUrl = apiBaseUri.ToString(),
    XBlocksKey = xBlocksKey,
    ProjectSlug = projectSlug
});

builder.Services.AddHttpClient<IAuthService, AuthService>(ConfigureBlocksApiClient)
    .AddHttpMessageHandler<AuthTokenHandler>();
builder.Services.AddHttpClient<IUserService, UserService>(ConfigureBlocksApiClient)
    .AddHttpMessageHandler<AuthTokenHandler>();
builder.Services.AddHttpClient<IDeviceService, DeviceService>(ConfigureBlocksApiClient)
    .AddHttpMessageHandler<AuthTokenHandler>();
builder.Services.AddHttpClient<IInventoryService, InventoryService>(ConfigureBlocksApiClient)
    .AddHttpMessageHandler<AuthTokenHandler>();
builder.Services.AddHttpClient<ILanguageService, LanguageService>(ConfigureBlocksApiClient);
builder.Services.AddHttpClient<ISsoService, SsoService>(ConfigureBlocksApiClient);

// Blocks: registers platform services (auth/token plumbing, messaging, etc.).
ApplicationConfigurations.ConfigureServices(builder.Services, new MessageConfiguration
{
    // rabbit settings
});

// Blocks: registers platform API dependencies.
ApplicationConfigurations.ConfigureApi(builder.Services);

// App: feature service registrations delegated to local extension methods.
builder.Services.AddApplicationServices(builder.Environment.WebRootPath);
#endregion

#region Build App
var app = builder.Build();
#endregion

#region Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseCors(policy => policy
    .AllowAnyHeader()
    .AllowAnyMethod()
    .SetIsOriginAllowed(_ => true)
    .AllowCredentials()
    .SetPreflightMaxAge(TimeSpan.FromDays(365)));
app.UseRateLimiter();
app.UseRouting();

app.UseWhen(
    ctx => ctx.Request.Path.StartsWithSegments("/api"),
    apiPipeline =>
    {
        // Blocks: apply API-only tenant/auth middleware for the /api branch in Blazor-hosted apps.
        ApplicationConfigurations.ConfigureApiBranchMiddleware(apiPipeline);
    });

app.MapControllers().RequireRateLimiting("api-per-ip");

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAntiforgery();
#endregion

#region Endpoints
app.MapGet("/client-config", () => Results.Json(new
{
    MicroserviceApiBaseUrl = apiBase,
    XBlocksKey = xBlocksKey,
    ProjectSlug = projectSlug
}));

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);
#endregion

#region Run
await app.RunAsync();
#endregion

#region Local Helpers
void ConfigureBlocksApiClient(HttpClient httpClient)
{
    httpClient.BaseAddress = apiBaseUri;
    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-blocks-key", xBlocksKey);
}

static string GetRequiredConfigurationValue(IConfiguration configuration, params string[] keys)
{
    foreach (var key in keys)
    {
        var value = configuration[key];
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }
    }

    throw new InvalidOperationException($"Missing required configuration. Set one of: {string.Join(", ", keys)}.");
}
#endregion
