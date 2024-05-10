using Microsoft.AspNetCore.RateLimiting;
using TMS_API.Middleware;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var myOptions = new TMS_API.Middleware.RateLimiting.ApiRateLimitSettings();
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
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
DotNetEnv.Env.Load(Path.Combine(Environment.CurrentDirectory, "Resources/.env"));
app.UseHttpsRedirection();
app.UseMiddleware<ApiMiddleware>();
app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
