using System;
using System.Threading.Tasks;
using Disqord.Bot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Ixfleura.Commands.Checks
{
    /// <summary>
    /// A check to see if the guild where a command is executing is Fundamics.
    /// </summary>
    public class RequireFundamicsAttribute : DiscordCheckAttribute
    {
        public override ValueTask<CheckResult> CheckAsync(DiscordCommandContext context)
        {
            var configuration = context.Services.GetRequiredService<IConfiguration>();

            var guildId = Convert.ToUInt64(configuration["fundamics:guild_id"]);
            
            if (context.GuildId is null)
                return Failure("You can only execute this command inside the Fundamics guild!");
            
            if (context.GuildId != guildId)
                return Failure("You can only execute this command inside the Fundamics guild!");

            return Success();
        }
    }
}