using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using VanillaRoles.Data;
using VanillaRoles.Services;

namespace VanillaRoles.Commands;


public class LinkRole(IConfiguration config, DatabaseContext db, MinecraftService mc) : InteractionModuleBase<SocketInteractionContext>
{
    private const string ModalId = "link_modal";

    private readonly Dictionary<ulong, string> _requiredRoles = config["vanilla_roles:required_roles"]!
        .Split(",")
        .Select(x =>
        {
            var split = x.Split(":");
            return new { role = ulong.Parse(split[0]), group = split[1] };
        })
        .ToDictionary(x => x.role, x => x.group);

    [SlashCommand("link", "Link your discord to the vanilla server")]
    public async Task Command()
    {
        var user = Context.User as SocketGuildUser;
        Log.Debug("{User} is trying to link an account", user);
        if (user!.Roles.All(x => !_requiredRoles.ContainsKey(x.Id)))
        {
            Log.Debug("{User} failed the role check", user);
            await Context.Interaction.RespondAsync("You are not allowed to use this command.", ephemeral: true);
            return;
        }

        await Context.Interaction.RespondWithModalAsync<LinkModal>(ModalId);
    }

    public class LinkModal : IModal
    {
        public string Title => "Link MC <> Discord";

        [InputLabel("Minecraft Username")]
        [ModalTextInput("mc_username", placeholder: "Enter your Minecraft username")]
        public string McUsername { get; set; } = string.Empty;
    }

    [ModalInteraction(ModalId)]
    public async Task DoLink(LinkModal modal)
    {
        Log.Debug("Received request from modal for {McUser} for {DiscordUser}", modal.McUsername, Context.User);
        var mcUsername = await mc.LookupUsername(modal.McUsername);
        if (mcUsername is null)
        {
            Log.Debug("Invalid Minecraft username {McUser}", modal.McUsername);
            await Context.Interaction.RespondAsync("Invalid Minecraft username.", ephemeral: true);
            return;
        }

        var mcGroup = ((SocketGuildUser) Context.User).Roles
            .Where(x => _requiredRoles.ContainsKey(x.Id))
            .Select(x => _requiredRoles[x.Id])
            .First();

        var existingClaims = await db.Links
            .Where(x =>
                x.McName.ToLower() == mcUsername.ToLower()
                && !x.Deleted
            )
            .AnyAsync();

        if (existingClaims)
        {
            Log.Debug("Username {McUser} is already linked to a discord account", modal.McUsername);
            await Context.Interaction.RespondAsync($"Username {mcUsername} is already linked to a discord account.", ephemeral: true);
            return;
        }

        var existingLinks = await db.Links
            .Where(x =>
                x.DiscordId == Context.User.Id
                && !x.Deleted
            )
            .ToListAsync();

        foreach (var existingLink in existingLinks)
        {
            existingLink.Deleted = true;
        }

        db.Add(new Link
        {
            Id = Guid.NewGuid(),
            DiscordId = Context.User.Id,
            McName = mcUsername,
            McGroup = mcGroup,
        });

        foreach (var oldMcName in existingLinks.Select(x => x.McName).Distinct())
        {
            await mc.RemoveLink(oldMcName);
        }
        await mc.AddLink(mcUsername, mcGroup);

        await db.SaveChangesAsync();

        Log.Information("Linked {McUser} to {DiscordUser}", mcUsername, Context.User);
        await Context.Interaction.RespondAsync($"Linked `{mcUsername}` to {Context.User.Mention}", allowedMentions: AllowedMentions.None);
    }
}
