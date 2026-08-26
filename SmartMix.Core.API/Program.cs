using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartMix.Core.API.HealthChecks;
using SmartMix.Core.API.Middleware;
using SmartMix.Core.API.Validators;
using SmartMix.Core.Application.Batching;
using SmartMix.Core.Application.Recipes;
using SmartMix.Core.Application.Bunkers;
using SmartMix.Core.Application.Reports;
using SmartMix.Core.Application.Users;
using SmartMix.Core.Application.Licenses;
using SmartMix.Core.Contracts.BaseModels;
using SmartMix.Core.Infrastructure.Persistence.Migrations;
using System.Text;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "SmartMix.Core API", 
        Version = "v1",
        Description = "API для управления БСУ SmartMix",
        Contact = new OpenApiContact { Name = "TTS", Email = "support@tts.ru" }
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
    c.EnableAnnotations();
});

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<StartBatchRequestValidator>();

// Validation Filter
builder.Services.AddScoped<ValidationFilter>();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "SmartMixSecretKeyForDevelopmentOnly12345678901234567890";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "SmartMix.Core";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Health Checks
builder.Services.AddSmartMixHealthChecks();

// Database Migrations
builder.Services.AddMigrations(builder.Configuration);

// Register Use Cases - Batching
builder.Services.AddScoped<StartBatchUseCase>();
builder.Services.AddScoped<DoseMaterialUseCase>();
builder.Services.AddScoped<CompleteBatchUseCase>();
builder.Services.AddScoped<GetBatchStatusUseCase>();

// Register Use Cases - Recipes
builder.Services.AddScoped<CreateRecipeUseCase>();
builder.Services.AddScoped<UpdateRecipeUseCase>();
builder.Services.AddScoped<CalculateRecipeMaterialsUseCase>();

// Register Use Cases - Bunkers
builder.Services.AddScoped<SetBunkerComponentUseCase>();
builder.Services.AddScoped<ToggleBunkerUseCase>();
builder.Services.AddScoped<CalibrateBunkerUseCase>();

// Register Use Cases - Reports
builder.Services.AddScoped<GenerateConsumptionReportUseCase>();
builder.Services.AddScoped<GenerateApplicationProtocolUseCase>();

// Register Use Cases - Users
builder.Services.AddScoped<AuthenticateUserUseCase>();
builder.Services.AddScoped<ChangePasswordUseCase>();

// Register Use Cases - Licenses
builder.Services.AddScoped<ActivateLicenseUseCase>();
builder.Services.AddScoped<ValidateLicenseUseCase>();

var app = builder.Build();

// Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartMix.Core API v1");
        c.DisplayRequestDuration();
    });
}

app.UseErrorHandling(); // Global error handling

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Health Checks
app.UseSmartMixHealthChecks();

app.MapControllers();

// Run migrations on startup
if (!app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("RunMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var migrationRunner = scope.ServiceProvider.GetRequiredService<MigrationRunner>();
    await migrationRunner.RunAsync();
}

app.Run();

// Helper mapping methods
static RecipeDto MapRecipe(SmartMix.Core.Domain.Entities.Recipe r) => new()
{
    Id = r.Id,
    Name = r.Name,
    Consider = r.Consider,
    UseAutoCorrection = r.UseAutoCorrection,
    UserId = r.UserId,
    EditDate = r.EditDate,
    CategoryId = r.RecipeCategory?.Id ?? 0,
    CategoryName = r.RecipeCategory?.Name ?? string.Empty,
    TimeSetId = r.RecipeTimesSet?.Id ?? 0,
    MixerSetId = r.RecipeMixerSet?.Id ?? 0,
    Structures = r.Structures?.Select(s => new RecipeStructureDto
    {
        ComponentId = s.ComponentId,
        ComponentName = s.ComponentName,
        Percentage = s.Percentage,
        TargetWeightKg = s.TargetWeight.Kilograms,
        Correct = s.Correct
    }).ToList() ?? new()
};

static BunkerDto MapBunker(SmartMix.Core.Domain.Entities.Bunker b) => new()
{
    Number = b.Number,
    LineNumber = 1,
    IsOn = b.IsOn,
    ComponentId = b.ComponentId,
    CurrentWeightKg = b.CurrentWeightKg,
    IsActive = b.IsActive
};

static UserDto MapUser(SmartMix.Core.Domain.Entities.User u) => new()
{
    Id = u.Id,
    Username = u.Username,
    Name = u.Name,
    Role = u.Role,
    IsActive = u.IsActive
};

static LicenseInfoDto? MapLicense(SmartMix.Core.Domain.Entities.Licenses.LicenseInfo? l) => l == null ? null : new LicenseInfoDto
{
    LicenseType = l.LicenseType,
    EndDate = l.EndDate,
    SerialNumber = l.SerialNumber,
    HardwareId = l.HardwareId,
    IsActive = l.IsActive,
    ActivatedAt = l.ActivatedAt,
    MaxLines = l.MaxLines,
    MaxMixers = l.MaxMixers,
    IsPermanent = l.IsPermanent,
    IsExpired = l.IsExpired,
    TimeRemaining = l.TimeRemaining
};