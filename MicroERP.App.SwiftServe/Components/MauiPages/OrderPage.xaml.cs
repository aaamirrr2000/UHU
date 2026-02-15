using MicroERP.App.SwiftServe.Helper;
using MicroERP.App.SwiftServe.Components.MauiPages.Controls;
using MicroERP.App.SwiftServe.Services;
using MicroERP.App.SwiftServe.ViewModels;
using MicroERP.Shared.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Linq;

namespace MicroERP.App.SwiftServe.Components.MauiPages;

public partial class OrderPage : ContentPage
{
    private ObservableCollection<CategoriesModel> _categories = new();
    private ObservableCollection<MenuItemViewModel> _menuItems = new();
    private CategoriesModel _selectedCategory;
    private bool _isLoading = false;
    private CartStateService _cartState;

    public ObservableCollection<MenuItemViewModel> MenuItems
    {
        get => _menuItems;
        set
        {
            _menuItems = value;
            OnPropertyChanged();
        }
    }

    public OrderPage()
    {
        InitializeComponent();
        BindingContext = this;
        NavigationPage.SetTitleView(this, NavigationMenu.CreateTitleView(this, NavMenu));
        _cartState = Application.Current?.Handler?.MauiContext?.Services?.GetService<CartStateService>() 
                     ?? new CartStateService();
        LoadData();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateHeader();
    }

    private void UpdateHeader()
    {
        if (MyGlobals._selectedTable != null)
        {
            HeaderSubtitle.Text = $"Table: {MyGlobals._selectedTable.TableNumber} | Service: {MyGlobals._serviceType}";
        }
        else
        {
            HeaderSubtitle.Text = $"Service: {MyGlobals._serviceType}";
        }
    }

    private async void LoadData()
    {
        // Load table if tableId is in query
        // Note: In MAUI, we'd pass this as a parameter or use a navigation parameter
        // For now, we'll use MyGlobals._selectedTable which should be set from TablesPage
        
        await LoadCategories();
    }

    private async Task LoadCategories()
    {
        try
        {
            var categories = await MyFunctions.GetAsync<List<CategoriesModel>>("api/Categories/Search", true) ?? new();
            _categories = new ObservableCollection<CategoriesModel>(categories);
            
            // Create category buttons
            CategoryButtonsLayout.Children.Clear();
            foreach (var category in _categories)
            {
                var button = new Button
                {
                    Text = category.Name,
                    BackgroundColor = Color.FromArgb("#512BD4"),
                    TextColor = Colors.White,
                    CornerRadius = 8,
                    Padding = new Thickness(15, 8),
                    Margin = new Thickness(0, 0, 8, 0)
                };
                button.Clicked += (s, e) => LoadItemsByCategory(category);
                CategoryButtonsLayout.Children.Add(button);
            }
            
            // Load items for first category
            if (_categories.Any())
            {
                _selectedCategory = _categories[0];
                await LoadItemsByCategory(_selectedCategory);
            }
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, $"Error loading categories: {ex.Message}");
            await DisplayAlert("Error", $"Failed to load categories: {ex.Message}", "OK");
        }
    }

    private async Task LoadItemsByCategory(CategoriesModel category)
    {
        try
        {
            _isLoading = true;
            if (LoadingOverlay != null) { LoadingOverlay.IsVisible = true; LoadingOverlay.Message = "Loading menu..."; }
            MenuItemsCollectionView.IsVisible = false;
            EmptyStateLayout.IsVisible = false;

            _selectedCategory = category;
            MenuItems.Clear();

            var criteria = Uri.EscapeDataString($"CategoryId={category.Id}");
            var fetchedItems = await MyFunctions.GetAsync<List<ItemsModel>>($"api/Items/Search/{criteria}", true) ?? new();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            foreach (var item in fetchedItems)
            {
                if (item.IsActive == 1)
                {
                    List<ServingSizeModel> sizes = new();
                    
                    if (!string.IsNullOrWhiteSpace(item.ServingSize))
                    {
                        try
                        {
                            sizes = JsonSerializer.Deserialize<List<ServingSizeModel>>(item.ServingSize, options) ?? new();
                        }
                        catch
                        {
                            sizes = new();
                        }
                    }

                    if (!sizes.Any())
                    {
                        sizes.Add(new ServingSizeModel
                        {
                            Size = "Regular",
                            Price = item.RetailPrice > 0 ? item.RetailPrice : item.BasePrice
                        });
                    }

                    var menuItem = new ItemsModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        MaxQty = 1,
                        RetailPrice = sizes.FirstOrDefault()?.Price ?? item.RetailPrice,
                        ServingSizes = sizes,
                        ServingSize = sizes.FirstOrDefault()?.Size ?? "Regular",
                        Rating = item.Rating,
                        Unit = item.Unit,
                        CategoryId = item.CategoryId
                    };
                    MenuItems.Add(new MenuItemViewModel(menuItem));
                }
            }

            UpdateEmptyState();
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, $"Error loading items for category {category.Name}: {ex.Message}");
            if (LoadingOverlay != null) LoadingOverlay.IsVisible = false;
            await DisplayAlert("Error", $"Failed to load items: {ex.Message}", "OK");
        }
        finally
        {
            _isLoading = false;
            if (LoadingOverlay != null) LoadingOverlay.IsVisible = false;
            MenuItemsCollectionView.IsVisible = MenuItems.Any();
        }
    }

    private void UpdateEmptyState()
    {
        bool isEmpty = !_isLoading && !MenuItems.Any();
        EmptyStateLayout.IsVisible = isEmpty;
        MenuItemsCollectionView.IsVisible = !isEmpty;
    }

    private void OnIncreaseQty(object sender, EventArgs e)
    {
        var vm = GetViewModelFromSender(sender);
        if (vm != null)
        {
            vm.MaxQty++;
            UpdateItemPrice(vm.Item);
            vm.NotifyAllProperties();
        }
    }

    private void OnDecreaseQty(object sender, EventArgs e)
    {
        var vm = GetViewModelFromSender(sender);
        if (vm != null && vm.MaxQty > 1)
        {
            vm.MaxQty--;
            UpdateItemPrice(vm.Item);
            vm.NotifyAllProperties();
        }
    }

    private static MenuItemViewModel? GetViewModelFromSender(object sender)
    {
        if (sender is not Button button) return null;
        return button.CommandParameter as MenuItemViewModel ?? button.BindingContext as MenuItemViewModel;
    }

    private void UpdateItemPrice(ItemsModel item)
    {
        var selectedSize = item.ServingSizes?.FirstOrDefault(s => s.Size == item.ServingSize);
        if (selectedSize != null)
        {
            item.RetailPrice = Convert.ToDouble(selectedSize.Price) * item.MaxQty;
        }
    }

    private async void OnAddToCart(object sender, EventArgs e)
    {
        var vm = GetViewModelFromSender(sender);
        if (vm == null) return;
        var item = vm.Item;
        try
        {
            var newItem = new ItemsModel
            {
                Id = item.Id,
                Name = item.Name,
                MaxQty = item.MaxQty,
                Unit = item.Unit,
                RetailPrice = item.RetailPrice,
                ServingSize = item.ServingSize,
                Person = item.Person,
                Description = item.Description,
                RevenueAccountId = item.RevenueAccountId,
                ExpenseAccountId = item.ExpenseAccountId
            };
            _cartState.CartItems?.Add(newItem);
            _cartState.SelectedTable = MyGlobals._selectedTable;
            _cartState.ServiceType = MyGlobals._serviceType;
            // Non-blocking toast: show message and auto-hide after 2 seconds (no OK tap)
            if (ToastLabel != null) ToastLabel.Text = $"✓ Added: {item.Name}";
            if (ToastBorder != null) ToastBorder.IsVisible = true;
            _ = DismissToastAfterAsync(2000);
            vm.MaxQty = 1;
            UpdateItemPrice(vm.Item);
            vm.NotifyAllProperties();
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, $"Error adding item to cart: {ex.Message}");
            await DisplayAlert("Error", $"Failed to add item to cart: {ex.Message}", "OK");
        }
    }

    private async Task DismissToastAfterAsync(int milliseconds)
    {
        await Task.Delay(milliseconds);
        if (ToastBorder != null)
            MainThread.BeginInvokeOnMainThread(() => ToastBorder.IsVisible = false);
    }

    private void OnViewCartClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new CartPage());
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }
}

