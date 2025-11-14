using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NG.MicroERP.Shared.Models;

public class FbrSubmissionModel
{
    public int Id { get; set; } = 0;
    public int BillId { get; set; } = 0;
    public string? JsonPayload { get; set; } = string.Empty;
    public DateTime SubmissionDateTime { get; set; } = DateTime.Today;
    public string? ResponseCode { get; set; } = string.Empty;
    public string? IRN { get; set; } = string.Empty;
    public string? DigitalInvoiceUrl { get; set; } = string.Empty;
    public string? QRCodeData { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; } = string.Empty;
    public int RetryCount { get; set; } = 0;
    public string? SubmissionMachineInfo { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; } = DateTime.Today;

}