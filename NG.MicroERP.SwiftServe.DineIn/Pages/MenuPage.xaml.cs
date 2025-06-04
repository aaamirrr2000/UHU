using System;

using Microsoft.Maui.Controls;

namespace NG.MicroERP.SwiftServe.DineIn.Pages
{
    public partial class MenuPage : ContentPage
    {
        public MenuPage()
        {
            InitializeComponent();
        }

        private async void OnHomeTapped(object sender, EventArgs e)
        {
            //await Shell.Current.GoToAsync("//OrderPage");
            await Navigation.PushAsync(new OrdersPage());
        }

        private async void OnOrdersTapped(object sender, EventArgs e)
        {
            //await Shell.Current.GoToAsync("//ExecutionPage");
            await Navigation.PushAsync(new ExecutionPage());
        }

        // Add more event handlers as needed
    }
}
