using Microsoft.EntityFrameworkCore;

namespace VanillaRoles.Data;

public class Link
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public bool Deleted { get; set; } = false;

    public string McName { get; set; } = string.Empty;
    public string McGroup { get; set; } = string.Empty;

    public ulong DiscordId { get; set; }
}
