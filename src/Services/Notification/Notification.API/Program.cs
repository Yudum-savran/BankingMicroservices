using Notification.API.Services;
using Notification.API.EventHandlers;
using Notification.API.MessageBus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<AccountEventHandler>();

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
builder.Services.AddHealthChecks();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Subscribe to RabbitMQ events
var eventBus = app.Services.GetRequiredService<RabbitMQEventBus>();
eventBus.SubscribeToAccountEvents();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.MapControllers();
app.MapHealthChecks("/health");

app.Logger.LogInformation("Notification Service started");

app.Run();
