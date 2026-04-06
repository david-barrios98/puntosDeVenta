using AspNetCoreRateLimit;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using puntosDeVenta.Api.Extensions;
using puntosDeVenta.Shared.Helper;
using puntosDeVenta.Core;
using puntosDeVenta.Infrastructure.Seed;
using System.Net;
using System.Text;
using puntosDeVenta.Infrastructure.Persistence.Adapters;
using puntosDeVenta.Api.Middleware;
using NLog.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var environment = builder.Environment.EnvironmentName;

// ============== CONFIGURACIÓN ==============
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// ============== BASE DE DATOS ==============
builder.Services.AddDbContext<puntosDeVentaDbContext>(options =>
    options.UseSqlServer(
        configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.CommandTimeout(30).EnableRetryOnFailure(3)));

// ============== JWT ==============
var jwtSettings = configuration.GetSection("JwtSettings");
ValidateJwtSettings(jwtSettings);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            context.HandleResponse();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Unauthorized - Token missing or invalid",
                timestamp = DateTime.UtcNow,
                traceId = context.HttpContext.TraceIdentifier
            });
        }
    };

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"])),
        ClockSkew = TimeSpan.Zero
    };
});

// ============== LOGGING ==============
builder.Logging.ClearProviders();
builder.Logging.AddNLog("nlog.config");

// ============== CORS ==============
var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:4202" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("_myAllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .WithMethods("PUT", "DELETE", "GET", "POST")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ============== RATE LIMITING ==============
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddInMemoryRateLimiting();

// ============== HEXAGONAL ARCHITECTURE ==============
builder.Services.AddSingleton<JwtService>();
builder.Services.AddApplicationUseCases();
builder.Services.AddApplicationPorts(configuration);
builder.Services.AddApplicationValidators();

// ============== CONTROLLERS ==============
builder.Services.AddControllers(options =>
{
    // Filtros globales si es necesario
});
builder.Services.AddEndpointsApiExplorer();

// ============== SWAGGER ==============
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "puntosDeVenta API",
        Version = "v1",
        Description = "API con Arquitectura Hexagonal - Autenticación JWT",
        Contact = new OpenApiContact { Name = "puntosDeVenta Team", Email = "tech@puntosDeVenta.com" },
        License = new OpenApiLicense { Name = "Proprietary" }
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "JWT Bearer token",
        In = ParameterLocation.Header
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });


    //var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    //if (File.Exists(xmlPath))
    //    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// ============== SEED DATABASE ==============
var runSeed = configuration.GetValue<bool>("RunSeed");
if (runSeed)
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<puntosDeVentaDbContext>();
        await DbInitializer.SeedAsync(context);
    }
}

// ============== MIDDLEWARE ==============
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "puntosDeVenta API v1");
        options.RoutePrefix = "swagger";
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("_myAllowSpecificOrigins");
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseIpRateLimiting();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapControllers();
app.Run();

// ============== VALIDACIÓN ==============
static void ValidateJwtSettings(IConfigurationSection jwtSettings)
{
    if (string.IsNullOrWhiteSpace(jwtSettings["SecretKey"]))
        throw new InvalidOperationException("JwtSettings:SecretKey no configurado");
    if (string.IsNullOrWhiteSpace(jwtSettings["Issuer"]))
        throw new InvalidOperationException("JwtSettings:Issuer no configurado");
    if (string.IsNullOrWhiteSpace(jwtSettings["Audience"]))
        throw new InvalidOperationException("JwtSettings:Audience no configurado");
}
