using NG.MicroERP.Shared.Models;

using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Dispatching;
using System.Text.Json;
using NG.MicroERP.Shared.Helper;
using MudBlazor;

namespace NG.MicroERP.App.Pages;

public partial class OrderPage : ContentPage
{
    private RestaurantTablesModel _selectedTable;
    private ObservableCollection<CategoriesModel> _categories=new ObservableCollection<CategoriesModel>();
    private ObservableCollection<ItemsModel> _menuItems= new ObservableCollection<ItemsModel>();
    private ObservableCollection<ItemsModel> _cart = new ObservableCollection<ItemsModel>();
    private List<ServingSizeModel> servingSizes = new List<ServingSizeModel>();
    private readonly string _serviceType;

    public OrderPage(RestaurantTablesModel table, string serviceType)
    {
        InitializeComponent();

        _selectedTable = table;
        _serviceType = serviceType;

        TableNameLabel.Text = $"{_selectedTable.TableNumber}";
        lblCount.Text = "0";
        lblServiceType.Text = _serviceType;

        _menuItems = new ObservableCollection<ItemsModel>();
        ItemsCollectionView.ItemsSource = _menuItems;
        BindingContext = this;

        LoadCategories();
    }

    private async void LoadCategories()
    {
        var fetchedCategories = await Functions.GetAsync<List<CategoriesModel>>("Categories/Search", true) ?? new List<CategoriesModel>();

        _categories.Clear();
        foreach (var category in fetchedCategories)
        {
            _categories.Add(category);
        }

        CategoryButtonsPanel.Children.Clear();

        foreach (var category in _categories)
        {
            var button = new Button
            {
                Text = category.Name,
                BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#196CD1"),
                TextColor = Microsoft.Maui.Graphics.Colors.White,
                HeightRequest=30,
                CornerRadius=2,
                CommandParameter = category
            };
            button.Clicked += OnCategoryClicked;
            CategoryButtonsPanel.Children.Add(button);
        }

        if (_categories.Count > 0)
        {
            LoadItemsByCategory(_categories[0]);
        }
    }

    private void OnCategoryClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is CategoriesModel category)
        {
            LoadItemsByCategory(category);
        }
    }

    private async void OnAddItemClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is ItemsModel item)
        {
            var itemInCart = new ItemsModel
            {
                Id = item.Id,
                Name = item.Name,
                //Description = item.Description,
                MaxQty = item.MaxQty,   //It Takes Order Qty
                Unit = item.Unit,
                RetailPrice = item.RetailPrice,
                ServingSize=item.ServingSize,
                MinQty = item.MinQty  //It takes TakeAway value
            };

            _cart.Add(itemInCart);
            lblCount.Text = _cart.Count.ToString("#,##0");

            await DisplayAlert("Added to Cart", $"{item.MaxQty} x {item.Name}", "OK");
        }
    }

    private async void OnViewCartClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CartPage(_cart, _selectedTable, _serviceType));
    }

    private void OnIncreaseQty(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is ItemsModel item)
        {
            item.MaxQty++;
            UpdateItemPrice(item);
            ItemsCollectionView.ItemsSource = null;
            ItemsCollectionView.ItemsSource = _menuItems;
        }
    }

    private void OnDecreaseQty(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is ItemsModel item)
        {
            if (item.MaxQty > 1)
            {
                item.MaxQty--;
                UpdateItemPrice(item);
                ItemsCollectionView.ItemsSource = null;
                ItemsCollectionView.ItemsSource = _menuItems;
            }
        }
    }

    private void OnSwitchToggled(object sender, ToggledEventArgs e)
    {
        if (sender is Switch switchControl && switchControl.BindingContext is ItemsModel order)
        {
            order.MinQty = e.Value == true? 1: 0;
        }
    }

    private void OnServingSizeChanged(object sender, EventArgs e)
    {

        if (sender is Picker picker && picker.BindingContext is ItemsModel item)
        {
            item.ServingSize = picker.SelectedItem?.ToString() ?? "Medium";
            UpdateItemPrice(item);
            ItemsCollectionView.ItemsSource = null;
            ItemsCollectionView.ItemsSource = _menuItems;
        }
    }

    private void UpdateItemPrice(ItemsModel item)
    {
        if (item.ServingSizes == null || item.ServingSizes.Count == 0)
            return;

        // Find the selected serving size by matching the Description
        var selectedServing = item.ServingSizes.FirstOrDefault(s => s.Size == item.ServingSize);

        if (selectedServing != null)
        {
            item.RetailPrice = Convert.ToDouble(selectedServing.Price) * item.MaxQty;
        }
        else
        {
            item.RetailPrice = item.RetailPrice * item.MaxQty;
        }
    }

    private async void LoadItemsByCategory(CategoriesModel category)
    {
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;

        _menuItems.Clear();

        var fetchedItems = await Functions.GetAsync<List<ItemsModel>>($"Items/Search/CategoriesId={category.Id}", true) ?? new List<ItemsModel>();

        foreach (var item in fetchedItems)
        {
            if (item.ServingSize != null)
            {
                servingSizes = JsonSerializer.Deserialize<List<ServingSizeModel>>(item.ServingSize)!;

                ItemsModel items = new ItemsModel();
                items.Id = item.Id;
                items.Name = item.Name;
                items.Description = "";
                items.MaxQty = 1;
                items.RetailPrice = item.RetailPrice;
                items.ServingSizes = servingSizes;
                items.ServingSize = servingSizes.FirstOrDefault()?.Size ?? "Medium";

                _menuItems.Add(items);
            }
        }


        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;
    }

    private void OnServingSizeButtonClicked(object sender, EventArgs e)
    {
       if (sender is Button btn && btn.CommandParameter is ServingSizeModel selectedSize)
        {
            var item = (btn.Parent?.Parent?.Parent as VisualElement)?.BindingContext as ItemsModel;
            if (item != null)
            {
                item.ServingSize = selectedSize.Size;
                UpdateItemPrice(item);
                RefreshItems(); 
            }
        }
    }

    private void RefreshItems()
    {
        ItemsCollectionView.ItemsSource = null;
        ItemsCollectionView.ItemsSource = _menuItems;
    }
}