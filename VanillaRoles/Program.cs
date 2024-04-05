using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using VanillaRoles;
using VanillaRoles.Data;
using VanillaRoles.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

Log.Information("Starting...");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((config) =>
        config.AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", true)
            .AddEnvironmentVariables()
            .Build()
    )
    .ConfigureServices(services =>
    {
        services.AddDbContext<DatabaseContext>(opts =>
            {
                opts.UseSqlite("Data Source=vanilla_roles.db");
            })
            .AddSingleton(Log.Logger)
            .AddHostedService<App>()
            .AddSingleton<MinecraftService>()
            .AddSingleton(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Info,
                GatewayIntents = GatewayIntents.AllUnprivileged,
            })
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(sp => new InteractionService(sp.GetRequiredService<DiscordSocketClient>().Rest));
    })
    .Build();

await host.RunAsync();
