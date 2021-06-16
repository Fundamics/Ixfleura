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
    /// <summary>
    /// Tag related commands.
    /// </summary>
    [Name("Tag")]
    [Description("Tag related commands")]
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

        /// <summary>
        /// Get help info about tags.
        /// </summary>
        [Command]
        [Description("Get tag help")]
        public DiscordCommandResult Help()
        {
            return Response(new LocalEmbed()
                .WithColor(IxfleuraColors.DefaultColor)
                .WithTitle("Tag")
                .WithDescription("`tag [name]` - Use a tag\n" +
                                 "`tag list` - List all tags in the server\n" +
                                 "`tag info [name...]` - Get information about a tag\n" +
                                 "`tag create [name] [content...]` - Create a new tag\n" +
                                 "`tag edit [name] [content...]` - Edit a tag\n" +
                                 "`tag remove [name...]` - Remove a tag"));
        }

        /// <summary>
        /// Get a tag.
        /// </summary>
        /// <param name="name">
        /// The name of the tag to get.
        /// </param>
        [Command]
        [Description("Grab a tag")]
        public async Task<DiscordCommandResult> TagAsync([Remainder] string name)
        {
            var tag = await _tagService.GetTagAsync(Context.GuildId, name);
            if (tag is null) 
                return await TagNotFoundResponse(name);
            
            tag.Uses++;
            await _tagService.UpdateTagAsync(tag);
            return Response(tag.Content);
        }

        /// <summary>
        /// List all the tags of this guild.
        /// </summary>
        [Command("list", "all")]
        [Description("List all the tags of this server")]
        public async Task<DiscordCommandResult> ListTagsAsync()
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
                    .WithColor(IxfleuraColors.DefaultColor)
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
        
        /// <summary>
        /// Get information about a particular tag.
        /// </summary>
        /// <param name="name">
        /// The name of the tag.
        /// </param>
        [Command("info", "about")]
        [Description("Get info about a particular tag")]
        public async Task<DiscordCommandResult> TagInfoAsync([Remainder] string name)
        {
            var tag = await _tagService.GetTagAsync(Context.GuildId, name);
            if (tag is null)
                return await TagNotFoundResponse(name);

            return Response(new LocalEmbed()
                .WithColor(IxfleuraColors.DefaultColor)
                .WithTitle($"Tag: {tag.Name}")
                .AddField("Uses", tag.Uses, true)
                .AddField("Created at", $"{tag.CreatedAt:yyyy-MM-dd}", true)
                .AddField("Edited at", $"{tag.EditedAt:yyyy-MM-dd}", true));
        }
        
        /// <summary>
        /// Create a new tag.
        /// </summary>
        /// <param name="name">
        /// The name of the tag.
        /// </param>
        /// <param name="value">
        /// The content of the tag.
        /// </param>
        [Command("create", "add")]
        [Description("Create a new tag")]
        [RequireModOrAdmin]
        public async Task<DiscordCommandResult> CreateTagAsync(string name, [Remainder] string value)
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
        
        /// <summary>
        /// Edit an existing tag.
        /// </summary>
        /// <param name="name">
        /// The name of the tag.
        /// </param>
        /// <param name="content">
        /// The new content of the tag.
        /// </param>
        [Command("edit", "update")]
        [Description("Edit a tag")]
        [RequireModOrAdmin]
        public async Task<DiscordCommandResult> EditTagAsync(string name, [Remainder] string content)
        {
            var tag = await _tagService.GetTagAsync(Context.GuildId, name);
            if (tag is null)
                return await TagNotFoundResponse(name);

            tag.Content = content;
            tag.EditedAt = DateTimeOffset.UtcNow;
            await _tagService.UpdateTagAsync(tag);
            
            return Response($"The tag \"{name}\" was edited successfully.");
        }

        /// <summary>
        /// Delete a tag.
        /// </summary>
        /// <param name="name">
        /// The name of the tag.
        /// </param>
        [Command("remove", "delete")]
        [Description("Delete a tag")]
        [RequireModOrAdmin]
        public async Task<DiscordCommandResult> RemoveTagAsync([Remainder] string name)
        {
            var tag = await _tagService.GetTagAsync(Context.GuildId, name);
            if (tag is null)
                return await TagNotFoundResponse(name);

            await _tagService.RemoveTagAsync(tag);
            
            return Response($"The tag \"{name}\" was removed successfully.");
        }

        /// <summary>
        /// Validates tag names.
        /// </summary>
        /// <param name="name">
        /// The name to verify.
        /// </param>
        /// <returns>
        /// A <see cref="bool"/> representing if the name is valid or not.
        /// </returns>
        private bool IsTagNameValid(string name)
            => _commandService
                .GetAllModules()
                .First(x => x.Type == typeof(TagModule)).Commands
                .All(x => x.Aliases
                    .All(y => !string.Equals(y, name, StringComparison.CurrentCultureIgnoreCase)));

        /// <summary>
        /// Used when a tag is not found.
        /// </summary>
        /// <param name="name">
        /// The name of the tag not found.
        /// </param>
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
