using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Transaction.Infrastructure.Persistence;
using Transaction.Infrastructure.Persistence.Repositories;
using Transaction.Domain.Repositories;
using Transaction.Application.EventHandlers;
using Transaction.Infrastructure.MessageBus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<TransactionDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repositories
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Event Handlers
builder.Services.AddScoped<AccountEventHandler>();

// MediatR
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(Transaction.Application.Queries.GetAccountTransactionsQuery).Assembly));

// JWT Authentication
var jwtSecret = builder.Configuration["JWT:Secret"];
var jwtIssuer = builder.Configuration["JWT:Issuer"];
var jwtAudience = builder.Configuration["JWT:Audience"];

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
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret ?? ""))
        };
    });

// RabbitMQ
var rabbitMQHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
var rabbitMQUser = builder.Configuration["RabbitMQ:Username"] ?? "admin";
var rabbitMQPass = builder.Configuration["RabbitMQ:Password"] ?? "Admin123!";

builder.Services.AddSingleton<RabbitMQEventBus>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<RabbitMQEventBus>>();
    return new RabbitMQEventBus(rabbitMQHost, rabbitMQUser, rabbitMQPass, logger, sp);
});

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString ?? "");

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Auto-migrate database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
    db.Database.Migrate();
}

// Subscribe to RabbitMQ events
var eventBus = app.Services.GetRequiredService<RabbitMQEventBus>();
eventBus.SubscribeToAccountEvents();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Logger.LogInformation("Transaction Service started");

app.Run();
