using Microsoft.OpenApi.Models;
using Rogbau.Api.Common.Extensions;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.MapType<DateOnly>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "date"
    });

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Wklej sam token JWT"
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
});

// Database + Identity
builder.Services.AddDatabaseAndIdentity(
    builder.Configuration);

// JWT
builder.Services.AddJwtAuthentication(
    builder.Configuration);

// Application services
builder.Services.AddApplicationServices();

// SignalR
builder.Services.AddSignalR();

// CORS dla Reacta
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Seed bazy
await DbInitializer.SeedAsync(app.Services);

// Swagger tylko Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global exception handler
app.UseGlobalExceptionHandler();

// Tunel zapewnia HTTPS na publicznym adresie; lokalnie łączy się przez HTTP.
if (!app.Environment.IsEnvironment("Tunnel"))
{
    app.UseHttpsRedirection();
}

app.UseDefaultFiles();
app.UseStaticFiles();

// CORS musi być przed auth/endpoints
app.UseCors("ReactPolicy");

// Authentication
app.UseAuthentication();

// Authorization
app.UseAuthorization();

// Controllers
app.MapControllers();

// SignalR Hub
app.MapHub<NotificationHub>(
    "/hubs/notifications");

// React obsługuje swoje adresy, np. /sites i /transfers/{id}.
app.MapFallbackToFile("index.html");

app.Run();
