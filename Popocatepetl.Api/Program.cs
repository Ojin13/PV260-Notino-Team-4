using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Popocatepetl.Api.Services;
using Popocatepetl.Application.Common;
using Popocatepetl.Host;
using Popocatepetl.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Popocatepetl API (dev/testing)",
        Version = "v1",
        Description = """
            This is a temporary testing interface.
            All endpoints map directly to application use cases.
            Set X-User-Email and X-User-Role headers to simulate different users.
            Admin password enforcement is skipped in this layer —
            the auth pipeline only runs when the CLI is active.
            """,
    });

    // Make both identity headers appear as lock icons on every endpoint.
    options.AddSecurityDefinition("EmailHeader", new OpenApiSecurityScheme
    {
        Name = "X-User-Email",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Set your email address for audit logging",
    });

    options.AddSecurityDefinition("RoleHeader", new OpenApiSecurityScheme
    {
        Name = "X-User-Role",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Set role: Admin, PowerUser, or User",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "EmailHeader",
                },
            },
            []
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "RoleHeader",
                },
            },
            []
        },
    });

});

builder.Services.AddHttpContextAccessor();
builder.Services.AddHostServices(builder.Configuration);

builder.Services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PopocatepetlDbContext>();
    await db.Database.MigrateAsync();
}

// Middleware pipeline
app.UseSwagger();
app.UseSwaggerUI(ui =>
{
    ui.SwaggerEndpoint("/swagger/v1/swagger.json", "Popocatepetl API v1");
    ui.RoutePrefix = "swagger";
});

app.MapControllers();
app.Run();

public partial class Program;
