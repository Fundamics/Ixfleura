using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Disqord.Gateway;
using Disqord.Http;
using Disqord.Rest;
using FuzzySharp;
using Ixfleura.Common.Extensions;
using Ixfleura.Common.Types;
using Ixfleura.Services;
using Qmmands;

namespace Ixfleura.Commands.Modules
{
    /// <summary>
    /// Miscellaneous and fun commands module.
    /// </summary>
    public class FunModule : DiscordGuildModuleBase
    {
        private static readonly IReadOnlyList<string> EightBallResponses = new[]
        {
            // good
            "It is certain.",
            "It is decidedly so.",
            "Without a doubt.",
            "Yes – definitely.",
            "You may rely on it.",
            "As I see it, yes.",
            "Most likely.",
            "Outlook good.",
            "Yes.",
            "Signs point to yes.",
            // uncertain
            "Reply hazy, try again.",
            "Ask again later.",
            "Better not tell you now.",
            "Cannot predict now.",
            "Concentrate and ask again.",
            // bad
            "Don't count on it.",
            "My reply is no.",
            "My sources say no.",
            "Outlook not so good.",
            "Very doubtful. "
        };
        
        private readonly SearchService _searchService;
        private readonly Random _random;

        public FunModule(SearchService searchService, Random random)
        {
            _searchService = searchService;
            _random = random;
        }
        
        /// <summary>
        /// Creates a poll.
        /// </summary>
        /// <param name="question">
        /// The poll question to ask.
        /// </param>
        [Command("poll")]
        [Description("Create a poll!")]
        public async Task PollAsync([Remainder] string question)
        {
            await Context.Message.DeleteAsync();
            var pollEmbed = new LocalEmbed()
                .WithTitle("Poll!")
                .WithDescription(question)
                .WithAuthor(Context.Author.ToString(), Context.Author.GetAvatarUrl())
                .WithIxColor();

            var msg = await Response(pollEmbed);
            
            await msg.AddReactionAsync(new LocalEmoji("👍"));
            await msg.AddReactionAsync(new LocalEmoji("👎"));
            await msg.AddReactionAsync(new LocalEmoji("🤷"));
        }
        
        /// <summary>
        /// Gets information about a particular user or yourself.
        /// </summary>
        /// <param name="member">
        /// The member to receive information about. Grabs information about yourself is no user is passed.
        /// </param>
        [Command("whois", "userinfo")]
        [Description("Get info about yourself or someone else")]
        public DiscordCommandResult UserInfo( [Description("The user for whom you wish to receive information")] IMember member = null)
        {
            member ??= Context.Author;
            
            var roles = member.GetRoles();
            var topRole = roles.Values.OrderByDescending(x => x.Position).First();

            var eb = new LocalEmbed()
                .WithTitle(member.Tag)
                .WithThumbnailUrl(member.GetAvatarUrl())
                .WithColor(topRole.Color ?? IxColors.IxColor)
                .AddField("Id", member.Id, true)
                .AddField("Nickname", member.Nick ?? "No nickname in this guild", true)
                .AddField("Is Bot", member.IsBot ? "Yes" : "No", true)
                .AddField("Joined At", member.JoinedAt.Value.ToString("f"), true)
                .AddField("Created At", member.CreatedAt().ToString("f"));

            return Reply(eb);
        }

        /// <summary>
        /// Grabs a trivia questions using the <see cref="Ixfleura.Services.SearchService"/> and uses interactivity to wait for an answer.
        /// </summary>
        [Command("trivia")]
        [Description("Play some trivia questions!")]
        public async Task<DiscordCommandResult> TriviaAsync()
        {
            var (question, answer) = await _searchService.GetTriviaQuestionAsync();

            await Response($"{question}");

            var input = await Context.Channel.WaitForMessageAsync(x =>
                x.ChannelId == Context.ChannelId && x.Member.Id == Context.Author.Id);

            var userAnswer = input.Message.Content.ToLower();
            var res = Fuzz.Ratio(answer, userAnswer);
            
            if (userAnswer == answer)
                return Response("Correct answer!");
            if (res >= 70)
                return Response("Close answer! you're correct!");

            return Response("Incorrect answer!");
        }

        /// <summary>
        /// Repeats the text provided.
        /// </summary>
        /// <param name="echoText">
        /// The text to repeat.
        /// </param>
        [Command("echo", "say", "repeat")]
        [Description("I repeat whatever you say")]
        public DiscordCommandResult Echo([Remainder] string echoText)
            => Reply(echoText);

        /// <summary>
        /// Gives a random choice out of the options to be provided.
        /// </summary>
        /// <param name="choiceOptions">
        /// The string from which the options are to be parsed.
        /// </param>
        [Command("choose", "choice")]
        [Description("Choose from some options")]
        public DiscordCommandResult Choice([Remainder] string choiceOptions)
        {
            var choices = choiceOptions.Split('|', StringSplitOptions.TrimEntries);

            return Response(choices.Length < 2 ? "I need more options to choose from" : choices[_random.Next(choices.Length)]);
        }

        /// <summary>
        /// Flips a coin.
        /// </summary>
        [Command("flip", "coin")]
        [Description("Do a coin flip")]
        public DiscordCommandResult CoinFlip()
        {
            var num = _random.Next(0, 2);
            return Response(num == 0 ? "Heads" : "Tails");
        }
        
        /// <summary>
        /// Quotes a message's content.
        /// </summary>
        /// <param name="quoteUrl">
        /// The jump link of the message.
        /// </param>
        [Command("quote")]
        [Description("Quote a message")]
        public async Task<DiscordCommandResult> QuoteMessageAsync(string quoteUrl)
        {
            var regex = Discord.MessageJumpLinkRegex;

            if (!regex.IsMatch(quoteUrl))
                return Response("It seems you have not given me a valid jump URL");

            var res = regex.Match(quoteUrl);
            var channelId = Convert.ToUInt64(res.Groups["channel_id"].Value);
            var messageId = Convert.ToUInt64(res.Groups["message_id"].Value);

            IChannel channel;
            try
            {
                channel = await Context.Bot.FetchChannelAsync(channelId);
            }
            catch (RestApiException e) when (e.StatusCode == HttpResponseStatusCode.Forbidden)
            {
                return Response("It seems I am unable to access that channel");
            }

            if (channel is not IGuildChannel guildChannel)
            {
                return Response("I cannot read messages from a DM");
            }
            
            if (!Context.CurrentMember.GetChannelPermissions(guildChannel).ReadMessageHistory)
                return Response("I don't have the necessary permissions to view this channel");

            if (!Context.Author.GetChannelPermissions(guildChannel).ReadMessageHistory)
                return Response("You don't have the necessary permissions to view this channel");

            var message = await Context.Bot.FetchMessageAsync(channelId, messageId);
            
            var eb = new LocalEmbed()
                .WithAuthor(message.Author.ToString(), message.Author.GetAvatarUrl())
                .WithDescription(message.Content)
                .WithIxColor()
                .WithFooter($"Id: {messageId}")
                .WithTimestamp(message.CreatedAt());

            return Response(eb);
        }

        /// <summary>
        /// Quotes a message's content.
        /// </summary>
        [Command("quote")]
        [Description("Quote a message")]
        public DiscordCommandResult QuoteMessageAsync()
        {
            var messageRef = Context.Message.ReferencedMessage.GetValueOrDefault();
            if (messageRef is null)
                return Response("I require a Jump URL or a reference to a message to quote");
            
            var eb = new LocalEmbed()
                .WithAuthor(messageRef.Author.ToString(), messageRef.Author.GetAvatarUrl())
                .WithDescription(messageRef.Content)
                .WithIxColor()
                .WithFooter($"Id: {messageRef.Id}")
                .WithTimestamp(messageRef.CreatedAt());

            return Response(eb);
        }
        
        /// <summary>
        /// Gets a response from the magic 
        /// </summary>
        /// <param name="question">
        /// The question to ask.
        /// </param>
        [Command("8ball", "eightball")]
        [Description("Consult the magic 8ball")]
        public DiscordCommandResult EightBall([Remainder] string question)
            => Response(EightBallResponses[_random.Next(0, EightBallResponses.Count)]);
    }
}