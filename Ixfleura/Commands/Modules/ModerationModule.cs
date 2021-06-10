using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using Ixfleura.Commands.Checks;
using Qmmands;

namespace Ixfleura.Commands.Modules
{
    [RequireModOrAdmin]
    public class ModerationModule : DiscordGuildModuleBase
    {
        [Command("kick", "boot")]
        [Description("Kick a user from the server")]
        [RequireBotGuildPermissions(Permission.KickMembers)]
        public async Task<DiscordCommandResult> KickAsync(
            IMember member,
            [Remainder] string reason = null)
        {
            var reqOptions = new DefaultRestRequestOptions
            {
                Reason = reason ?? $"Kicked by action of {Context.Author.Tag}"
            };

            await member.KickAsync(reqOptions);

            return Response($"Successfully kicked {member.Tag}");
        }
        
        [Command("kick", "boot")]
        [Description("Kick a user from the server")]
        [RequireBotGuildPermissions(Permission.KickMembers)]
        public async Task<DiscordCommandResult> KickAsync(
            ulong id,
            [Remainder] string reason = null)
        {
            var reqOptions = new DefaultRestRequestOptions
            {
                Reason = reason ?? $"Kicked by action of {Context.Author.Tag}"
            };
            
            await Context.Guild.KickMemberAsync(new Snowflake(id), reqOptions);

            return Response("Successfully kicked that user");
        }

        [Command("ban", "hammer")]
        [Description("Ban a user from the server")]
        [RequireBotGuildPermissions(Permission.BanMembers)]
        public async Task<DiscordCommandResult> BanAsync(
            IMember member, 
            [Remainder] string reason = null)
        {
            await member.BanAsync(reason ?? $"Banned by action of {Context.Author.Tag}");
            return Response($"Successfully banned {member.Tag}");
        }
        
        [Command("ban", "hammer")]
        [Description("Ban a user from the server")]
        [RequireBotGuildPermissions(Permission.BanMembers)]
        public async Task<DiscordCommandResult> BanAsync(
            ulong id, 
            [Remainder] string reason = null)
        {
            await Context.Guild.CreateBanAsync(new Snowflake(id), reason ?? $"Banned by action of {Context.Author.Tag}");
            return Response("Successfully banned that user");
        }
    }
}