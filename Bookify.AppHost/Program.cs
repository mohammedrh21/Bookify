var builder = DistributedApplication.CreateBuilder(args);

var apiService=builder.AddProject<Projects.Bookify_API>("apiservice");

builder.AddProject<Projects.Bookify_Client>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
