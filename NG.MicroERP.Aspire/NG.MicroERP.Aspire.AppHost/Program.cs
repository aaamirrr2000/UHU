var builder = DistributedApplication.CreateBuilder(args);
var apiService = builder.AddProject<Projects.NG_MicroERP_Aspire_ApiService>("aspire-api-service");

var nexgenApi = builder.AddProject<Projects.NG_MicroERP_API>("nexgen-api-service")
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.NG_MicroERP_Web>("nexgen-api-webapp")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

// Avalonia desktop app (login and main window)
var avaloniaDesktop = builder.AddProject<Projects.AvaloniaApp>("avalonia-desktop")
    .WithExternalHttpEndpoints()
    .WaitFor(nexgenApi);

// ControlCenter Website - public-facing registration site
var controlCenterWebsite = builder.AddProject<Projects.NG_ControlCenter_WebSite>("ng-controlcenter-website")
    .WithExternalHttpEndpoints()
    .WithReference(nexgenApi);

// Note: MAUI apps (NG.MicroERP.WorkHub, NG.MicroERP.App, NG.MicroERP.App.SwiftServe, etc.) 
// are excluded from Aspire as they are desktop/mobile applications, not web services.
// Aspire is designed for orchestrating web services and APIs, not desktop/mobile apps.

builder.Build().Run();
