using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Ixfleura.Commands.Checks
{
    /// <summary>
    /// A check to ensure that the member executing the command has the server's mod or admin role.
    /// </summary>
    public class RequireModOrAdminAttribute : DiscordCheckAttribute
    {
        public override ValueTask<CheckResult> CheckAsync(DiscordCommandContext context)
        {
            var configuration = context.Services.GetRequiredService<IConfiguration>();
            var modId = configuration.GetValue<ulong>("roles:mod");
            var adminId = configuration.GetValue<ulong>("roles:admin");
            
            if (context.Author is not IMember member)
                return Failure("You need to be inside a guild");

            if (!(member.RoleIds.Contains(modId) || member.RoleIds.Contains(adminId)))
                return Failure("You do not have the necessary permissions to do this command");

            return Success();
        }
    }
}
