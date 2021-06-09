using Disqord;
using Ixfleura.Common.Globals;

namespace Ixfleura.Common.Extensions
{
    public static class EmbedExtensions
    {
        public static LocalEmbed WithIxColor(this LocalEmbed le)
            => le.WithColor(IxGlobals.IxColor);
    }
}