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
        private readonly int _resetAfter;

        public StatusService(ILogger<StatusService> logger, DiscordBotBase bot, IConfiguration configuration) : base(logger, bot)
        {
            _guildId = configuration.GetValue<ulong>("fundamics:guild_id");
            _resetAfter = configuration.GetValue<int>("discord:status_reset");
        }

        /// <summary>
        /// Sets the status with the member count of Fundamics.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Client.WaitUntilReadyAsync(stoppingToken);

            var guild = Client.GetGuild(_guildId);
            var memberCount = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                if (memberCount != guild.MemberCount)
                {
                    memberCount = guild.MemberCount;
                    
                    await Client.SetPresenceAsync(new LocalActivity($"ixhelp | {memberCount} members", ActivityType.Playing), stoppingToken);
                }

                await Task.Delay(TimeSpan.FromMinutes(_resetAfter), stoppingToken);
            }
        }
    }
}