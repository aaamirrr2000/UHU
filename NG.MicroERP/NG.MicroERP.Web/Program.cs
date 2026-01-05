

using MudBlazor;
using MudBlazor.Services;
using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Services;
using NG.MicroERP.Web.Components;
using NG.MicroERP.Web.Services;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options => { options.DetailedErrors = true; });


// Add device-specific services used by the NG.MicroERP.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 2000;
    config.SnackbarConfiguration.HideTransitionDuration = 300;
    config.SnackbarConfiguration.ShowTransitionDuration = 300;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

builder.Services.AddServices();
builder.Services.AddScoped<Globals>();
builder.Services.AddScoped<Functions>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ClientInfoService>();

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        rollOnFileSizeLimit: true)
    .CreateLogger();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(NG.MicroERP.Shared._Imports).Assembly);

app.Run();
