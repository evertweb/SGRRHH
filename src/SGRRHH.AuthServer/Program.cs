using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;
using SGRRHH.Infrastructure.Firebase;
using SGRRHH.AuthServer;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

// Add caching for sessions
builder.Services.AddMemoryCache();

// Firebase initializer (uses SGRRHH.Infrastructure.Firebase.FirebaseInitializer)
builder.Services.AddSingleton<FirebaseInitializer>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>().GetSection("Firebase").Get<SGRRHH.Infrastructure.Firebase.FirebaseConfig>()
                 ?? throw new InvalidOperationException("Firebase config not found");
    return new FirebaseInitializer(config, sp.GetService<Microsoft.Extensions.Logging.ILogger<FirebaseInitializer>>());
});

// Session service
builder.Services.AddScoped<FirebaseSessionService>();

// Add CORS for local dev (adjust origin as needed)
builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalDev", policy =>
    {
        policy.WithOrigins("http://localhost:5103")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Initialize Firebase at startup
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<FirebaseInitializer>();
    var initTask = initializer.InitializeAsync();
    initTask.GetAwaiter().GetResult();
}

app.UseCors("LocalDev");

app.MapControllers();

app.Run();

// Public types
public partial class Program { }
