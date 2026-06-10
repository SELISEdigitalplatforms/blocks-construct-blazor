using Blazored.LocalStorage;
using Client.Models.Language;
using Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Client.State;

public sealed class LanguageState(
    ILanguageService languageService,
    NavigationManager nav,
    ILocalStorageService localStorage) : IDisposable
{
    public string CurrentLanguage { get; private set; } = "en-US";
    public List<LanguageItem> AvailableLanguages { get; private set; } = [];
    public bool IsLoading { get; private set; }

    private readonly Dictionary<string, string> _translations = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, HashSet<string>> _moduleCache = new(StringComparer.OrdinalIgnoreCase);
    private List<LanguageModule> _availableModules = [];
    private bool _initialized;
    private Task? _initTask;

    public event Action? OnChange;

    private static readonly Dictionary<string, string[]> RouteModuleMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["/"] = ["common", "auth"],
        ["/login"] = ["common", "auth"],
        ["/forgot-password"] = ["common", "auth"],
        ["/reset-password"] = ["common", "auth"],
        ["/activate"] = ["common", "auth"],
        ["/activation-success"] = ["common", "auth"],
        ["/verify-mfa"] = ["common", "auth", "mfa"],
        ["/email-sent"] = ["common", "auth"],
        ["/identity-management"] = ["common", "iam"],
        ["/inventory"] = ["common", "inventory"],
        ["/profile"] = ["common", "profile", "mfa"]
    };

    private static readonly string[] DefaultModules = ["common"];

    public string this[string key]
        => _translations.TryGetValue(key, out var value) ? value : key;

    public Task InitializeAsync()
    {
        _initTask ??= RunInitializationAsync();
        return _initTask;
    }

    private async Task RunInitializationAsync()
    {
        if (_initialized)
            return;

        IsLoading = true;
        NotifyStateChanged();

        try
        {
            AvailableLanguages = await languageService.GetAvailableLanguagesAsync();
            _availableModules = await languageService.GetModulesAsync();

            var stored = await localStorage.GetItemAsync<string>("language");
            var apiDefault = AvailableLanguages.FirstOrDefault(l => l.IsDefault)?.LanguageCode;

            CurrentLanguage = PickStartingLanguage(stored, apiDefault, AvailableLanguages);

            var path = new Uri(nav.Uri).AbsolutePath;
            await LoadModulesForRouteAsync(CurrentLanguage, path, resetTranslations: true);

            nav.LocationChanged += OnLocationChanged;
            _initialized = true;
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    public async Task SetLanguageAsync(string languageCode, bool isUserAction = true)
    {
        if (string.Equals(languageCode, CurrentLanguage, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        IsLoading = true;
        NotifyStateChanged();

        try
        {
            CurrentLanguage = languageCode;
            await localStorage.SetItemAsync("language", languageCode);

            if (isUserAction)
            {
                await localStorage.SetItemAsync("language_user_selected", "true");
            }

            var path = new Uri(nav.Uri).AbsolutePath;
            await LoadModulesForRouteAsync(languageCode, path, resetTranslations: true);
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    public async Task OnRouteChangedAsync(string path)
    {
        if (!_initialized)
        {
            return;
        }

        var modules = GetModulesForRoute(path);
        if (AreModulesCached(CurrentLanguage, modules))
        {
            return;
        }

        try
        {
            await LoadModulesForRouteAsync(CurrentLanguage, path, resetTranslations: false);

            // Notify once after background route-module load completes.
            NotifyStateChanged();
        }
        catch
        {
            // Never let localization failures break page navigation.
        }
    }

    private async Task LoadModulesForRouteAsync(string language, string path, bool resetTranslations)
    {
        if (resetTranslations)
        {
            _translations.Clear();

            // Translation dictionary is rebuilt from scratch, so invalidate per-language
            // module cache to avoid false cache hits on subsequent route navigation.
            _moduleCache.Remove(language);
        }

        foreach (var moduleName in GetModulesForRoute(path))
        {
            if (IsModuleCached(language, moduleName))
            {
                continue;
            }

            var module = _availableModules.FirstOrDefault(m =>
                string.Equals(m.Name, moduleName, StringComparison.OrdinalIgnoreCase));
            if (module is null || string.IsNullOrWhiteSpace(module.ItemId))
            {
                continue;
            }

            var dict = await languageService.GetTranslationsAsync(language, module.ItemId, module.Name);
            foreach (var kv in dict)
            {
                _translations[kv.Key] = kv.Value;
            }

            CacheModule(language, moduleName);
        }
    }

    private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        try
        {
            var path = new Uri(e.Location).AbsolutePath;
            await OnRouteChangedAsync(path);
        }
        catch
        {
            // Never let localization failures break page navigation.
        }
    }

    private static string[] GetModulesForRoute(string path)
    {
        var baseSegment = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
        var basePath = "/" + baseSegment;

        return RouteModuleMap.TryGetValue(basePath, out var modules)
            ? modules
            : DefaultModules;
    }

    private bool AreModulesCached(string language, string[] modules)
        => modules.All(module => IsModuleCached(language, module));

    private bool IsModuleCached(string language, string module)
        => _moduleCache.TryGetValue(language, out var set) && set.Contains(module);

    private void CacheModule(string language, string module)
    {
        if (!_moduleCache.TryGetValue(language, out var set))
        {
            set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _moduleCache[language] = set;
        }

        set.Add(module);
    }

    private static string PickStartingLanguage(string? stored, string? apiDefault, List<LanguageItem> available)
    {
        if (!string.IsNullOrWhiteSpace(stored) &&
            available.Any(l => string.Equals(l.LanguageCode, stored, StringComparison.OrdinalIgnoreCase)))
        {
            return stored;
        }

        if (!string.IsNullOrWhiteSpace(apiDefault))
        {
            return apiDefault;
        }

        var firstAvailable = available
            .Select(l => l.LanguageCode)
            .FirstOrDefault(code => !string.IsNullOrWhiteSpace(code));
        if (!string.IsNullOrWhiteSpace(firstAvailable))
        {
            return firstAvailable;
        }

        return "en";
    }

    private void NotifyStateChanged() => OnChange?.Invoke();

    public void Dispose()
    {
        nav.LocationChanged -= OnLocationChanged;
    }
}
