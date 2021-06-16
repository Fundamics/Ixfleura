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
    /// <summary>
    /// Moderation commands module.
    /// </summary>
    [Name("Moderation")]
    [Description("Moderation commands")]
    [RequireModOrAdmin]
    public class ModerationModule : DiscordGuildModuleBase
    {
        private readonly ModerationService _moderationService;
        public ModerationModule(ModerationService moderationService)
        {
            _moderationService = moderationService;
        }
        
        /// <summary>
        /// Kicks a member from the server.
        /// </summary>
        /// <param name="member">
        /// The member to kick.
        /// </param>
        /// <param name="reason">
        /// The reason for which the user was kicked.
        /// </param>
        [Command("kick", "boot")]
        [Description("Kick a user from the server")]
        [RequireBotGuildPermissions(Permission.KickMembers)]
        public async Task<DiscordCommandResult> KickAsync(
            [RequireBotHierarchy, Description("The member to kick")] IMember member,
            [Description("The reason for the action"), Remainder] string reason = null)
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

        /// <summary>
        /// Bans a member from the server.
        /// </summary>
        /// <param name="member">
        /// The member to ban.
        /// </param>
        /// <param name="reason">
        /// The reason for which the member was banned.
        /// </param>
        [Command("ban", "hammer")]
        [Description("Ban a user from the server")]
        [RequireBotGuildPermissions(Permission.BanMembers)]
        public async Task<DiscordCommandResult> BanAsync(
            [RequireBotHierarchy, Description("The member to ban")] IMember member, 
            [Description("The reason for the action"), Remainder] string reason = null)
        {
            reason ??= $"Banned by action of {Context.Author.Tag}";
            
            await member.BanAsync(reason);

            await _moderationService.SendModLogAsync(member.Tag, Context.Author.Tag, reason, member.Id, ModLogType.Ban);
            
            return Response($"Successfully banned {member.Tag}");
        }
        
        /// <summary>
        /// Ban a member from the server
        /// </summary>
        /// <param name="id">
        /// The id of the user to ban
        /// </param>
        /// <param name="reason">
        /// The reason for which the member was banned.
        /// </param>
        /// <returns></returns>
        [Command("ban", "hammer")]
        [Description("Ban a user from the server")]
        [RequireBotGuildPermissions(Permission.BanMembers)]
        public async Task<DiscordCommandResult> BanAsync(
            [Description("The id of the user to ban")] ulong id, 
            [Description("The reason for the ban"), Remainder] string reason = null)
        {
            reason ??= $"Banned by action of {Context.Author.Tag}";
            
            await Context.Guild.CreateBanAsync(new Snowflake(id), reason);

            await _moderationService.SendModLogAsync($"ID: {id} (User no longer in server)", Context.Author.Tag, reason, id,
                ModLogType.Ban);

            return Response("Successfully banned that user");
        }
    }
}
