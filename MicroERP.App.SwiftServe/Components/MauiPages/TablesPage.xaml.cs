using MicroERP.App.SwiftServe.Helper;
using MicroERP.Shared.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MicroERP.App.SwiftServe.Components.MauiPages;

public partial class TablesPage : ContentPage, INotifyPropertyChanged
{
    private ObservableCollection<RestaurantTablesModel> _tables = new();
    private ObservableCollection<RestaurantTablesModel> _filteredTables = new();
    private bool _isLoading = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public ObservableCollection<RestaurantTablesModel> FilteredTables
    {
        get => _filteredTables;
        set
        {
            if (_filteredTables != value)
            {
                _filteredTables = value;
                OnPropertyChanged();
            }
        }
    }

    public TablesPage()
    {
        InitializeComponent();
        BindingContext = this;
        
        // Ensure BaseURI is set
        if (string.IsNullOrEmpty(MyGlobals.BaseURI))
        {
            string savedUrl = Preferences.Get("BaseURI", "").Trim();
            MyGlobals.BaseURI = !string.IsNullOrEmpty(savedUrl) ? savedUrl : "https://localhost:7019/api/";
        }

        var userType = MyGlobals.User?.UserType?.ToUpper() ?? "";
        if (userType == "ONLINE")
        {
            // Replace navigation stack with OrdersPage for online users
            Application.Current!.MainPage = new NavigationPage(new OrdersPage("Online"));
            return;
        }

        _ = LoadTablesAsync();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        MyGlobals.PageTitle = "Tables";
    }

    private async Task LoadTablesAsync()
    {
        try
        {
            _isLoading = true;
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            string apiUrl = BuildApiUrl("api/RestaurantTables/Search");
            
            Serilog.Log.Information($"Calling RestaurantTables API: {apiUrl}");
            
            var result = await MyFunctions.GetAsync<List<RestaurantTablesModel>>(apiUrl, true);

            if (result != null && result.Any())
            {
                Serilog.Log.Information($"Loaded {result.Count} tables successfully");
                _tables = new ObservableCollection<RestaurantTablesModel>(
                    result.Select(table =>
                    {
                        table.AvailableStatus = table.IsAvailable == 1 ? "Busy" : "Available";
                        return table;
                    }).OrderBy(t => t.TableNumber).ToList()
                );
                
                FilteredTables = new ObservableCollection<RestaurantTablesModel>(_tables);
                UpdateTableCount();
                
                // Force UI update on main thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    OnPropertyChanged(nameof(FilteredTables));
                    UpdateEmptyState();
                });
            }
            else
            {
                Serilog.Log.Warning("No tables returned from API");
                _tables = new ObservableCollection<RestaurantTablesModel>();
                FilteredTables = new ObservableCollection<RestaurantTablesModel>();
            }

            UpdateEmptyState();
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, $"Error loading tables: {ex.Message}");
            _tables = new ObservableCollection<RestaurantTablesModel>();
            FilteredTables = new ObservableCollection<RestaurantTablesModel>();
            await DisplayAlert("Error", $"Failed to load tables: {ex.Message}", "OK");
        }
        finally
        {
            _isLoading = false;
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private void UpdateTableCount()
    {
        TableCountLabel.Text = $"{FilteredTables.Count} Tables";
    }

    private void UpdateEmptyState()
    {
        bool isEmpty = !_isLoading && !FilteredTables.Any();
        EmptyStateLayout.IsVisible = isEmpty;
        TablesCollectionView.IsVisible = !isEmpty;
        
        if (isEmpty && !string.IsNullOrWhiteSpace(SearchEntry.Text))
        {
            EmptyStateMessage.Text = "No tables match your search criteria. Try a different search term.";
        }
        else
        {
            EmptyStateMessage.Text = "Please ensure tables are configured in the system.";
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        FilterTables();
    }

    private void FilterTables()
    {
        string searchText = SearchEntry.Text?.ToLower() ?? "";
        
        if (string.IsNullOrWhiteSpace(searchText))
        {
            FilteredTables = new ObservableCollection<RestaurantTablesModel>(_tables);
        }
        else
        {
            var filtered = _tables.Where(t => 
                t.TableNumber?.ToLower().Contains(searchText) == true ||
                t.TableLocation?.ToLower().Contains(searchText) == true ||
                t.Capacity.ToString().Contains(searchText)
            ).ToList();
            
            FilteredTables = new ObservableCollection<RestaurantTablesModel>(filtered);
        }
        
        // Force UI update
        MainThread.BeginInvokeOnMainThread(() =>
        {
            OnPropertyChanged(nameof(FilteredTables));
            UpdateTableCount();
            UpdateEmptyState();
        });
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await LoadTablesAsync();
    }

    private void OnServiceTypeSelected(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is RestaurantTablesModel table)
        {
            NavigateToOrderPage(table, "Dine-In");
        }
    }

    private void OnTakeawaySelected(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is RestaurantTablesModel table)
        {
            NavigateToOrderPage(table, "Takeaway");
        }
    }

    private void OnParcelSelected(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is RestaurantTablesModel table)
        {
            NavigateToOrderPage(table, "Parcel");
        }
    }

    private void NavigateToOrderPage(RestaurantTablesModel selectedTable, string serviceType)
    {
        MyGlobals._selectedTable = selectedTable;
        MyGlobals._serviceType = serviceType;
        Navigation.PushAsync(new OrderPage());
    }

    private void OnViewOrderClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is int tableId)
        {
            Navigation.PushAsync(new OrderDetailsPage("TABLE", tableId));
        }
    }

    private void OnTableTapped(object sender, EventArgs e)
    {
        // Optional: Handle table tap if needed
    }

    private void UpdateTableVisualStates()
    {
        // This will be called after items are rendered to update visual states
        // Note: In a production app, you'd use a DataTemplateSelector or Converter
        // For now, we'll handle button states via IsEnabled binding in XAML
    }

    private string BuildApiUrl(string endpoint)
    {
        if (string.IsNullOrEmpty(MyGlobals.BaseURI))
            return endpoint.TrimStart('/');

        string baseUri = MyGlobals.BaseURI.TrimEnd('/');
        string normalizedEndpoint = endpoint.TrimStart('/');

        bool baseUriHasApi = baseUri.EndsWith("/api", StringComparison.OrdinalIgnoreCase) || 
                            baseUri.EndsWith("/api/", StringComparison.OrdinalIgnoreCase) ||
                            baseUri.EndsWith("api", StringComparison.OrdinalIgnoreCase);

        bool endpointHasApi = normalizedEndpoint.StartsWith("api/", StringComparison.OrdinalIgnoreCase);

        if (baseUriHasApi && endpointHasApi)
        {
            return normalizedEndpoint.Substring(4);
        }
        else if (!baseUriHasApi && !endpointHasApi)
        {
            return $"api/{normalizedEndpoint}";
        }
        else
        {
            return normalizedEndpoint;
        }
    }
}

