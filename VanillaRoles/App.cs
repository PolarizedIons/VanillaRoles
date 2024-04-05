using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using VanillaRoles.Data;

namespace VanillaRoles;

public class App(DiscordSocketClient client, InteractionService interactionService, IConfiguration config, IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<DatabaseContext>().Database.MigrateAsync(cancellationToken: cancellationToken);
        }

        client.Log += async msg =>
        {
            await Task.CompletedTask;
            Log.Information("{Source}: {Message}", msg.Source, msg.Message);
        };

        client.Ready += async () =>
        {
            await (await client.Rest.GetGuildAsync(382135773073375232)).DeleteSlashCommandsAsync();
#if DEBUG
            await interactionService.RegisterCommandsToGuildAsync(382135773073375232);
#else
            await client.Rest.DeleteAllGlobalCommandsAsync();
            await interactionService.RegisterCommandsGloballyAsync();
#endif
            Log.Information("Bot is ready");
        };

        client.InteractionCreated += async interaction =>
        {
            var ctx = new SocketInteractionContext(client, interaction);
            await interactionService.ExecuteCommandAsync(ctx, serviceProvider);
        };

        await client.LoginAsync(TokenType.Bot, config["bot:token"]);

        await interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);

        await client.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await client.StopAsync();
    }
}
