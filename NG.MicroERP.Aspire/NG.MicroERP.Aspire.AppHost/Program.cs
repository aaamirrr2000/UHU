var builder = DistributedApplication.CreateBuilder(args);
var apiService = builder.AddProject<Projects.NG_MicroERP_Aspire_ApiService>("aspire-api-service");

builder.AddProject<Projects.NG_MicroERP_API>("nexgen-api-service");

builder.AddProject<Projects.NG_MicroERP_Web>("nexgen-api-webapp")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);


builder.Build().Run();
