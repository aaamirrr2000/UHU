using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MicroERP.Shared.Models;

namespace Avalonia.Pages.Invoices
{
    public partial class ItemPickerWindow : Window
    {
        private List<ItemsModel> _allItems = new();
        public ItemsModel? SelectedItem { get; private set; }

        public ItemPickerWindow()
        {
            InitializeComponent();
            AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        }

        protected override async void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            await LoadItemsAsync();
        }

        private async Task LoadItemsAsync()
        {
            StatusLabel.Text = "Loading…";
            try
            {
                _allItems = await App.Functions.GetAsync<List<ItemsModel>>("Items/Search", true) ?? new List<ItemsModel>();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Error: {ex.Message}";
                _allItems = new List<ItemsModel>();
            }
        }

        private void ApplyFilter()
        {
            var q = TxtSearch?.Text?.Trim() ?? "";
            var list = string.IsNullOrWhiteSpace(q)
                ? _allItems
                : _allItems.Where(x =>
                    (x.Name?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.Code?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
            ListItems.ItemsSource = list;
            StatusLabel.Text = $"{list.Count} item(s). Double-click or Enter to add.";
        }

        private void OnSearchKeyUp(object? sender, KeyEventArgs e)
        {
            ApplyFilter();
        }

        private void OnItemDoubleTapped(object? sender, TappedEventArgs e)
        {
            ConfirmSelection();
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ConfirmSelection();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }
        }

        private void ConfirmSelection()
        {
            if (ListItems.SelectedItem is ItemsModel item)
            {
                SelectedItem = item;
                SelectedItem.Qty = 1;
                Close(true);
            }
        }
    }
}

