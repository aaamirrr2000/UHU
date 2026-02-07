using NG.MicroERP.App.SwiftServe.Helper;
using NG.MicroERP.App.SwiftServe.Services;
using NG.MicroERP.Shared.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Linq;

namespace NG.MicroERP.App.SwiftServe.Components.MauiPages;

public partial class OrderPage : ContentPage
{
    private ObservableCollection<CategoriesModel> _categories = new();
    private ObservableCollection<ItemsModel> _menuItems = new();
    private CategoriesModel _selectedCategory;
    private bool _isLoading = false;
    private CartStateService _cartState;

    public ObservableCollection<ItemsModel> MenuItems
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
        
        // Get CartStateService (could be injected via DI or accessed statically)
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
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            MenuItemsCollectionView.IsVisible = false;
            EmptyStateLayout.IsVisible = false;

            _selectedCategory = category;
            MenuItems.Clear();

            var fetchedItems = await MyFunctions.GetAsync<List<ItemsModel>>($"api/Items/Search/CategoriesId={category.Id}", true) ?? new();

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
                    
                    MenuItems.Add(menuItem);
                }
            }

            UpdateEmptyState();
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, $"Error loading items for category {category.Name}: {ex.Message}");
            await DisplayAlert("Error", $"Failed to load items: {ex.Message}", "OK");
        }
        finally
        {
            _isLoading = false;
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
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
        if (sender is Button button && button.CommandParameter is ItemsModel item)
        {
            item.MaxQty++;
            UpdateItemPrice(item);
        }
    }

    private void OnDecreaseQty(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ItemsModel item)
        {
            if (item.MaxQty > 1)
            {
                item.MaxQty--;
                UpdateItemPrice(item);
            }
        }
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
        if (sender is Button button && button.CommandParameter is ItemsModel item)
        {
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
                    Description = item.Description
                };
                
                _cartState.CartItems?.Add(newItem);
                _cartState.SelectedTable = MyGlobals._selectedTable;
                _cartState.ServiceType = MyGlobals._serviceType;
                
                // Show notification that item was added (user can continue shopping)
                await DisplayAlert("Added to Cart", $"{item.Name} has been added to your cart", "OK");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, $"Error adding item to cart: {ex.Message}");
                await DisplayAlert("Error", $"Failed to add item to cart: {ex.Message}", "OK");
            }
        }
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
