using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Threading.RateLimiting;
using TMS_API.Configuration;
using TMS_API.Utilities;
using TMS_API.Middleware;
using TMS_API.DbContext;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Loading Environment Variables
if (Environment.GetEnvironmentVariable("API__Key") is null)
{
    DotNetEnv.Env.Load(Path.Combine(Environment.CurrentDirectory, "Resources/.env"));
}

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).AddEnvironmentVariables();
// Add services to the container.

string connectionString = builder.Configuration["ConnectionStrings:Development"] ?? throw new ArgumentNullException(nameof(connectionString));

// Rate Limit Configuration
var myOptions = new ApiRateLimitSettings();
builder.Configuration.GetSection("ApiRateLimitSettings").Bind(myOptions);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = (int)HttpStatusCode.TooManyRequests;
    options.AddTokenBucketLimiter(policyName: "TokenPolicy", configureOptions: tokenOptions =>
    {
        tokenOptions.TokenLimit = myOptions.TokenLimit; 
        tokenOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; 
        tokenOptions.QueueLimit = myOptions.QueueLimit;
        tokenOptions.ReplenishmentPeriod = TimeSpan.FromSeconds(myOptions.ReplenishmentPeriod);
        tokenOptions.TokensPerPeriod = myOptions.TokensPerPeriod;
        tokenOptions.AutoReplenishment = myOptions.AutoReplenishment;
    });
});


// AspNetCore Identity DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

// Duende Configuration DbContext
builder.Services.AddDbContext<ConfigurationDbContext>(options => options.UseSqlServer(connectionString));

// Duende Persisted DbContext
builder.Services.AddDbContext<PersistedGrantDbContext>(options => options.UseSqlServer(connectionString));

// AspNetCore Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Duende Identity Server Configuration
var migrationsAssembly = typeof(Program).Assembly.GetName().Name;
builder.Services.AddIdentityServer()
    .AddConfigurationStore(options =>
    {
        options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
        sql => sql.MigrationsAssembly(migrationsAssembly));
    })
    .AddOperationalStore(options =>
    {
        options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
        sql => sql.MigrationsAssembly(migrationsAssembly));
    });


builder.Services.AddAuthentication()
    .AddJwtBearer(options => 
    {
        options.Authority = "http://localhost:5188";
        options.TokenValidationParameters.ValidateAudience = false;
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IDatabaseActions, DatabaseActions>();
builder.Services.AddTransient<ISecurityUtils, SecurityUtils>();

// Ensure logging services are added
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();
app.UseHttpsRedirection();
app.UseMiddleware<ApiMiddleware>();
app.UseRateLimiter();
app.UseIdentityServer();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();  
app.UseAuthorization();
IConfiguration configuration = app.Configuration;
IWebHostEnvironment environment = app.Environment;

app.MapControllers();

app.Run();
