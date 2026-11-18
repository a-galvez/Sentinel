using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Sentinel.API.Middleware;
using Sentinel.API.Modules;
using Sentinel.Application;
using Sentinel.Infrastructure;
using Sentinel.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
Env.Load();

// Las variables pueden venir de .env o del entorno real (Render, Docker).
builder.Configuration.AddEnvironmentVariables();

var connectionString = builder.Configuration.GetConnectionString("PostgresConnection")
                       ?? builder.Configuration["ConnectionStrings__PostgresConnection"]
                       ?? throw new InvalidOperationException("Falta la cadena de conexión a PostgreSQL.");

var corsAllowedOrigins = builder.Configuration["CORS_ALLOWED_ORIGINS"]
                         ?? throw new InvalidOperationException("Falta la variable CORS_ALLOWED_ORIGINS en el archivo .env.");

var jwtSecret = builder.Configuration["Jwt:Secret"]
               ?? builder.Configuration["JWT__Secret"]
               ?? throw new InvalidOperationException("Falta la variable JWT__Secret.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? builder.Configuration["JWT__Issuer"] ?? "sentinel";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? builder.Configuration["JWT__Audience"] ?? "sentinel-web";

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PoliticaCorsSentinel", policy =>
    {
        var origenes = corsAllowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        policy.WithOrigins(origenes)
              .SetIsOriginAllowedToAllowWildcardSubdomains() // Habilita comodines de subdominio (ej. https://*.unah.edu.hn)
              .AllowAnyHeader()                              // Autoriza encabezados JSON y futuros Tokens JWT
              .AllowAnyMethod();                             // Autoriza verbos GET y Preflights (POST, PUT, DELETE)
    });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

builder.Services
    .AddAuthModule(builder.Configuration)
    .AddRegistroModule()
    .AddProductosModule()
    .AddCatalogosModule()
    .AddEstablecimientosModule()
    .AddLicenciasModule()
    .AddAlertasModule()
    .AddRequisitosModule()
    .AddExpedienteModule()
    .AddDashboardModule();

var app = builder.Build();

// Migraciones al arrancar (P1). En producción se controla con MIGRATE_ON_START.
if (builder.Configuration.GetValue("MIGRATE_ON_START", app.Environment.IsDevelopment()))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors("PoliticaCorsSentinel");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
