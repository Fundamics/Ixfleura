using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Hosting;
using Disqord.Rest;
using Ixfleura.Common.Extensions;
using Ixfleura.Common.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ixfleura.Services
{
    public class ModerationService : DiscordBotService
    {
        private readonly ulong _botLogChannelId;
        private readonly string[] _swears;
        
        public ModerationService(ILogger<ModerationService> logger, DiscordBotBase bot, IConfiguration configuration) : base(logger, bot)
        {
            _botLogChannelId = configuration.GetValue<ulong>("fundamics:botlog_id");
            _swears = configuration.GetSection("fundamics:swears").Get<string[]>();
        }

        public async Task SendModLogAsync(string offender, string responsibleModerator, string reason, ulong id, ModLogType modLogType)
        {
            var logColor = modLogType.GetModLogColor();

            var le = new LocalEmbed()
                .WithTitle($"{modLogType.ToString()} case")
                .WithDescription($"**Offender:** {offender}\n**Reason:** {reason}\n**Responsible Moderator:** {responsibleModerator}")
                .WithColor(logColor)
                .WithFooter($"ID: {id}")
                .WithTimestamp(DateTimeOffset.UtcNow);

            await Client.SendMessageAsync(_botLogChannelId, new LocalMessage().WithEmbed(le));
        }

        protected override async ValueTask OnMessageReceived(BotMessageReceivedEventArgs e)
        {
            var words = e.Message.Content.Split(new[] {' ', '?', '.'}, StringSplitOptions.TrimEntries);
            e.ProcessCommands = !words.Intersect(_swears).Any();
            
            if (!e.ProcessCommands)
            {
                await e.Message.DeleteAsync(new DefaultRestRequestOptions
                {
                    Reason = "Message contained swear word."
                });

                await SendModLogAsync(e.Message.Author.Tag, "Ixfleura",
                    "Automatic action done because message contained swear word", e.Message.Author.Id, ModLogType.MessageDeleted);
            }
        }
    }
}