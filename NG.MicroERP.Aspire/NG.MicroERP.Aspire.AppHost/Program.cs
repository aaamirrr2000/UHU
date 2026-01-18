var builder = DistributedApplication.CreateBuilder(args);
var apiService = builder.AddProject<Projects.NG_MicroERP_Aspire_ApiService>("aspire-api-service");

var nexgenApi = builder.AddProject<Projects.NG_MicroERP_API>("nexgen-api-service")
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.NG_MicroERP_Web>("nexgen-api-webapp")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

// ControlCenter Website - public-facing registration site
var controlCenterWebsite = builder.AddProject<Projects.NG_ControlCenter_WebSite>("ng-controlcenter-website")
    .WithExternalHttpEndpoints()
    .WithReference(nexgenApi);

builder.Build().Run();
