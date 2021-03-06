using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using Ixfleura.Commands.Bases.ViewBases;
using Ixfleura.Common.Extensions;
using Qmmands;

namespace Ixfleura.Commands.Modules
{
    
    /// <summary>
    /// General commands to give some info.
    /// </summary>
    [Name("General")]
    [Description("A bunch of general commands")]
    public class GeneralModule : DiscordModuleBase
    {
        /// <summary>
        /// Gets the latency of the client.
        /// </summary>
        /// <remarks>
        /// Uses a <see cref="System.Diagnostics.Stopwatch"/> to mark the time taken to send a message.
        /// </remarks>
        [Command("ping")]
        [Description("play some ping-pong!")]
        public async Task Ping()
        {
            var stopwatch = Stopwatch.StartNew();
            var msg = await Response("Pong: *loading* response time");
            stopwatch.Stop();

            await msg.ModifyAsync(x => x.Content = $"Pong: {stopwatch.ElapsedMilliseconds}ms response time");
        }

        /// <summary>
        /// Get some information about Ixfleura
        /// </summary>
        [Command("info")]
        [Description("Get some info about the bot")]
        public DiscordCommandResult Info()
            => View(new InfoView());
        
        
        /// <summary>
        /// A help command. 
        /// </summary>
        /// <param name="path">
        /// The path to the command or module.
        /// </param>
        [Command("help")]
        [Description("Receive help")]
        public DiscordCommandResult Help([Description("The path to the command or module")] params string[] path)
        {
            var service = Context.Bot.Commands;
            var topLevelModules = service.TopLevelModules.ToArray();
            IReadOnlyList<Module> modules = topLevelModules.Where(x => x.Aliases.Count != 0).ToArray();
            IReadOnlyList<Command> commands = topLevelModules.Except(modules).SelectMany(x => x.Commands).ToArray();
            if (path.Length == 0)
            {
                var builder = new LocalEmbed()
                    .WithIxfleuraColor();
                if (modules.Count != 0)
                {
                    var aliases = modules.Select(x => x.Aliases[0])
                        .OrderBy(x => x)
                        .Select(Markdown.Code);
                    builder.AddField("Modules", string.Join(", ", aliases));
                }

                if (commands.Count != 0)
                {
                    var aliases = commands.Select(x => x.Aliases[0])
                        .OrderBy(x => x)
                        .Select(x => Markdown.Code(x))
                        .Distinct();
                    builder.AddField("Commands", string.Join(", ", aliases));
                }

                return builder.Fields.Count == 0 ? Reply("Nothing to display.") : Reply(builder);
            }

            var comparison = service.StringComparison;
            Module foundModule = null;
            Command foundCommand = null;
            foreach (var t in path)
            {
                if (foundModule != null)
                {
                    modules = foundModule.Submodules;
                    commands = foundModule.Commands;
                    foundModule = null;
                }

                var currentAlias = t.Trim();
                foreach (var module in modules)
                {
                    foreach (var alias in module.Aliases)
                    {
                        if (!currentAlias.Equals(alias, comparison)) continue;
                        foundModule = module;
                        break;
                    }
                }

                if (foundModule != null)
                    continue;
                
                foreach (var command in commands)
                {
                    foreach (var alias in command.Aliases)
                    {
                        if (currentAlias.Equals(alias, comparison))
                        {
                            foundCommand = command;
                            break;
                        }
                    }
                }

                if (foundCommand != null)
                    break;
            }

            if (foundModule == null && foundCommand == null)
                return Reply("No module or command found matching the input.");
            if (foundCommand != null)
            {
                var eb = new LocalEmbed()
                    .WithTitle(foundCommand.Name)
                    .WithDescription(foundCommand.Description ?? "No Description")
                    .WithIxfleuraColor()
                    .AddField("Module", foundCommand.Module is null ? "Top level command" : foundCommand.Module.Name)
                    .AddField("Aliases", foundCommand.Aliases is null
                        ? "No aliases"
                        : string.Join(", ", foundCommand.Aliases.Select(Markdown.Code)))
                    .AddField("Parameters", foundCommand.Parameters.Count == 0
                        ? "No parameters"
                        : string.Join(' ', foundCommand.Parameters.Select(FormatParameter)));

                if (foundCommand.Parameters.Count != 0)
                    eb.AddField("Parameter Descriptions", string.Join('\n', 
                        foundCommand.Parameters.Select(x => $"{x.Name}: {x.Description ?? "No description"}")));

                return Reply(eb);
            }
            else
            {
                var eb = new LocalEmbed()
                    .WithTitle(foundModule.Name)
                    .WithDescription(foundModule.Description ?? "No Description")
                    .WithIxfleuraColor()
                    .AddField("Submodules",
                        foundModule.Submodules.Count == 0
                            ? "No submodules"
                            : string.Join('\n', foundModule.Submodules.Select(x => Markdown.Code(x.Name))))
                    .AddField("Commands", string.Join('\n', foundModule.Commands.Where(x => !string.IsNullOrEmpty(x.Name))
                        .Select(x => Markdown.Code(x.Name))));

                return Reply(eb);
            }
        }

        /// <summary>
        /// Formats a <see cref="Parameter"/> for the help command.
        /// </summary>
        /// <param name="parameter">
        /// The parameter to be formatted.
        /// </param>
        /// <returns>
        /// a string consisting of the formatted parameter.
        /// </returns>
        private static string FormatParameter(Parameter parameter)
        {
            string format;
            if (parameter.IsMultiple)
            {
                format = "{0}[]";
            }
            else
            {
                var str = parameter.IsRemainder ? "{0}…" : "{0}";
                format = parameter.IsOptional ? "[" + str + "]" : "<" + str + ">";
            }
            return string.Format(format, parameter.Name);
        }
    }
}
