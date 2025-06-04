using NG.MicroERP.Shared.Models;

using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Dispatching;
using System.Text.Json;
using NG.MicroERP.Shared.Helper;
using MudBlazor;

namespace NG.MicroERP.SwiftServe.DineIn.Pages;

public partial class OrderPage : ContentPage
{
    private RestaurantTablesModel _selectedTable;
    private ObservableCollection<CategoriesModel> _categories=new ObservableCollection<CategoriesModel>();
    private ObservableCollection<ItemsModel> _menuItems= new ObservableCollection<ItemsModel>();
    private ObservableCollection<ItemsModel> _cart = new ObservableCollection<ItemsModel>();
    private List<ServingSizeModel> servingSizes = new List<ServingSizeModel>();

    public OrderPage(RestaurantTablesModel table)
    {
        InitializeComponent();

        _selectedTable = table;
        TableNameLabel.Text = $"Table: {_selectedTable.TableNumber}";
        TableStatusLabel.Text = $"Status: {_selectedTable.AvailableStatus}";

        _menuItems = new ObservableCollection<ItemsModel>();
        ItemsCollectionView.ItemsSource = _menuItems;
        BindingContext = this;

        LoadCategories();
    }

    private async void LoadCategories()
    {
        var fetchedCategories = await Config.GetAsync<List<CategoriesModel>>("Categories/Search", true) ?? new List<CategoriesModel>();

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
                BackgroundColor = Microsoft.Maui.Graphics.Colors.LightGray,
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
                Description = item.Description,
                MaxQty = item.MaxQty,
                Unit = item.Unit,
                RetailPrice = item.RetailPrice,
                ServingSize=item.ServingSize,
            };

            _cart.Add(itemInCart);

            await DisplayAlert("Added to Cart", $"{item.MaxQty} x {item.Name}", "OK");
        }
    }

    private async void OnViewCartClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CartPage(_cart.ToList()));
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

    private void OnServingSizeChanged(object sender, EventArgs e)
    {
        if (sender is Picker picker && picker.BindingContext is ItemsModel item)
        {
            item.Description = picker.SelectedItem?.ToString() ?? "Medium";
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
        var selectedServing = item.ServingSizes.FirstOrDefault(s => s.Size == item.Description);

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

        var fetchedItems = await Config.GetAsync<List<ItemsModel>>($"Items/Search/CategoriesId={category.Id}", true) ?? new List<ItemsModel>();

        foreach (var item in fetchedItems)
        {
            servingSizes = JsonSerializer.Deserialize<List<ServingSizeModel>>(item.ServingSize)!;

            _menuItems.Add(new ItemsModel
            {
                Id=item.Id,
                Name = item.Name,
               // Description = item.ServingSize,
                MaxQty = 1,
                RetailPrice = item.RetailPrice,
                ServingSizes = servingSizes 
            });
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
                item.Description = selectedSize.Size;
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