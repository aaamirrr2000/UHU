using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;

namespace MicroERP.App.SwiftServe.Services;

public interface INavigationService
{
    Task NavigateToAsync<TPage>() where TPage : Page, new();
    Task NavigateToAsync(Page page); // Overload for pages needing parameters
    Task NavigateBackAsync();
}


public class NavigationService : INavigationService
{
    public async Task NavigateToAsync<TPage>() where TPage : Page, new()
    {
        var page = new TPage();
        await NavigateToAsync(page);
    }

    public async Task NavigateToAsync(Page page)
    {
        if (Application.Current.MainPage is NavigationPage navPage)
        {
            await navPage.PushAsync(page);
        }
        else if (Application.Current.MainPage is not null)
        {
            // Wrap the page in a NavigationPage for proper stack-based navigation
            Application.Current.MainPage = new NavigationPage(page);
        }
        else
        {
            throw new InvalidOperationException("Application.Current.MainPage is null.");
        }
    }

    public async Task NavigateBackAsync()
    {
        if (Application.Current.MainPage is NavigationPage navPage)
        {
            await navPage.PopAsync();
        }
    }
}

