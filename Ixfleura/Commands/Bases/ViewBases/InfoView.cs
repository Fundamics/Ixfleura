using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Ixfleura.Common.Extensions;
using Ixfleura.Common.Globals;

namespace Ixfleura.Commands.Bases.ViewBases
{
    public class InfoView : ViewBase
    {
        private static readonly LocalEmbed MainInfoEmbed = new LocalEmbed()
            .WithIxfleuraColor()
            .WithTitle("Ixfleura")
            .AddField("Authors", "shift-eleven#7304 and Daniel#1920")
            .AddField("Library", Markdown.Link("Disqord " + Library.Version, Library.RepositoryUrl), true);

        private static readonly LocalEmbed SecondaryInfoEmbed = new LocalEmbed()
            .WithIxfleuraColor()
            .WithTitle("Some more info")
            .WithDescription(IxfleuraGlobals.Description);

        public InfoView() : base(new LocalMessage().WithEmbeds(MainInfoEmbed))
        {
            AddComponent(new LinkButtonViewComponent(IxfleuraGlobals.GitHubRepo)
            {
                Label = "Source",
                Position = 2,
                Emoji = LocalEmoji.Custom(856386133235728404, "github")
            });
        }

        [Button(Label = "Main info", Style = ButtonComponentStyle.Secondary)]
        public ValueTask MainInfo(ButtonEventArgs e)
        {
            if (TemplateMessage.Embeds[0].Title == "Ixfleura")
                return default;

            TemplateMessage.Embeds[0] = MainInfoEmbed;
            ReportChanges();
            return default;
        }

        [Button(Label = "Some more info", Style = ButtonComponentStyle.Secondary)]
        public ValueTask SecondaryInfo(ButtonEventArgs e)
        {
            if (TemplateMessage.Embeds[0].Title == "Some more info")
                return default;

            TemplateMessage.Embeds[0] = SecondaryInfoEmbed;
            ReportChanges();
            return default;
        }
    }
}