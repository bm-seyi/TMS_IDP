using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using IdServer = Duende.IdentityServer.EntityFramework.DbContexts;
using TMS_SEED.Models;
using TMS_SEED.Utilities;
using TMS_MIGRATE.DbContext;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        // Optional: extra config setup if you need
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        IConfiguration configuration = context.Configuration;

        string? connectionString = configuration.GetConnectionString("ConfigurationDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.WriteLine("Connection string 'ConfigurationDb' not found in appsettings.json or environment variables.");
            Environment.ExitCode = 1;
            return;
        }

        services.Configure<ConfigurationStoreOptions>(options =>
        {
            options.DefaultSchema = "dbo";
        });

        services.AddDbContext<ConfigurationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddIdentityServer()
        .AddConfigurationStore(options =>
        {
            options.EnablePooling = true;
            options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
            sql => sql.MigrationsAssembly(typeof(Program).Assembly.GetName().Name));
        });

        // Register your Seeder class
        services.AddTransient<ISeeder, Seeder>();

        IdentityServerModel identityServerModel = new IdentityServerModel();
        configuration.GetSection("IdentityServer").Bind(identityServerModel);
        services.AddSingleton(identityServerModel);
    })
    .Build();

using (var scope = host.Services.CreateAsyncScope())
{
    IServiceProvider services = scope.ServiceProvider;

    try
    {
        var seeder = services.GetRequiredService<Seeder>();
        await seeder.SeedAsync();
        Console.WriteLine("Database seeding completed successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
        }
        Console.WriteLine(ex.StackTrace);
        Environment.ExitCode = 1;
    }
}