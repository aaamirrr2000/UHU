using MicroERP.App.SwiftServe.Helper;
using MicroERP.App.SwiftServe.Components.MauiPages;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System.Collections.Generic;
using Microsoft.Maui.Storage;

namespace MicroERP.App.SwiftServe.Components.MauiPages.Controls;

public partial class NavigationMenu : ContentView
{
    private bool _isMenuOpen = false;
    private List<MenuItem> _menuItems = new();

    public NavigationMenu()
    {
        InitializeComponent();
        LoadMenuItems();
        UpdateUserInfo();
    }

    private void LoadMenuItems()
    {
        var userType = MyGlobals.User?.UserType?.ToUpper() ?? "";
        _menuItems.Clear();

        // Always show "Submit Order" and "Place Order" for most user types (except ONLINE)
        if (userType != "ONLINE")
        {
            _menuItems.Add(new MenuItem
            {
                Text = "Submit Order",
                PageType = typeof(TablesPage),
                Icon = "🍽"
            });
            _menuItems.Add(new MenuItem
            {
                Text = "Place Order",
                PageType = typeof(OrderPage),
                Icon = "📝"
            });
        }

        // Menu items based on user type
        switch (userType)
        {
            case "WAITER":
                _menuItems.Add(new MenuItem
                {
                    Text = "Kitchen",
                    PageType = typeof(KitchenPage),
                    Icon = "🍳"
                });
                _menuItems.Add(new MenuItem
                {
                    Text = "Orders History",
                    PageType = typeof(OrdersPage),
                    Icon = "📋"
                });
                break;

            case "ONLINE":
                _menuItems.Add(new MenuItem
                {
                    Text = "Online Orders",
                    PageType = typeof(OrdersPage),
                    Icon = "🌐",
                    Parameter = "Online"
                });
                _menuItems.Add(new MenuItem
                {
                    Text = "All Orders",
                    PageType = typeof(OrdersPage),
                    Icon = "📋"
                });
                break;

            case "KITCHEN":
                _menuItems.Add(new MenuItem
                {
                    Text = "Kitchen",
                    PageType = typeof(KitchenPage),
                    Icon = "🍳"
                });
                _menuItems.Add(new MenuItem
                {
                    Text = "All Orders",
                    PageType = typeof(OrdersPage),
                    Icon = "📋"
                });
                break;

            case "ADMIN":
                _menuItems.Add(new MenuItem
                {
                    Text = "Kitchen",
                    PageType = typeof(KitchenPage),
                    Icon = "🍳"
                });
                _menuItems.Add(new MenuItem
                {
                    Text = "Orders History",
                    PageType = typeof(OrdersPage),
                    Icon = "📋"
                });
                _menuItems.Add(new MenuItem
                {
                    Text = "Online Orders",
                    PageType = typeof(OrdersPage),
                    Icon = "🌐",
                    Parameter = "Online"
                });
                break;

            default:
                _menuItems.Add(new MenuItem
                {
                    Text = "Kitchen",
                    PageType = typeof(KitchenPage),
                    Icon = "🍳"
                });
                _menuItems.Add(new MenuItem
                {
                    Text = "Orders History",
                    PageType = typeof(OrdersPage),
                    Icon = "📋"
                });
                break;
        }

        // Account section - commented out until UserProfilePage is created
        // _menuItems.Add(new MenuItem
        // {
        //     Text = "My Profile",
        //     PageType = typeof(UserProfilePage),
        //     Icon = "👤",
        //     IsSeparator = true
        // });

        RenderMenuItems();
    }

    private void RenderMenuItems()
    {
        MenuItemsContainer.Children.Clear();

        foreach (var item in _menuItems)
        {
            if (item.IsSeparator && MenuItemsContainer.Children.Count > 0)
            {
                var separator = new BoxView
                {
                    HeightRequest = 1,
                    BackgroundColor = Color.FromArgb("#E0E0E0"),
                    Margin = new Thickness(12, 8)
                };
                MenuItemsContainer.Children.Add(separator);
            }

            var row = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition { Width = GridLength.Auto }, new ColumnDefinition { Width = GridLength.Star } }, Padding = new Thickness(14, 10), MinimumHeightRequest = 48 };
            var iconLabel = new Label
            {
                Text = item.Icon,
                FontSize = 20,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 12, 0)
            };
            var textLabel = new Label
            {
                Text = item.Text,
                FontSize = 16,
                TextColor = Color.FromArgb("#212121"),
                VerticalOptions = LayoutOptions.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
            row.Add(iconLabel, 0, 0);
            row.Add(textLabel, 1, 0);

            var wrap = new Border
            {
                Content = row,
                StrokeThickness = 0,
                BackgroundColor = Color.FromArgb("#F5F5F5"),
                Margin = new Thickness(0, 3),
                StrokeShape = new RoundRectangle { CornerRadius = 10 }
            };
            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) => OnMenuItemClicked(item);
            wrap.GestureRecognizers.Add(tap);
            MenuItemsContainer.Children.Add(wrap);
        }
    }

    private void OnMenuItemClicked(MenuItem item)
    {
        CloseMenu();

        if (item.PageType != null)
        {
            // When going to Place Order (OrderPage), ensure default service type if none set
            if (item.PageType == typeof(OrderPage) && string.IsNullOrEmpty(MyGlobals._serviceType))
                MyGlobals._serviceType = "Dine-In";

            Page? page = null;

            // Handle pages with parameters
            if (item.Parameter != null)
            {
                try
                {
                    page = Activator.CreateInstance(item.PageType, item.Parameter) as Page;
                }
                catch
                {
                    // If parameter constructor fails, try parameterless
                    page = Activator.CreateInstance(item.PageType) as Page;
                }
            }
            else
            {
                page = Activator.CreateInstance(item.PageType) as Page;
            }

            if (page != null)
            {
                // Get current navigation page and push new page
                if (Application.Current?.MainPage is NavigationPage navPage)
                {
                    navPage.Navigation.PushAsync(page);
                }
                else
                {
                    Application.Current!.MainPage = new NavigationPage(page);
                }
            }
        }
    }

    /// <summary>Opens or closes the flyout. Call from the nav bar menu button.</summary>
    public void ToggleMenu()
    {
        _isMenuOpen = !_isMenuOpen;
        SidebarFlyout.IsVisible = _isMenuOpen;
        Overlay.IsVisible = _isMenuOpen;
        NavigationMenuControl.HorizontalOptions = _isMenuOpen ? LayoutOptions.Fill : LayoutOptions.Start;
        NavigationMenuControl.VerticalOptions = _isMenuOpen ? LayoutOptions.Fill : LayoutOptions.Start;
    }

    private void OnCloseMenuClicked(object sender, EventArgs e)
    {
        CloseMenu();
    }

    private void OnOverlayTapped(object sender, EventArgs e)
    {
        CloseMenu();
    }

    private void CloseMenu()
    {
        _isMenuOpen = false;
        SidebarFlyout.IsVisible = false;
        Overlay.IsVisible = false;
        NavigationMenuControl.HorizontalOptions = LayoutOptions.Start;
        NavigationMenuControl.VerticalOptions = LayoutOptions.Start;
    }

    /// <summary>Builds a title view for the nav bar: [☰] [title]. Use as page.TitleView.</summary>
    public static View CreateTitleView(Page page, NavigationMenu navMenu)
    {
        var menuBtn = new Button
        {
            Text = "☰",
            FontSize = 22,
            BackgroundColor = Colors.Transparent,
            TextColor = Colors.White,
            Padding = new Thickness(12, 0),
            WidthRequest = 48,
            HeightRequest = 44,
            VerticalOptions = LayoutOptions.Center
        };
        menuBtn.Clicked += (_, _) => navMenu.ToggleMenu();

        var titleLabel = new Label
        {
            Text = page.Title ?? "",
            TextColor = Colors.White,
            FontSize = 18,
            VerticalOptions = LayoutOptions.Center,
            VerticalTextAlignment = TextAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0)
        };

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition { Width = GridLength.Auto }, new ColumnDefinition { Width = GridLength.Star } } };
        grid.Add(menuBtn, 0, 0);
        grid.Add(titleLabel, 1, 0);
        return grid;
    }

    private void UpdateUserInfo()
    {
        var username = MyGlobals.User?.Username ?? "User";
        UserInfoLabel.Text = username;
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        CloseMenu();
        
        bool confirm = await Application.Current!.MainPage!.DisplayAlert(
            "Log Out",
            "Are you sure you want to log out?",
            "Yes",
            "No"
        );

        if (confirm)
        {
            // Clear user data
            MyGlobals.User = null;
            MyGlobals.Token = string.Empty;
            MyGlobals.Organization = null;
            
            // Clear saved credentials
            Preferences.Remove("Username");
            Preferences.Remove("Password");
            
            // Navigate back to login
            var loginPage = new LoginPage();
            Application.Current!.MainPage = new NavigationPage(loginPage);
        }
    }

    private class MenuItem
    {
        public string Text { get; set; } = string.Empty;
        public Type? PageType { get; set; }
        public string Icon { get; set; } = string.Empty;
        public object? Parameter { get; set; }
        public bool IsSeparator { get; set; }
    }
}

