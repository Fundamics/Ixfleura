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
    /// <summary>
    /// A service to assist in moderation and auto-mod.
    /// </summary>
    public class ModerationService : DiscordBotService
    {
        private readonly ulong _botLogChannelId;
        private readonly string[] _swears;
        
        public ModerationService(ILogger<ModerationService> logger, DiscordBotBase bot, IConfiguration configuration) : base(logger, bot)
        {
            _botLogChannelId = configuration.GetValue<ulong>("fundamics:botlog_id");
            _swears = configuration.GetSection("fundamics:swears").Get<string[]>();
        }

        /// <summary>
        /// Sends a log message to the log channel.
        /// </summary>
        /// <param name="offender">The offender who broke the rules.</param>
        /// <param name="responsibleModerator">The moderator responsible for handling the action.</param>
        /// <param name="reason">The reason for the action taken.</param>
        /// <param name="id">The id of the offender.</param>
        /// <param name="modLogType">The type of action taken.</param>
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

        /// <summary>
        /// Auto filters messages for swear words based on values present in the config.
        /// </summary>
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
