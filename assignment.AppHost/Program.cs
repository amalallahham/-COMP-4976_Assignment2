using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);
var sql = builder.AddSqlServer("sql")
    .AddDatabase("sqldata");
var server = builder.AddProject<Projects.ObituaryApplication>("server")
                    .WithReference(sql);

// Optional: include the client explicitly; not required if Server has a ProjectReference to Client
var client = builder.AddProject<Projects.assignment_Client>("client")
                    .WithReference(server);

builder.Build().Run();
