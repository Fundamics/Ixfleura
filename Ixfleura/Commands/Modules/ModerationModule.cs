using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using Ixfleura.Commands.Checks;
using Ixfleura.Common.Types;
using Ixfleura.Services;
using Qmmands;

namespace Ixfleura.Commands.Modules
{
    [RequireModOrAdmin]
    public class ModerationModule : DiscordGuildModuleBase
    {
        private readonly ModerationService _moderationService;
        public ModerationModule(ModerationService moderationService)
        {
            _moderationService = moderationService;
        }
        
        [Command("kick", "boot")]
        [Description("Kick a user from the server")]
        [RequireBotGuildPermissions(Permission.KickMembers)]

        public async Task<DiscordCommandResult> KickAsync(
            [RequireBotHierarchy] IMember member,
            [Remainder] string reason = null)
        {
            reason ??= $"Kicked by action of {Context.Author.Tag}";

            await member.KickAsync(new DefaultRestRequestOptions
            {
                Reason = reason
            });
            
            await _moderationService.SendModLogAsync(member.Tag, Context.Author.Tag, reason,member.Id, ModLogType.Kick);

            return Response($"Successfully kicked {member.Tag}");
        }
        
        // [Command("kick", "boot")]
        // [Description("Kick a user from the server")]
        // [RequireBotGuildPermissions(Permission.KickMembers)]
        // public async Task<DiscordCommandResult> KickAsync(
        //     ulong id,
        //     [Remainder] string reason = null)
        // {
        //     
        //     
        //     reason ??= $"Kicked by action of {Context.Author.Tag}";
        //     
        //     await Context.Guild.KickMemberAsync(new Snowflake(id), new DefaultRestRequestOptions
        //     {
        //         Reason = reason
        //     });
        //
        //     await _moderationService.SendModLogAsync($"ID: {id} (User no longer in server)", Context.Author.Tag, reason, id,
        //         ModLogType.Kick);
        //
        //     return Response("Successfully kicked that user");
        // }

        [Command("ban", "hammer")]
        [Description("Ban a user from the server")]
        [RequireBotGuildPermissions(Permission.BanMembers)]
        public async Task<DiscordCommandResult> BanAsync(
            IMember member, 
            [Remainder] string reason = null)
        {
            reason ??= $"Banned by action of {Context.Author.Tag}";
            
            await member.BanAsync(reason);

            await _moderationService.SendModLogAsync(member.Tag, Context.Author.Tag, reason, member.Id, ModLogType.Ban);
            
            return Response($"Successfully banned {member.Tag}");
        }
        
        [Command("ban", "hammer")]
        [Description("Ban a user from the server")]
        [RequireBotGuildPermissions(Permission.BanMembers)]
        public async Task<DiscordCommandResult> BanAsync(
            ulong id, 
            [Remainder] string reason = null)
        {
            reason ??= $"Banned by action of {Context.Author.Tag}";
            
            await Context.Guild.CreateBanAsync(new Snowflake(id), reason);

            await _moderationService.SendModLogAsync($"ID: {id} (User no longer in server)", Context.Author.Tag, reason, id,
                ModLogType.Ban);

            return Response("Successfully banned that user");
        }
    }
}