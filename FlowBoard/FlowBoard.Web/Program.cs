using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FlowBoard.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ── MVC ──────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── HttpClient (IHttpClientFactory) — inter-service calls ────────────────────
builder.Services.AddHttpClient("AuthService", client =>
    client.BaseAddress = new Uri(builder.Configuration["Services:AuthService"]!));

builder.Services.AddHttpClient("WorkspaceService", client =>
    client.BaseAddress = new Uri(builder.Configuration["Services:WorkspaceService"]!));

builder.Services.AddHttpClient("BoardService", client =>
    client.BaseAddress = new Uri(builder.Configuration["Services:BoardService"]!));

builder.Services.AddHttpClient("CardService", client =>
    client.BaseAddress = new Uri(builder.Configuration["Services:CardService"]!));

builder.Services.AddHttpClient("NotificationService", client =>
    client.BaseAddress = new Uri(builder.Configuration["Services:NotificationService"]!));

builder.Services.AddHttpClient("LabelService", client =>
    client.BaseAddress = new Uri(builder.Configuration["Services:LabelService"]!));

builder.Services.AddHttpClient("CommentService", client =>
    client.BaseAddress = new Uri(builder.Configuration["Services:CommentService"]!));

// ── Dependency Injection — Service layer ──────────────────────────────────────
builder.Services.AddScoped<IAuthService,         AuthService>();
builder.Services.AddScoped<IWorkspaceService,    WorkspaceService>();
builder.Services.AddScoped<IBoardService,        BoardService>();
builder.Services.AddScoped<IListService,         ListService>();
builder.Services.AddScoped<ICardService,         CardService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ILabelService,        LabelService>();
builder.Services.AddScoped<ICommentService,      CommentService>();

// ── JWT Authentication ────────────────────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    });

builder.Services.AddAuthorization();

// ── Session (for storing JWT in server-side session) ─────────────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromHours(24);
    options.Cookie.HttpOnly    = true;
    options.Cookie.IsEssential = true;
});

// ── SignalR (real-time board updates) ────────────────────────────────────────
builder.Services.AddSignalR();

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// ── Routes ────────────────────────────────────────────────────────────────────
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=BoardView}/{action=Home}/{id?}");

// ── SignalR Hub ───────────────────────────────────────────────────────────────
app.MapHub<FlowBoard.Web.Hubs.BoardHub>("/boardHub");

app.Run();