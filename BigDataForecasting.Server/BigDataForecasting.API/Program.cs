using BigDataForecasting.API.Context.ForecastingDb;
using BigDataForecasting.API.Dtos.MLDtos;
using BigDataForecasting.API.Repositories;
using BigDataForecasting.API.Services.BaseServices.AuthServices;
using BigDataForecasting.API.Services.BaseServices.CustomerServices;
using BigDataForecasting.API.Services.BaseServices.GameServices;
using BigDataForecasting.API.Services.BaseServices.SaleServices;
using BigDataForecasting.API.Services.MLServices;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ML;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Db
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// DI
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));
builder.Services.AddHangfireServer();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ITokenService, TokenManager>();
builder.Services.AddScoped<IAuthService, AuthManager>();
builder.Services.AddScoped<IGameService, GameManager>();
builder.Services.AddScoped<ISaleService, SaleManager>();
builder.Services.AddScoped<ICustomerService, CustomerManager>();
builder.Services.AddScoped<IAITrainerService, AITrainerManager>();

// Auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// ? Built-in OpenAPI (transformer YOK)
builder.Services.AddOpenApi();

builder.Services.AddPredictionEnginePool<CustomerChurnInput, CustomerChurnPrediction>()
    .FromFile(modelName: "ChurnModel", filePath: "ChurnModel.zip", watchForChanges: true);

var app = builder.Build();

app.UseHangfireDashboard("/hangfire");

if (app.Environment.IsDevelopment())
{
    // ? OpenAPI JSON endpoint: Scalar 'v1' doc name ister
    app.MapOpenApi("/openapi/{documentName}.json");

    // ? Scalar UI
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Big Data Forecasting API")
            .WithTheme(ScalarTheme.Moon)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

        // ? Scalar hangi OpenAPI route'undan �ekecek
        options.OpenApiRoutePattern = "/openapi/{documentName}.json";
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();



    recurringJobManager.AddOrUpdate<IAITrainerService>(
        "AI_Model_Training_Full_Job",
        service => service.TrainAndSaveModelFromDbAsync(), 
        Cron.Daily(3));
}
app.Run();
