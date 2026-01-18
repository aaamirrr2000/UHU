namespace NG.MicroERP.Shared.Models;

public class ServiceChargeInfo
{
    public string ChargeType { get; set; } = "PERCENTAGE"; // PERCENTAGE or FLAT
    public double Amount { get; set; } = 0;
}
