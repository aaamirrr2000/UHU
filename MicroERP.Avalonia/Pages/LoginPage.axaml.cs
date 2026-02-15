using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MicroERP.Shared.Helper;
using MicroERP.Shared.Models;

namespace Avalonia.Pages
{
    public partial class LoginPage : Window
    {
        private bool _isSigningIn;

        public LoginPage()
        {
            InitializeComponent();
            // Enter key submits (keyboard-first)
            AddHandler(KeyDownEvent, OnWindowKeyDown, RoutingStrategies.Tunnel);
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            // Focus username for keyboard entry without mouse
            UsernameBox?.Focus();
        }

        private void OnWindowKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || _isSigningIn) return;
            // If focus is on a button, let it handle; otherwise submit
            if (e.Source is not Button)
                _ = TrySignInAsync();
            e.Handled = false;
        }

        private async void OnSignInClick(object? sender, RoutedEventArgs e) => await TrySignInAsync();

        private async Task TrySignInAsync()
        {
            if (_isSigningIn) return;
            var username = UsernameBox?.Text?.Trim() ?? "";
            var password = PasswordBox?.Text ?? "";

            HideError();

            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("Please enter your username.");
                UsernameBox?.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Please enter your password.");
                PasswordBox?.Focus();
                return;
            }

            _isSigningIn = true;
            SetSigningIn(true);

            try
            {
                // Same logic as Shared/Pages/Login/LoginPage.razor
                var functions = App.Functions;
                var globals = App.Globals;

                // Same URL as Shared: API does ToUpper on username server-side
                var res = await functions.GetAsync<UsersModel>($"Login/Login/{Uri.EscapeDataString(username.Trim())}/{Uri.EscapeDataString(password)}", false);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _isSigningIn = false;
                    SetSigningIn(false);
                });

                if (res == null || string.IsNullOrEmpty(res.Token))
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        ShowError("User Name or Password is Incorrect.");
                    });
                    return;
                }

                globals.User = res;
                try { globals.User.Password = globals.Decrypt(res.Password ?? ""); } catch { /* ignore */ }
                globals.Token = res.Token;

                var orgList = await functions.GetAsync<System.Collections.Generic.List<OrganizationsModel>>("Organizations/Search", true);
                globals.Organization = orgList?.FirstOrDefault() ?? new OrganizationsModel();

                var emp = await functions.GetAsync<EmployeesModel>($"Employees/Get/{res.EmpId}", true) ?? new EmployeesModel();
                globals.Employee = emp;

                var dept = await functions.GetAsync<DepartmentsModel>($"Departments/Get/{emp.DepartmentId}", true) ?? new DepartmentsModel();
                globals.Department = dept;

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (globals.Organization?.Expiry != null && globals.Organization.Expiry <= DateTime.Now)
                    {
                        ShowError("License Expired.");
                        return;
                    }
                    var main = new MainPage();
                    main.Show();
                    Close();
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _isSigningIn = false;
                    SetSigningIn(false);
                    ShowError(ex.Message);
                });
            }
        }

        private void ShowError(string message)
        {
            if (ErrorText != null)
            {
                ErrorText.Text = message;
                ErrorText.IsVisible = true;
            }
        }

        private void HideError()
        {
            if (ErrorText != null)
            {
                ErrorText.Text = "";
                ErrorText.IsVisible = false;
            }
        }

        private void SetSigningIn(bool signingIn)
        {
            if (SignInButton != null)
            {
                SignInButton.IsEnabled = !signingIn;
                SignInButton.Content = signingIn ? "Signing in…" : "Sign in";
            }
            if (UsernameBox != null) UsernameBox.IsEnabled = !signingIn;
            if (PasswordBox != null) PasswordBox.IsEnabled = !signingIn;
            if (RememberCheck != null) RememberCheck.IsEnabled = !signingIn;
            if (BusyOverlay != null) BusyOverlay.IsVisible = signingIn;
        }

        private void OnForgotPasswordClick(object? sender, RoutedEventArgs e)
        {
            ShowError("Use the web app to reset your password.");
        }

        private void OnCreateAccountClick(object? sender, RoutedEventArgs e)
        {
            ShowError("Use the web app or Control Center to create an account.");
        }
    }
}

