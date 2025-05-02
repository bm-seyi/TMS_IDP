using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Threading.RateLimiting;
using System.Net;
using StackExchange.Redis;
using TMS_IDP.Utilities;
using TMS_MIGRATE.DbContext;
using TMS_IDP.Models.RateLimit;

var builder = WebApplication.CreateBuilder(args);

// Loading Environment Variables
#if DEBUG
string? parentDirectory = Directory.GetParent(Environment.CurrentDirectory)?.FullName;
if (string.IsNullOrWhiteSpace(parentDirectory))
{
    throw new DirectoryNotFoundException("Parent directory not found.");
}
DotNetEnv.Env.Load(Path.Combine(parentDirectory, ".env"));
#endif

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).AddEnvironmentVariables();

string connectionString = builder.Configuration["ConnectionStrings:Development"] ?? throw new ArgumentNullException(nameof(connectionString));
string redisPassword = builder.Configuration["Redis:Password"] ?? throw new ArgumentNullException(nameof(redisPassword));
string vaultToken = builder.Configuration["HashiCorp:Vault:Token"] ?? throw new ArgumentNullException(nameof(vaultToken));

// Rate Limit Configuration
tokenBucketRateLimitOptions myOptions = new tokenBucketRateLimitOptions();
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
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid, ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin, ApplicationUserToken, ApplicationRoleClaim>>()
    .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid, ApplicationUserRole, ApplicationRoleClaim>>();

// Duende Identity Server Configuration
builder.Services.AddIdentityServer(options =>
{
    options.UserInteraction.LoginUrl = "/auth/login";
    options.UserInteraction.LogoutUrl = "/auth/logout";
    options.UserInteraction.CreateAccountUrl = "/auth/register";
})
.AddAspNetIdentity<ApplicationUser>()
.AddConfigurationStore(options =>
{
    options.EnablePooling = true;
    options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
    sql => sql.MigrationsAssembly(typeof(ConfigurationDbContext).Assembly.FullName));
})
.AddOperationalStore(options =>
{
    options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
    sql => sql.MigrationsAssembly(typeof(PersistedGrantDbContext).Assembly.FullName));   
    options.EnablePooling = true;
    options.EnableTokenCleanup = true;
    options.TokenCleanupInterval = 600;
    options.TokenCleanupBatchSize = 200;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/auth/login";
    options.LogoutPath = "/auth/logout";
    options.AccessDeniedPath = "/auth/access-denied"; // TODO: Create this page
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
})
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = "https://localhost:5188";
    options.ClientId = "maui_client";
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidateAudience = true,
        ClockSkew = TimeSpan.FromMinutes(5),
    };

    options.Events = new OpenIdConnectEvents
    {
        OnAuthenticationFailed = context => 
        {
            context.Response.Redirect("/error");
            context.HandleResponse();
            return Task.CompletedTask;
        },
    };
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/auth/Login";
    options.LogoutPath = "/auth/Logout";
    options.AccessDeniedPath = "/auth/AccessDenied"; // TODO: Create this page
});

// Add Session services to the container
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.ConfigurationOptions = new ConfigurationOptions
    {
        EndPoints = { "localhost:6380" },
        AbortOnConnectFail = false,
        ConnectTimeout = 1000,
        SyncTimeout = 1000,
        AllowAdmin = true,
        Password = redisPassword
    };
}); // Required for session state

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

ICertificateService certificateService = new CertificateService(new HttpClient());
IConnectionMultiplexer redis = ConnectionMultiplexer.Connect($"localhost:6379,password={redisPassword}");
await builder.Services.AddDataProtectionAsync(certificateService, redis);

builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Ensure logging services are added
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseIdentityServer();
app.UseAuthentication();  
app.UseAuthorization();
app.UseSession();

IConfiguration configuration = app.Configuration;
IWebHostEnvironment environment = app.Environment;

app.MapControllers();

app.Run();
