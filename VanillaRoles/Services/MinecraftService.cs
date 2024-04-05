using System.Net;
using CoreRCON;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace VanillaRoles.Services;

public class MinecraftService(IConfiguration configuration)
{
    private const string MojangUsernameApiUrl = "https://api.mojang.com/users/profiles/minecraft/{username}";
    private readonly RCON _rcon = new(IPAddress.Parse(configuration["minecraft:host"]!), ushort.Parse(configuration["minecraft:port"]!), configuration["minecraft:password"]);

    public async Task AddLink(string mcUsername, string mcGroup)
    {
        Log.Information("Adding link for {mcUsername} to group {mcGroup}", mcUsername, mcGroup);
        await _rcon.SendCommandAsync($"team join {mcGroup} {mcUsername}");
        await _rcon.SendCommandAsync($"whitelist add {mcUsername}");
    }

    public async Task RemoveLink(string mcUsername)
    {
        Log.Information("Removing link for {mcUsername}", mcUsername);
        await _rcon.SendCommandAsync($"team leave {mcUsername}");
        await _rcon.SendCommandAsync($"whitelist remove {mcUsername}");
    }

    public async Task<string?> LookupUsername(string inputUsername)
    {
        try
        {
            Log.Debug("Looking up username {inputUsername} from Mojang API", inputUsername);
            var response = await MojangUsernameApiUrl
                .Replace("{username}", inputUsername)
                .GetJsonAsync<ProfileResponse>();

            Log.Debug("Got response from Mojang API: {@response}", response.Name);
            return response.Name;
        }
        catch
        {
            return null;
        }
    }
}

internal class ProfileResponse
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
}
