using backendLibraryManagement.Data;
using backendLibraryManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add MVC Controllers + Swagger (for API documentation & testing)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register application-level services
builder.Services.AddScoped<NotificationService>();
builder.Services.Configure<ReminderOptions>(builder.Configuration.GetSection("Reminder"));
builder.Services.AddHostedService<DueReminderWorker>(); // Background job for due reminders

// JWT Authentication Configuration
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"];
var jwtIssuer = jwtSection["Issuer"];
var jwtAudience = jwtSection["Audience"];
var jwtExpireMinutes = int.TryParse(jwtSection["ExpireMinutes"], out var m) ? m : 60;

// Email Service Configuration (loaded from appsettings.json)
// Example appsettings:
// "EmailSettings": {
//   "Host": "smtp.gmail.com",
//   "Port": "587",
//   "From": "myemail@gmail.com",
//   "Password": "mypassword"
// }
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>().GetSection("EmailSettings");
    return new EmailService(
        config["Host"],
        int.Parse(config["Port"]),
        config["From"],
        config["Password"]
    );
});

// Validate configuration
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Jwt:Key is not configured in appsettings.json");

// Register JWT Bearer Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Allow tokens without HTTPS during development
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    // Token validation rules
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // No delay in token expiration
    };
});

// CORS Configuration (allows Angular frontend to access API)
builder.Services.AddCors(option =>
{
    option.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200", // Local Angular dev server
            "http://192.168.0.208:4200" // LAN device
            )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// Database Context Configuration
// If DefaultConnection exists → SQL Server
// Otherwise fallback to in-memory for quick development.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<LibraryContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    builder.Services.AddDbContext<LibraryContext>(options =>
        options.UseInMemoryDatabase("LibraryDb"));
}

// Register domain services (DI container)
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<LoanService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ReservationService>();
builder.Services.AddScoped<NotificationService>();
var app = builder.Build();

// Development-only middleware (Swagger UI)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global Middleware Pipeline
app.UseHttpsRedirection();

// Allow CORS requests (must come before auth)
app.UseCors("AllowFrontend");

// Enable JWT authentication & authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controller endpoints
app.MapControllers();

// Start application
app.Run();

