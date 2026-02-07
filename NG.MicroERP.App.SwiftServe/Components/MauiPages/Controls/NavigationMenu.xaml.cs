using NG.MicroERP.App.SwiftServe.Helper;
using NG.MicroERP.App.SwiftServe.Components.MauiPages;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using Microsoft.Maui.Storage;

namespace NG.MicroERP.App.SwiftServe.Components.MauiPages.Controls;

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

        // Always show "Submit Order" for most user types (except ONLINE)
        if (userType != "ONLINE")
        {
            _menuItems.Add(new MenuItem
            {
                Text = "Submit Order",
                PageType = typeof(TablesPage),
                Icon = "🍽"
            });
        }

        // Menu items based on user type
        switch (userType)
        {
            case "WAITER":
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
                    Text = "All Orders",
                    PageType = typeof(OrdersPage),
                    Icon = "📋"
                });
                break;

            case "ADMIN":
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
                // Add separator
                var separator = new BoxView
                {
                    HeightRequest = 1,
                    BackgroundColor = Color.FromArgb("#E0E0E0"),
                    Margin = new Thickness(8, 12)
                };
                MenuItemsContainer.Children.Add(separator);
            }

            var menuButton = new Button
            {
                Text = $"{item.Icon} {item.Text}",
                BackgroundColor = Colors.Transparent,
                TextColor = Color.FromArgb("#212121"),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                CornerRadius = 8,
                HeightRequest = 48,
                Margin = new Thickness(4, 2),
                Padding = new Thickness(12, 0)
            };

            menuButton.Clicked += (s, e) => OnMenuItemClicked(item);
            MenuItemsContainer.Children.Add(menuButton);
        }
    }

    private void OnMenuItemClicked(MenuItem item)
    {
        CloseMenu();

        if (item.PageType != null)
        {
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

    private void OnMenuButtonClicked(object sender, EventArgs e)
    {
        ToggleMenu();
    }

    private void OnCloseMenuClicked(object sender, EventArgs e)
    {
        CloseMenu();
    }

    private void OnOverlayTapped(object sender, EventArgs e)
    {
        CloseMenu();
    }

    private void ToggleMenu()
    {
        _isMenuOpen = !_isMenuOpen;
        SidebarFlyout.IsVisible = _isMenuOpen;
        Overlay.IsVisible = _isMenuOpen;
    }

    private void CloseMenu()
    {
        _isMenuOpen = false;
        SidebarFlyout.IsVisible = false;
        Overlay.IsVisible = false;
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
