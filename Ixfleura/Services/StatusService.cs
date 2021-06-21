using System;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ixfleura.Services
{
    /// <summary>
    /// A service to help set the status every minute.
    /// </summary>
    public class StatusService : DiscordBotService
    {
        private readonly ulong _guildId;

        public StatusService(ILogger<StatusService> logger, DiscordBotBase bot, IConfiguration configuration) : base(logger, bot)
        {
            _guildId = configuration.GetValue<ulong>("fundamics:guild_id");
        }

        /// <summary>
        /// Sets the status with the member count of Fundamics.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Client.WaitUntilReadyAsync(cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
                await Client.SetPresenceAsync(new LocalActivity($"ixhelp | {Client.GetGuild(_guildId).MemberCount}",
                    ActivityType.Playing));
            }
        }
    }
}