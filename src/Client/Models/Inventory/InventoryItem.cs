using System.ComponentModel.DataAnnotations;

namespace Client.Models.Inventory;

public class InventoryItem
{
    public string ItemId { get; set; } = "";
    public string ItemName { get; set; } = "";
    public string Category { get; set; } = "";
    public string Supplier { get; set; } = "";
    public string ItemLoc { get; set; } = "";
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Status { get; set; } = "Active";
    public List<string> Tags { get; set; } = [];
    public string? ItemImageFileId { get; set; }
    public bool EligibleWarranty { get; set; }
    public bool EligibleReplacement { get; set; }
    public bool Discount { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastUpdatedDate { get; set; }
}

public class InventoryFormModel
{
    [Required] public string ItemName { get; set; } = "";
    [Required] public string Category { get; set; } = "";
    public string Supplier { get; set; } = "";
    public string ItemLoc { get; set; } = "";
    [Range(0, double.MaxValue)] public decimal Price { get; set; }
    [Range(0, int.MaxValue)] public int Stock { get; set; }
    public string Status { get; set; } = "Active";
    public bool EligibleWarranty { get; set; }
    public bool EligibleReplacement { get; set; }
    public bool Discount { get; set; }
}
