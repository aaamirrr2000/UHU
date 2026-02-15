using System.ComponentModel;
using System.Runtime.CompilerServices;
using MicroERP.Shared.Models;

namespace MicroERP.App.SwiftServe.ViewModels;

/// <summary>
/// Wraps ItemsModel for Order page so +/- updates the label without removing/re-inserting (no flicker).
/// </summary>
public class MenuItemViewModel : INotifyPropertyChanged
{
    private readonly ItemsModel _item;

    public MenuItemViewModel(ItemsModel item)
    {
        _item = item ?? new ItemsModel();
    }

    public ItemsModel Item => _item;

    public int Id => _item.Id;
    public string Name => _item.Name ?? "";
    public string Unit => _item.Unit ?? "";
    public string? Description { get => _item.Description; set => _item.Description = value; }
    public string? ServingSize { get => _item.ServingSize; set => _item.ServingSize = value; }
    public int Person { get => _item.Person; set => _item.Person = value; }
    public List<ServingSizeModel>? ServingSizes => _item.ServingSizes;

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
        }
    }

    public void NotifyAllProperties()
    {
        OnPropertyChanged(nameof(MaxQty));
        OnPropertyChanged(nameof(RetailPrice));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
