using MetalSavingsManager.Data;
using MetalSavingsManager.Services;
using MetalSavingsManager.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Read WorkDir from configuration
var workDir = builder.Configuration["WorkDir"];
if (string.IsNullOrWhiteSpace(workDir))
{
    throw new Exception("WorkDir configuration missing in appsettings.json");
}

// Ensure WorkDir directory exists
if (!Directory.Exists(workDir))
{
    Directory.CreateDirectory(workDir);
}

// Build full DB path
var dbPath = Path.Combine(workDir, "MetalSavingsManager.db");

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});


builder.Services.AddHttpClient();
builder.Services.AddScoped<IDepositService, DepositService>();
builder.Services.AddScoped<IMetalPriceService, MetalPriceService>();
builder.Services.AddScoped<ISavingsPlanService, SavingsPlanService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IQuarterlyFeeService, QuarterlyFeeService>();

builder.Services.AddHostedService<QuarterlyFeeWorker>();
builder.Services.AddHostedService<MonthlyDepositService>();
builder.Services.AddScoped<MonthlyDepositService>();

builder.Services.AddScoped<IAuthService, AuthService>();
// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter JWT Bearer token only",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement{
        {
            new OpenApiSecurityScheme{
                Reference = new OpenApiReference{
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});

// Configure DbContext with full path
builder.Services.AddDbContext<BankingDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}")
    .EnableSensitiveDataLogging()       // show parameter values
    .LogTo(Console.WriteLine, LogLevel.Information));

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:4200")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);
var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)

.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero // optional: eliminates default delay time tolerance
    };
});


// Build the app
var app = builder.Build();
var runningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

// Before the app runs, ensure the DB is created if it doesn't exist (and folder is ready)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
    // This will ensure DB file and tables are created without applying migrations
    var path = db.Database.GetDbConnection().ConnectionString;
    Console.WriteLine($"Using DB at: {path}");
    db.Database.EnsureCreated();
}

app.UseRouting();
app.UseCors(MyAllowSpecificOrigins);


// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


if (app.Environment.IsDevelopment() && !runningInContainer)
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();