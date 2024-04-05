using Discord.Interactions;

namespace VanillaRoles.Commands;

public class Ping : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Get a pong")]
    public async Task PongSubcommand()
        => await RespondAsync("Pong!");
}
