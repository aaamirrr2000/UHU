var builder = DistributedApplication.CreateBuilder(args);
var apiService = builder.AddProject<Projects.MicroERP_Aspire_ApiService>("Aspire-API-Service");

var microErpApi = builder.AddProject<Projects.MicroERP_API>("MicroERP-API-Service")
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.MicroERP_Web>("MicroERP-WebApp")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

// Avalonia desktop app (login and main window) - Disabled auto-launch
// var avaloniaDesktop = builder.AddProject<Projects.MicroERP_Avalonia>("Avalonia-Desktop")
//     .WithExternalHttpEndpoints()
//     .WaitFor(microErpApi);

// Note: MAUI apps are excluded from Aspire as they are desktop/mobile applications.

builder.Build().Run();

