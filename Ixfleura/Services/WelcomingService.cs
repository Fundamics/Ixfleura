using System;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ixfleura.Services
{
    public class WelcomingService : DiscordBotService
    {
        private readonly ulong _welcomeChannelId;
        private readonly ulong _guildId;

        public WelcomingService(ILogger<WelcomingService> logger, DiscordBotBase bot, IConfiguration configuration) : base(logger, bot)
        {
            _welcomeChannelId = configuration.GetValue<ulong>("fundamics:welcome_id");
            _guildId = configuration.GetValue<ulong>("fundamics:guild_id");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Client.WaitUntilReadyAsync(stoppingToken);
        }

        protected override async ValueTask OnMemberJoined(MemberJoinedEventArgs e)
        {
            if (e.GuildId != _guildId)
                return;
            
            await Client.SendMessageAsync(_welcomeChannelId, new LocalMessage()
                .WithContent($"Welcome {e.Member.Tag} to Fundamics! Explore around and meet some Fundamicians just like you who are set on doing something big"));
        }
    }
}