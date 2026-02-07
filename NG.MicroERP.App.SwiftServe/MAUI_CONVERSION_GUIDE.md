# MAUI Pages Conversion Guide

This document outlines the conversion of Blazor pages to MAUI XAML pages in the SwiftServe project.

## Conversion Status

### ✅ Completed
- TablesPage.razor → TablesPage.xaml + TablesPage.xaml.cs
- LoginPage.razor → LoginPage.xaml + LoginPage.xaml.cs (already existed)
- UserTypeWarningPage.razor → UserTypeWarningPage.xaml + UserTypeWarningPage.xaml.cs (already existed)

### 🔄 In Progress / To Do
- OrderPage.razor → OrderPage.xaml
- CartPage.razor → CartPage.xaml
- OrdersPage.razor → OrdersPage.xaml
- OrderDetailsPage.razor → OrderDetailsPage.xaml
- GenerateBillPage.razor → GenerateBillPage.xaml
- ClientFeedbackPage.razor → ClientFeedbackPage.xaml
- UserProfilePage.razor → UserProfilePage.xaml
- Home.razor → HomePage.xaml
- LogoutPage.razor → LogoutPage.xaml

## Key Conversion Patterns

### 1. Navigation
**Blazor:**
```csharp
Nav.NavigateTo("/OrderPage?tableId=123");
```

**MAUI:**
```csharp
Navigation.PushAsync(new OrderPage());
// Or with parameters:
var page = new OrderPage { TableId = 123 };
Navigation.PushAsync(page);
```

### 2. Data Binding
**Blazor:**
```razor
@foreach (var item in items)
{
    <div>@item.Name</div>
}
```

**MAUI:**
```xml
<CollectionView ItemsSource="{Binding Items}">
    <CollectionView.ItemTemplate>
        <DataTemplate>
            <Label Text="{Binding Name}"/>
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>
```

### 3. State Management
- Use `ObservableCollection<T>` for collections that need to update UI
- Use `INotifyPropertyChanged` for properties that need to trigger UI updates
- `MyGlobals` static class works the same in both Blazor and MAUI

### 4. API Calls
- `MyFunctions.GetAsync<T>()` and `MyFunctions.PostAsync<T>()` work identically
- No changes needed for API integration

### 5. Services
- `CartStateService` can be used via Dependency Injection or as a static service
- Register in `MauiProgram.cs` if using DI

## Next Steps

1. Complete conversion of remaining pages
2. Update MainPage to use MAUI navigation instead of BlazorWebView
3. Update Routes.razor or remove it if not needed
4. Test all navigation flows
5. Update any remaining Blazor-specific code

## Notes

- MudBlazor components are replaced with native MAUI controls
- CSS styling is converted to XAML Styles and Resources
- Event handlers use `Clicked` instead of `@onclick`
- Data binding uses XAML syntax instead of Razor syntax
