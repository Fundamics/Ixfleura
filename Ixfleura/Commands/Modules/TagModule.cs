using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Ixfleura.Commands.Checks;
using Ixfleura.Common.Types;
using Ixfleura.Data.Entities;
using Ixfleura.Services;
using Qmmands;

namespace Ixfleura.Commands.Modules
{
    [Group("tag")]
    [RequireFundamics]
    public class TagModule : DiscordGuildModuleBase
    {
        private readonly TagService _tagService;
        private readonly CommandService _commandService;

        public TagModule(TagService tagService, CommandService commandService)
        {
            _tagService = tagService;
            _commandService = commandService;
        }

        [Command("")]
        public DiscordCommandResult Help()
        {
            return Response(new LocalEmbed()
                .WithColor(IxColors.DefaultColor)
                .WithTitle("Tag")
                .WithDescription("`tag [name]` - Use a tag\n" +
                                 "`tag list` - List all tags in the server\n" +
                                 "`tag info [name...]` - Get information about a tag\n" +
                                 "`tag create [name] [content...]` - Create a new tag\n" +
                                 "`tag edit [name] [content...]` - Edit a tag\n" +
                                 "`tag remove [name...]` - Remove a tag"));
        }

        [Command("")]
        public async Task<DiscordCommandResult> Tag([Remainder] string name)
        {
            var tag = await _tagService.GetTagAsync(Context.GuildId, name);
            if (tag is null) 
                return await TagNotFoundResponse(name);
            
            tag.Uses++;
            await _tagService.UpdateTagAsync(tag);
            return Response(tag.Content);
        }

        [Command("list", "all")]
        public async Task<DiscordCommandResult> List()
        {
            var tags = await _tagService.GetTagsAsync(Context.GuildId);
            tags = tags.OrderByDescending(x => x.Uses).ToList();
            
            var tagStrings = tags.Select(x => $"{x.Name}, ");
            var stringPages = new List<string>();
            
            var current = "";
            foreach (var tagString in tagStrings)
            {
                if((current + tagString).Length <= 2048)
                    current += tagString;
                else
                {
                    stringPages.Add(current[..^2]);
                    current = tagString;
                }
            }
            if (!string.IsNullOrWhiteSpace(current))
                stringPages.Add(current[..^2]);

            var pages = stringPages.Select(x => new Page(
                new LocalEmbed()
                    .WithColor(IxColors.DefaultColor)
                    .WithTitle("Tags")
                    .WithDescription(x)
                    .WithFooter($"Page {stringPages.IndexOf(x) + 1} of {stringPages.Count}")))
                .ToList();

            return pages.Count switch
            {
                0 => Response("There are no tags for this server."),
                1 => Response(pages[0].Embed),
                _ => Pages(pages)
            };
        }
        
        [Command("info", "about")]
        [RequireModOrAdmin]
        public async Task<DiscordCommandResult> Info([Remainder] string name)
        {
            var tag = await _tagService.GetTagAsync(Context.GuildId, name);
            if (tag is null)
                return await TagNotFoundResponse(name);

            return Response(new LocalEmbed()
                .WithColor(IxColors.DefaultColor)
                .WithTitle($"Tag: {tag.Name}")
                .AddField("Uses", tag.Uses, true)
                .AddField("Created at", $"{tag.CreatedAt:yyyy-MM-dd}", true)
                .AddField("Edited at", $"{tag.EditedAt:yyyy-MM-dd}", true));
        }
        
        [Command("create", "add")]
        [RequireModOrAdmin]
        public async Task<DiscordCommandResult> Create(string name, [Remainder] string value)
        {
            if (!IsTagNameValid(name))
                return Response($"The tag name \"{name}\" is forbidden, please choose another name.");
            if (await _tagService.GetTagAsync(Context.GuildId, name) is not null)
                return Response($"The tag \"{name}\" already exists, please choose another name.");
            
            var tag = new Tag
            {
                GuildId = Context.GuildId,
                Name = name,
                Content = value,
                CreatedAt = DateTimeOffset.UtcNow,
                EditedAt = DateTimeOffset.UtcNow
            };
            await _tagService.CreateTagAsync(tag);
            
            return Response($"The tag \"{name}\" was created successfully.");
        }
        
        [Command("edit", "update")]
        [RequireModOrAdmin]
        public async Task<DiscordCommandResult> Edit(string name, [Remainder] string content)
        {
            var tag = await _tagService.GetTagAsync(Context.GuildId, name);
            if (tag is null)
                return await TagNotFoundResponse(name);

            tag.Content = content;
            tag.EditedAt = DateTimeOffset.UtcNow;
            await _tagService.UpdateTagAsync(tag);
            
            return Response($"The tag \"{name}\" was edited successfully.");
        }

        [Command("remove", "delete")]
        [RequireModOrAdmin]
        public async Task<DiscordCommandResult> Remove([Remainder] string name)
        {
            var tag = await _tagService.GetTagAsync(Context.GuildId, name);
            if (tag is null)
                return await TagNotFoundResponse(name);

            await _tagService.RemoveTagAsync(tag);
            
            return Response($"The tag \"{name}\" was removed successfully.");
        }

        private bool IsTagNameValid(string name)
            => _commandService
                .GetAllModules()
                .First(x => x.Type == typeof(TagModule)).Commands
                .All(x => x.Aliases
                    .All(y => !string.Equals(y, name, StringComparison.CurrentCultureIgnoreCase)));

        private async Task<DiscordCommandResult> TagNotFoundResponse(string name)
        {
            var closeTags = await _tagService.SearchTagsAsync(Context.GuildId, name);
            if (closeTags.Count == 0) 
                return Response($"I couldn't find a tag with the name \"{name}\".");
            
            var didYouMean = " • " + string.Join("\n • ", closeTags.Take(3).Select(x => x.Name));
            return Response($"I couldn't find a tag with the name \"{name}\", did you mean...\n{didYouMean}");
        }
    }
}