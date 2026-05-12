using System.Security.Claims;
using System.Text;
using FlowBoard.Notification.Consumers;
using FlowBoard.Notification.Exceptions;
using FlowBoard.Notification.Hubs;
using FlowBoard.Notification.Infrastructure;
using FlowBoard.Notification.Repositories;
using FlowBoard.Notification.Services;
using FlowBoard.Notification.Workers;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Quartz;
using SendGrid.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine(">>> FLOWBOARD NOTIFICATION SERVICE STARTING <<<");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ── Swagger with JWT ──────────────────────────────────────────────────────────
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title   = "FlowBoard Notification Service",
        Version = "v1"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter JWT token only. Do NOT include Bearer prefix."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── SendGrid ──────────────────────────────────────────────────────────────────
builder.Services.AddSendGrid(options =>
    options.ApiKey = builder.Configuration["SendGrid:ApiKey"]
                     ?? throw new InvalidOperationException("SendGrid:ApiKey is not configured."));

// ── SignalR ───────────────────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ── MassTransit + RabbitMQ ────────────────────────────────────────────────────
builder.Services.AddMassTransit(x =>
{
    // Consumers — events arriving from other services
    x.AddConsumer<CardAssignedConsumer>();
    x.AddConsumer<MentionConsumer>();
    x.AddConsumer<CommentReplyConsumer>();
    x.AddConsumer<CardMovedToDoneConsumer>();
    x.AddConsumer<DueDateReminderConsumer>();
    x.AddConsumer<BulkNotificationConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });

        cfg.ConfigureEndpoints(ctx);
    });
});

// ── Quartz.NET — due-date reminders ───────────────────────────────────────────
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    var jobKey = new JobKey("DueDateReminderJob");
    q.AddJob<DueDateReminderJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("DueDateReminderTrigger")
        // Fire every 30 minutes so we catch the ~1-day and ~1-hour windows
        .WithCronSchedule("0 0/30 * * * ?"));
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// ── HTTP client for Task-Service card lookup (due-date job) ───────────────────
builder.Services.AddHttpClient<IDueDateCardProvider, HttpDueDateCardProvider>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Services:TaskService"] ?? "http://localhost:5005/");
});

// ── DI ────────────────────────────────────────────────────────────────────────
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationServiceImpl>();

// ── JWT Auth ──────────────────────────────────────────────────────────────────
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var key = Encoding.UTF8.GetBytes("FlowBoardProjectSecretKey2026!!!!");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(key),
        ValidateIssuer           = false,
        ValidateAudience         = false,
        ValidateLifetime         = false,
        RequireExpirationTime    = false,
        NameClaimType            = ClaimTypes.NameIdentifier,
        RoleClaimType            = ClaimTypes.Role
    };

    // SignalR sends JWT via query string — extract it here
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path        = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/hubs/notifications"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"[AUTH ERROR] {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("[AUTH SUCCESS] Token is valid!");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("ADMIN", "SUPER_ADMIN"));
});
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// ── DB Migration ──────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    db.Database.Migrate();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ── SignalR Hub endpoint ───────────────────────────────────────────────────────
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();