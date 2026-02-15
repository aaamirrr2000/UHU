namespace MicroERP.Shared.Models;

public class RestaurantTablesModel
{
    public int Id { get; set; } = 0;
    public int OrganizationId { get; set; } = 0;
    public string? TableNumber { get; set; } = string.Empty;
    public int Capacity { get; set; } = 0;
    public int IsAvailable { get; set; } = 1;
    public string? TableLocation { get; set; } = string.Empty;
    public string? Notes { get; set; } = string.Empty;
    public int IsActive { get; set; } = 1;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;

    // UI Display Fields (not persisted to DB)
    public string? TableImage { get; set; } = string.Empty;
    public string? AvailableStatus { get; set; } = string.Empty;
}

