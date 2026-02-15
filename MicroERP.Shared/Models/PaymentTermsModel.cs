using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroERP.Shared.Models;

public class PaymentTermsModel
{
    public int Id { get; set; } = 0;
    public string? Code { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public int DaysDue { get; set; } = 0;
    public int IsDefault { get; set; } = 0;
    public int IsActive { get; set; } = 0;
    public int CreatedBy { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public int UpdatedBy { get; set; } = 0;
    public DateTime UpdatedOn { get; set; } = DateTime.Today;
    public int IsSoftDeleted { get; set; } = 0;

}
