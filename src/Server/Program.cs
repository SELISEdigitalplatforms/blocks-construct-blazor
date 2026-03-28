using Blazored.LocalStorage;
using Client.Services;
using Client.State;
using Microsoft.AspNetCore.Components.Authorization;
using Server.Components.Layout;
using Server.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, AppAuthStateProvider>();
builder.Services.AddScoped<AppAuthStateProvider>();
builder.Services.AddSingleton<SidebarState>();

builder.Services.AddHttpClient();

var apiBase = builder.Configuration["ApiBaseUrl"]
    ?? builder.Configuration["ApiClient:BaseUrl"]
    ?? "https://localhost:7001";
var xBlocksKey = builder.Configuration["ApiSecurity:XBlocksKey"]
    ?? builder.Configuration["ApiClient:XBlocksKey"];

builder.Services.AddHttpClient<IAuthService, AuthService>(ConfigureBlocksApiClient);
builder.Services.AddHttpClient<IUserService, UserService>(ConfigureBlocksApiClient);
builder.Services.AddHttpClient<IInventoryService, InventoryService>(ConfigureBlocksApiClient);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationServices(builder.Environment.WebRootPath);

var app = builder.Build();

// Configure the HTTP request pipeline.
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

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAntiforgery();

app.MapControllers();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

app.Run();

void ConfigureBlocksApiClient(HttpClient httpClient)
{
    httpClient.BaseAddress = new Uri(apiBase);
    if (!string.IsNullOrWhiteSpace(xBlocksKey))
    {
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-blocks-key", xBlocksKey);
    }
}
