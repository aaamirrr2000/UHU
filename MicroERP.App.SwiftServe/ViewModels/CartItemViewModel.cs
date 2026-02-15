using System.ComponentModel;
using System.Runtime.CompilerServices;
using MicroERP.Shared.Models;

namespace MicroERP.App.SwiftServe.ViewModels;

/// <summary>
/// Wraps ItemsModel for Cart page so +/- updates the label without removing/re-inserting (no flicker).
/// </summary>
public class CartItemViewModel : INotifyPropertyChanged
{
    private readonly ItemsModel _item;

    public CartItemViewModel(ItemsModel item)
    {
        _item = item ?? new ItemsModel();
    }

    public ItemsModel Item => _item;

    public int Id => _item.Id;
    public string Name => _item.Name ?? "";
    public string Unit => _item.Unit ?? "";
    public string? ServingSize { get => _item.ServingSize; set => _item.ServingSize = value; }
    public int Person { get => _item.Person; set => _item.Person = value; }

    public string? Description
    {
        get => _item.Description;
        set { _item.Description = value; OnPropertyChanged(); }
    }

    public double MaxQty
    {
        get => _item.MaxQty;
        set
        {
            if (Math.Abs(_item.MaxQty - value) < 0.01) return;
            _item.MaxQty = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(RetailPrice));
        }
    }

    public double RetailPrice
    {
        get => _item.RetailPrice;
        set
        {
            _item.RetailPrice = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(UnitPrice));
        }
    }

    /// <summary>Unit price for display (line total ÷ qty).</summary>
    public double UnitPrice => MaxQty > 0 ? RetailPrice / MaxQty : RetailPrice;

    public void NotifyAllProperties()
    {
        OnPropertyChanged(nameof(MaxQty));
        OnPropertyChanged(nameof(RetailPrice));
        OnPropertyChanged(nameof(UnitPrice));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
