using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var server = builder.AddProject<Projects.ObituaryApplication>("server");

// Optional: include the client explicitly; not required if Server has a ProjectReference to Client
var client = builder.AddProject<Projects.assignment_Client>("client")
                    .WithReference(server);

builder.Build().Run();
