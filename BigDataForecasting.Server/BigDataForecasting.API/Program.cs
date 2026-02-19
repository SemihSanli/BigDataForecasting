using BigDataForecasting.API.Context.ForecastingDb;
using BigDataForecasting.API.Repositories;
using BigDataForecasting.API.Services.BaseServices.AuthServices;
using BigDataForecasting.API.Services.BaseServices.GameServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
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
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ITokenService, TokenManager>();
builder.Services.AddScoped<IAuthService, AuthManager>();
builder.Services.AddScoped<IGameService, GameManager>();

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

var app = builder.Build();

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

        // ? Scalar hangi OpenAPI route'undan çekecek
        options.OpenApiRoutePattern = "/openapi/{documentName}.json";
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
