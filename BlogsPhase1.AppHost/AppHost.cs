var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Blogs_API>("blogs-api");

builder.Build().Run();
