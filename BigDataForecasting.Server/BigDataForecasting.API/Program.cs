using BigDataForecasting.API.Context.ForecastingDb;
using BigDataForecasting.API.Dtos.MLDtos;
using BigDataForecasting.API.Repositories;
using BigDataForecasting.API.Services.BaseServices.AuthServices;
using BigDataForecasting.API.Services.BaseServices.CustomerServices;
using BigDataForecasting.API.Services.BaseServices.GameServices;
using BigDataForecasting.API.Services.BaseServices.GlobeAnalyticsServices;
using BigDataForecasting.API.Services.BaseServices.SaleServices;
using BigDataForecasting.API.Services.Caching;
using BigDataForecasting.API.Services.MLServices;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ML;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using Scalar.AspNetCore;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
ThreadPool.SetMinThreads(200, 200);
// Db
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});


//Redis
var redisSection = builder.Configuration.GetSection("RedisSettings");
var redisConnectionString = redisSection["ConnectionString"] ?? "localhost:6379";

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var options = ConfigurationOptions.Parse(redisConnectionString, true);
    options.AbortOnConnectFail = false;
    options.ConnectRetry = 3;
    return ConnectionMultiplexer.Connect(options);
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
builder.Services.AddScoped<IGlobeAnalyticsService, GlobeAnalyticsManager>();

//Redis Singleton
builder.Services.AddSingleton<IRedisCachingService, RedisCacheManager>();
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
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var cookeToken = context.Request.Cookies["auth_token"];
                if (!string.IsNullOrEmpty(cookeToken)) 
                {
                    context.Token = cookeToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextjs", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001", "http://localhost:5069" ) // Next.js'in çalıştığı varsayılan port

              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Eğer ileride cookie/token kullanırsan hayat kurtarır
    });
});
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

//app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowNextjs");

app.UseAuthentication();

app.UseAuthorization();

app.UseHttpMetrics();

app.MapControllers();
//Hangfire
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();



    recurringJobManager.AddOrUpdate<IAITrainerService>(
        "AI_Model_Training_Full_Job",
        service => service.TrainAndSaveModelFromDbAsync(), 
        Cron.Daily(3,0));

    recurringJobManager.AddOrUpdate<IAITrainerService>(
         "AI_CLTV_Model_Training",
         service => service.TrainCLTVModelAsync(),
         Cron.Daily(3, 30));

    recurringJobManager.AddOrUpdate<IAITrainerService>(
        "AI_Recommendation_Model_Training",
        service => service.TrainRecommendationModelAsync(),
        Cron.Daily(4, 0));
}
app.MapMetrics();
app.Run();
