using System.Net.Http.Json;
using Client.Models.IAM;
using Client.Models.Inventory;

namespace Client.Services;

public interface IInventoryService
{
    Task<PagedResult<InventoryItem>> GetItemsAsync(int page, int pageSize, string? search = null, string? category = null, string? status = null);
    Task<InventoryItem?> GetItemAsync(string id);
    Task<string> CreateItemAsync(InventoryFormModel model);
    Task UpdateItemAsync(string id, InventoryFormModel model);
    Task DeleteItemAsync(string id);
}

public class InventoryService(HttpClient http) : IInventoryService
{
    public async Task<PagedResult<InventoryItem>> GetItemsAsync(int page, int pageSize, string? search = null, string? category = null, string? status = null)
    {
        var url = $"/api/inventory?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(search)) url += $"&search={Uri.EscapeDataString(search)}";
        if (!string.IsNullOrEmpty(category)) url += $"&category={Uri.EscapeDataString(category)}";
        if (!string.IsNullOrEmpty(status)) url += $"&status={Uri.EscapeDataString(status)}";
        return await http.GetFromJsonAsync<PagedResult<InventoryItem>>(url) ?? new PagedResult<InventoryItem>();
    }

    public async Task<InventoryItem?> GetItemAsync(string id) => await http.GetFromJsonAsync<InventoryItem>($"/api/inventory/{id}");

    public async Task<string> CreateItemAsync(InventoryFormModel model)
    {
        var response = await http.PostAsJsonAsync("/api/inventory", model);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        return result?.GetValueOrDefault("itemId") ?? "";
    }

    public async Task UpdateItemAsync(string id, InventoryFormModel model)
    {
        var response = await http.PutAsJsonAsync($"/api/inventory/{id}", model);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteItemAsync(string id)
    {
        var response = await http.DeleteAsync($"/api/inventory/{id}");
        response.EnsureSuccessStatusCode();
    }
}
