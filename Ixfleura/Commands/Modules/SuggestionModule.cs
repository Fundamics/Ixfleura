using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using Ixfleura.Commands.Checks;
using Ixfleura.Common.Extensions;
using Ixfleura.Data.Entities;
using Ixfleura.Services;
using Microsoft.Extensions.Configuration;
using Qmmands;

namespace Ixfleura.Commands.Modules
{
    [Group("suggest", "suggestion")]
    public class SuggestionModule : DiscordGuildModuleBase
    {
        private readonly IConfiguration _configuration;
        private readonly SuggestionService _suggestionService;

        public SuggestionModule(IConfiguration configuration, SuggestionService suggestionService)
        {
            _configuration = configuration;
            _suggestionService = suggestionService;
        }

        [Command]
        public async Task CreateSuggestionAsync([Remainder] string content)
        {
            var suggestion = new Suggestion
            {
                Content = content,
                GuildId = Context.GuildId,
                SuggesterId = Context.Author.Id
            };

            var suggestionChannelId = _configuration.GetValue<ulong>("fundamics:suggestion_id");

            await _suggestionService.CreateSuggestionAsync(suggestion);

            var suggestionEmbed = new LocalEmbed()
                .WithThumbnailUrl(Context.Author.GetAvatarUrl(ImageFormat.Png))
                .WithFooter($"Suggestion Id: {suggestion.Id}")
                .WithIxColor()
                .AddField("Submitter", Context.Author.Tag)
                .AddField("Suggestion", content);

            // for testing just change this to Context.ChannelId
            var suggestionMessage = await Context.Bot.SendMessageAsync(suggestionChannelId, new LocalMessage().WithEmbed(suggestionEmbed));

            var (denyEmoji, checkEmoji) = _suggestionService.GetSuggestionEmojis();

            await suggestionMessage.AddReactionAsync(checkEmoji);
            await suggestionMessage.AddReactionAsync(denyEmoji);
            
            suggestion.MessageId = suggestionMessage.Id;
            suggestion.ChannelId = suggestionMessage.ChannelId;
            
            await _suggestionService.UpdateSuggestionAsync(suggestion);

            await Response("Your suggestion has been made.");

            await Context.Message.DeleteAsync();
        }

        [Command("accept")]
        [RequireModOrAdmin]
        public async Task AcceptSuggestionAsync(int id, [Remainder] string reason)
        {
            var suggestion = await _suggestionService.GetSuggestionAsync(id);

            await Context.Message.DeleteAsync();

            if (suggestion is null)
            {
                var message = await Response("A suggestion with that id could not be found.");
                await Task.Delay(8000);
                await message.DeleteAsync();
                return;
            }
            
            var forCount = 0;
            var againstCount = 0;

            var suggestionMessage = await Context.Bot.FetchMessageAsync(suggestion.ChannelId, suggestion.MessageId);

            if (suggestionMessage.Reactions.HasValue)
            {
                var (denyEmoji, checkEmoji) = _suggestionService.GetSuggestionEmojis();

                forCount = suggestionMessage.Reactions.Value[denyEmoji].Count - 1;
                againstCount = suggestionMessage.Reactions.Value[checkEmoji].Count - 1;
            }
            
            var metaChannelId = _configuration.GetValue<ulong>("fundamics:meta_id");

            var acceptEmbed = new LocalEmbed()
                .WithColor(Color.Green)
                .WithFooter($"suggestion Id: {suggestion.Id}")
                .WithTimestamp(DateTimeOffset.UtcNow)
                .AddField("Results", $"For: {forCount}\n Against: {againstCount}")
                .AddField("Suggestion", suggestion.Content)
                .AddField("Submitter", Mention.User(suggestion.SuggesterId))
                .AddField("Accepted by", Context.Author.Mention)
                .AddField("Response", reason);

            await Context.Bot.SendMessageAsync(metaChannelId, new LocalMessage().WithEmbed(acceptEmbed));

            await _suggestionService.RemoveSuggestionAsync(suggestion);
        }
        
        [Command("reject")]
        [RequireModOrAdmin]
        public async Task RejectSuggestionAsync(int id, [Remainder] string reason)
        {
            var suggestion = await _suggestionService.GetSuggestionAsync(id);

            await Context.Message.DeleteAsync();

            if (suggestion is null)
            {
                var message = await Response("A suggestion with that id could not be found.");
                await Task.Delay(8000);
                await message.DeleteAsync();
                return;
            }
            
            var forCount = 0;
            var againstCount = 0;

            var suggestionMessage = await Context.Bot.FetchMessageAsync(suggestion.ChannelId, suggestion.MessageId);

            if (suggestionMessage.Reactions.HasValue)
            {
                var (denyEmoji, checkEmoji) = _suggestionService.GetSuggestionEmojis();

                forCount = suggestionMessage.Reactions.Value[denyEmoji].Count - 1;
                againstCount = suggestionMessage.Reactions.Value[checkEmoji].Count - 1;
            }
            
            var metaChannelId = _configuration.GetValue<ulong>("fundamics:meta_id");

            var rejectEmbed = new LocalEmbed()
                .WithColor(Color.Red)
                .WithFooter($"suggestion Id: {suggestion.Id}")
                .WithTimestamp(DateTimeOffset.UtcNow)
                .AddField("Results", $"For: {forCount}\n Against: {againstCount}")
                .AddField("Suggestion", suggestion.Content)
                .AddField("Submitter", Mention.User(suggestion.SuggesterId))
                .AddField("Rejected by", Context.Author.Mention)
                .AddField("Response", reason);

            await Context.Bot.SendMessageAsync(metaChannelId, new LocalMessage().WithEmbed(rejectEmbed));

            await _suggestionService.RemoveSuggestionAsync(suggestion);
        }
    }
}
