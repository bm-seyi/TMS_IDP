using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Threading.RateLimiting;
using System.Text;
using TMS_API.Configuration;
using TMS_API.Utilities;
using TMS_API.Middleware;
using TMS_API.Models;

var builder = WebApplication.CreateBuilder(args);

// Loading Environment Variables
if (Environment.GetEnvironmentVariable("API__Key") is null)
{
    DotNetEnv.Env.Load(Path.Combine(Environment.CurrentDirectory, "Resources/.env"));
}

// Rate Limit Configuration
var myOptions = new ApiRateLimitSettings();
builder.Configuration.GetSection("ApiRateLimitSettings").Bind(myOptions);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
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


// AspNetCore Identity Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration["ConnectionStrings:Development"]));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Duende Identity Server Configuration
builder.Services.AddIdentityServer()
    .AddInMemoryClients(Clients.Get())
    .AddInMemoryApiScopes(ApiScopes.Get())
    .AddInMemoryApiResources(ApiResources.Get())
    .AddInMemoryIdentityResources(IdentityResources.Get())
    .AddAspNetIdentity<ApplicationUser>()
    .AddDeveloperSigningCredential();

// JWT Configuration
builder.Services.AddAuthentication(options => 
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new ArgumentNullException())),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});


builder.Services.AddAuthorization();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).AddEnvironmentVariables();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IDatabaseActions, DatabaseActions>();
builder.Services.AddScoped<IJwtSecurity, JwtSecurity>();
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
