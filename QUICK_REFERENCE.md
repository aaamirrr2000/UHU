# 🚀 Quick Reference Card - Web Application Enhancements

## State Persistence (Survives Page Refresh)

### Save Data
```csharp
@inject IStateService StateService

await StateService.SaveStateAsync("form_key", myObject);
```

### Load Data
```csharp
protected override async Task OnInitializedAsync()
{
    myData = await StateService.LoadStateAsync<MyModel>("form_key") ?? new();
}
```

### Clear Data
```csharp
await StateService.RemoveStateAsync("form_key");
```

---

## User Notifications

### Success
```csharp
@inject INotificationService Notifier

NotificationService.ShowSuccess("Operation completed!");
```

### Error
```csharp
NotificationService.ShowError("Failed to save", "Details here");
```

### Warning & Info
```csharp
NotificationService.ShowWarning("This action cannot be undone");
NotificationService.ShowInfo("Please note this information");
```

---

## Input Validation

### Model with Validation
```csharp
using System.ComponentModel.DataAnnotations;

public class LoginModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; }
}
```

### Form with Validation
```razor
<EditForm Model="model" OnSubmit="HandleSubmit">
    <DataAnnotationsValidator />
    <ValidationSummary />
    
    <MudTextField @bind-Value="model.Email" 
        For="@(() => model.Email)" />
    <MudTextField @bind-Value="model.Password" 
        Type="InputType.Password"
        For="@(() => model.Password)" />
    
    <MudButton ButtonType="ButtonType.Submit">Submit</MudButton>
</EditForm>
```

---

## Environment Variables

### Set on Windows
```powershell
$env:JWT_KEY = "value"
$env:CONNECTION_STRING = "value"
```

### Set on Linux/Mac
```bash
export JWT_KEY="value"
export CONNECTION_STRING="value"
```

### Set in Docker
```dockerfile
ENV JWT_KEY=value
ENV CONNECTION_STRING=value
```

---

## Health Check

### Check API Health
```bash
curl https://yourdomain.com/health
curl https://yourdomain.com/health/ready
```

---

## Log Files

### View Today's Logs
```bash
tail -f ./logs/app-2024-01-21.txt
```

### View All Logs
```bash
ls ./logs/
```

---

## Common Page Template

```razor
@page "/mypage"
@using NG.ControlCenter.WebSite.Models
@using NG.ControlCenter.WebSite.Services
@using System.ComponentModel.DataAnnotations
@inject IStateService StateService
@inject INotificationService NotificationService

<EditForm Model="model" OnSubmit="HandleSubmitAsync">
    <DataAnnotationsValidator />
    <ValidationSummary />
    
    <MudTextField @bind-Value="model.Name" 
        For="@(() => model.Name)" />
    
    <MudButton ButtonType="ButtonType.Submit" 
        Loading="isLoading">
        Save
    </MudButton>
</EditForm>

@code {
    private MyModel model = new();
    private bool isLoading = false;

    protected override async Task OnInitializedAsync()
    {
        // Load saved state
        model = await StateService.LoadStateAsync<MyModel>("mypage_key") ?? new();
    }

    private async Task HandleSubmitAsync()
    {
        try
        {
            isLoading = true;
            
            // Save to API
            // await ApiService.SaveAsync(model);
            
            // Clear saved state
            await StateService.RemoveStateAsync("mypage_key");
            
            NotificationService.ShowSuccess("Saved successfully!");
        }
        catch (Exception ex)
        {
            // Keep state saved for recovery
            await StateService.SaveStateAsync("mypage_key", model);
            
            NotificationService.ShowError("Save failed", ex.Message);
        }
        finally
        {
            isLoading = false;
        }
    }
}
```

---

## HTTP Client with Timeout

```csharp
@inject HttpClient Http

// Automatically configured with:
// - 30 second timeout
// - 3 automatic retries
// - Circuit breaker

var response = await Http.GetAsync("api/endpoint");
```

---

## Error Handling

### Global Exception Handler
Already active! All exceptions logged and handled gracefully.

### User-Friendly Error Page
Shows at `/Error` with recovery options.

---

## Security

### CORS Configuration
Set `ALLOWED_ORIGINS` environment variable:
```
https://yourdomain.com,https://www.yourdomain.com
```

### Rate Limiting
- 100 requests per minute per IP
- Automatic 429 response when exceeded

---

## Checklist for New Pages

- [ ] Add `@inject IStateService StateService`
- [ ] Add `@inject INotificationService NotificationService`
- [ ] Load state in `OnInitializedAsync()`
- [ ] Save state before submit
- [ ] Add validation to model
- [ ] Use `DataAnnotationsValidator`
- [ ] Show `ValidationSummary`
- [ ] Use `NotificationService` for feedback

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| State not persisting | Check localStorage enabled, verify StateService injected |
| No log files | Create `/logs/` folder, check permissions |
| API timeout | Increase timeout in HttpClientService.cs |
| CORS errors | Add origin to ALLOWED_ORIGINS env var |
| Validation not showing | Use `<ValidationSummary />` component |

---

## Key Files

| File | Purpose |
|------|---------|
| `Services/StateService.cs` | State persistence |
| `Services/NotificationService.cs` | User feedback |
| `Services/HttpClientService.cs` | HTTP resilience |
| `Middleware/GlobalExceptionHandlerMiddleware.cs` | Exception handling |
| `Models/ValidationModels.cs` | Validation examples |
| `Components/Pages/Error.razor` | Error page |

---

## Resources

- **Implementation Guide**: [IMPLEMENTATION_GUIDE.md](IMPLEMENTATION_GUIDE.md)
- **Configuration Guide**: [ENV_CONFIG_GUIDE.md](ENV_CONFIG_GUIDE.md)
- **Changes Summary**: [CHANGES_SUMMARY.md](CHANGES_SUMMARY.md)
- **Login Example**: [LoginExample.razor](Components/Pages/LoginExample.razor)
- **Register Example**: [RegisterExample.razor](Components/Pages/RegisterExample.razor)

---

## Time Savers

```csharp
// Quick state save
await StateService.SaveStateAsync("key", data);

// Quick state load  
var data = await StateService.LoadStateAsync<Type>("key") ?? new();

// Quick notification
NotificationService.ShowSuccess("Done!");

// All validation at once
<DataAnnotationsValidator />
<ValidationSummary />
```

---

**Remember**: The app now gracefully handles errors and persists user data! 🎉
