using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Qmmands;

namespace Ixfleura.Commands.Checks
{
    public class RequireModOrAdminAttribute : DiscordCheckAttribute
    {
        private readonly Snowflake _modId = new Snowflake(828662023666663473);
        private readonly Snowflake _adminId = new Snowflake(828662023666663473);
        
        public override ValueTask<CheckResult> CheckAsync(DiscordCommandContext context)
        {
            if (context.Author is not IMember member)
                return Failure("You need to be inside a guild");

            if (!(member.RoleIds.Contains(_modId) || member.RoleIds.Contains(_adminId)))
                return Failure("You do not have the necessary permissions to do this command");

            return Success();
        }
    }
}