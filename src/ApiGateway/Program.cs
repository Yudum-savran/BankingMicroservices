using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Cache.CacheManager;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add Ocelot configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Configure JWT Authentication for API Gateway
var jwtSecret = builder.Configuration["JWT:Secret"] ?? "YourSuperSecretKeyForJWTTokenGeneration123!";
var jwtIssuer = builder.Configuration["JWT:Issuer"] ?? "BankingSystem";
var jwtAudience = builder.Configuration["JWT:Audience"] ?? "BankingClients";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
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

// Add Ocelot with Cache Manager
builder.Services
    .AddOcelot()
    .AddCacheManager(x =>
    {
        x.WithDictionaryHandle();
    });

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.UseCors("AllowAll");

// Use Ocelot middleware
await app.UseOcelot();

app.Run();
