using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class LocationsModel
{
    public int Id { get; set; } = 0;
    public int OrganizationId { get; set; } = 0;
    public string? Code { get; set; } = string.Empty;
    public string? Name { get; set; } = string.Empty;
    public string? Address { get; set; } = string.Empty;
    public string? PocName { get; set; } = string.Empty;
    public string? PocEmail { get; set; } = string.Empty;
    public string? PocPhone { get; set; } = string.Empty;
    public string? LocationType { get; set; } = string.Empty;
    public string? Latitude { get; set; } = string.Empty;
    public string? Longitude { get; set; } = string.Empty;
    public int Radius { get; set; } = 0;
    public int IsActive { get; set; } = 0;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;

}