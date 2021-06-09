using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Disqord.Gateway;
using Disqord.Rest;
using FuzzySharp;
using Ixfleura.Common.Extensions;
using Ixfleura.Common.Globals;
using Ixfleura.Services;
using Qmmands;

namespace Ixfleura.Commands.Modules
{
    public class FunModule : DiscordGuildModuleBase
    {
        private readonly SearchService _searchService;

        public FunModule(SearchService searchService)
        {
            _searchService = searchService;
        }
        
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
        
        [Command("whois", "userinfo")]
        [Description("Get info about yourself or someone else")]
        public DiscordCommandResult UserInfo([Description("The user for whom you wish to receive information")] IMember member = null)
        {
            member ??= Context.Author;
            
            var roles = member.GetRoles();
            var topRole = roles.Values.OrderByDescending(x => x.Position).First();

            var eb = new LocalEmbed()
                .WithTitle(member.Tag)
                .WithThumbnailUrl(member.GetAvatarUrl())
                .WithColor(topRole.Color ?? IxGlobals.IxColor)
                .AddField("Id", member.Id, true)
                .AddField("Nickname", member.Nick ?? "No nickname in this guild", true)
                .AddField("Is Bot", member.IsBot ? "Yes" : "No", true)
                .AddField("Joined At", member.JoinedAt.Value.ToString("f"), true)
                .AddField("Created At", member.CreatedAt().ToString("f"));

            return Reply(eb);
        }

        [Command("trivia")]
        public async Task<DiscordCommandResult> TriviaAsync()
        {
            var (question, answer) = await _searchService.GetTriviaQuestionAsync();

            await Response($"{question}\nAnswer: {answer}");

            var input = await Context.Channel.WaitForMessageAsync(x =>
                x.ChannelId == Context.ChannelId && x.Member.Id == Context.Author.Id);

            var userAnswer = input.Message.Content.ToLower();
            var res = Fuzz.Ratio(answer, userAnswer);
            
            if (userAnswer == answer)
                return Response("Correct answer!");
            if (res >= 85)
                return Response("Close answer! you're correct!");

            return Response("Incorrect answer!");
        }
    }
}