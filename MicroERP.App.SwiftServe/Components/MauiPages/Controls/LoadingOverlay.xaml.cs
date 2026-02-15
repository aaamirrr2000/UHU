using Microsoft.Maui.Controls;

namespace MicroERP.App.SwiftServe.Components.MauiPages.Controls;

public partial class LoadingOverlay : ContentView
{
    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(LoadingOverlay), "Loading...",
            propertyChanged: (b, o, n) => { if (b is LoadingOverlay ov && ov.MessageLabel != null) ov.MessageLabel.Text = (string?)n ?? "Loading..."; });

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public LoadingOverlay()
    {
        InitializeComponent();
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        if (MessageLabel != null)
            MessageLabel.Text = Message;
    }
}
