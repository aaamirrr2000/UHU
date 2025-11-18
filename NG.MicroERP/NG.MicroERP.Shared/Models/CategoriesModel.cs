using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class CategoriesModel
{
    public int Id { get; set; } = 0;
    public int OrganizationId { get; set; } = 0;
    public string? Code { get; set; } = string.Empty;
    public string? Name { get; set; } = string.Empty;
    public string? CategoryType { get; set; } = string.Empty;
    public int ParentId { get; set; } = 0;
    public int IsActive { get; set; } = 0;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public string? CreatedFrom { get; set; } = string.Empty;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public string? UpdatedFrom { get; set; } = string.Empty;
    public int IsSoftDeleted { get; set; } = 0;
    public double ItemCount { get; set; } = 0;

}