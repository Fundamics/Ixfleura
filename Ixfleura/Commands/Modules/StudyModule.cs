using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot;
using Disqord.Rest;
using Ixfleura.Commands.Checks;
using Microsoft.Extensions.Configuration;
using Qmmands;

namespace Ixfleura.Commands.Modules
{
    /// <summary>
    /// Studying commands module.
    /// </summary>
    [Name("Studying")]
    [Description("Studying related commands")]
    public class StudyModule : DiscordGuildModuleBase
    {
        private readonly IConfiguration _configuration;

        public StudyModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        /// <summary>
        /// Mark yourself as studying. Use again to remove the studying role.
        /// </summary>
        [Command("studying")]
        [Description("Mark yourself as studying. Use again to remove the studying role")]
        [RequireFundamics]
        public async Task<DiscordCommandResult> StudyingRoleAsync()
        {
            var studyingRoleId = _configuration.GetValue<ulong>("roles:studying");

            if (Context.Author.RoleIds.Contains(studyingRoleId))
            {
                await Context.Author.RevokeRoleAsync(studyingRoleId);
                return Response("You are no longer studying!");
            }

            await Context.Author.GrantRoleAsync(studyingRoleId);
            return Response($"{Context.Author.Name}, you are now studying");
        }
    }
}