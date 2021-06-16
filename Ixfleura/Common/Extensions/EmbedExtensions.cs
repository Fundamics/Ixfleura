using Disqord;
using Ixfleura.Common.Types;

namespace Ixfleura.Common.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="LocalEmbed"/>.
    /// </summary>
    public static class EmbedExtensions
    {
        /// <summary>
        /// An extension method to add Ixfleura's brand color to embeds.
        /// </summary>
        /// <param name="le">
        /// The LocalEmbed to add the color to.
        /// </param>
        /// <returns>
        /// The LocalEmbed with the color added.
        /// </returns>
        public static LocalEmbed WithIxfleuraColor(this LocalEmbed le)
            => le.WithColor(IxfleuraColors.IxColor);
    }
}
