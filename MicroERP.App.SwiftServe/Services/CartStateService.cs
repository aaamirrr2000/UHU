using MicroERP.Shared.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroERP.App.SwiftServe.Services;

public class CartStateService
{
    public ObservableCollection<ItemsModel> CartItems { get; set; } = new();
    public RestaurantTablesModel SelectedTable { get; set; }
    public string ServiceType { get; set; }
    public string BillNote { get; set; } = string.Empty;

    public void Clear()
    {
        CartItems.Clear();
        SelectedTable = null;
        ServiceType = null;
        BillNote = string.Empty;
    }
}
