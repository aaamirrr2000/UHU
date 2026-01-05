using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class TaxMasterModel
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int AccountId { get; set; }
    public string? TaxName { get; set; }
    public string? TaxType { get; set; }
    public string? TaxBaseType { get; set; }
    public double Rate { get; set; }
    public string? Description { get; set; }
    public int IsActive { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string? CreatedFrom { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedOn { get; set; }
    public string? UpdatedFrom { get; set; }
    public int IsSoftDeleted { get; set; }

}