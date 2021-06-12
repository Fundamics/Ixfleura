using Disqord;
using Ixfleura.Common.Types;

namespace Ixfleura.Common.Extensions
{
    public static class EmbedExtensions
    {
        public static LocalEmbed WithIxColor(this LocalEmbed le)
            => le.WithColor(IxColors.IxColor);
    }
}