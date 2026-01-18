namespace NG.MicroERP.Shared.Models;

public class PhysicalCashCountDenominationEntry
{
    public decimal Denomination { get; set; } = 0;
    public int Quantity { get; set; } = 0;
    
    private decimal _amount = 0;
    public decimal Amount 
    { 
        get => _amount == 0 ? (Denomination * Quantity) : _amount;
        set => _amount = value;
    }
    
    public int? RecordId { get; set; } // To track existing record ID for update/delete
}
