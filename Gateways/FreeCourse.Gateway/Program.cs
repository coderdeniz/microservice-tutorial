using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddJwtBearer("GatewayAuthenticationScheme",
    opt =>
    {
        opt.Authority = builder.Configuration["IdentityServerUrl"];
        opt.Audience = "resource_gateway";
        opt.RequireHttpsMetadata = false;
    });


builder.Services.AddOcelot();

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
    .AddJsonFile($"configuration.{builder.Environment.EnvironmentName}.json")
    .AddEnvironmentVariables();

var app = builder.Build();

await app.UseOcelot();

app.Run();
