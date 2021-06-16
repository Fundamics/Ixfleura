using Disqord;
using Ixfleura.Common.Types;

namespace Ixfleura.Common.Extensions
{
    /// <summary>
    /// Extension methods for enums.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Get the embed color based on a <see cref="ModLogType"/>.
        /// </summary>
        /// <param name="modLogType">
        /// The ModLogType for the action.
        /// </param>
        /// <returns>
        /// A <see cref="Color"/> for the action.
        /// </returns>
        public static Color GetModLogColor(this ModLogType modLogType)
        {
            return modLogType switch
            {
                ModLogType.Ban => IxfleuraColors.BanColor,
                ModLogType.Unban => IxfleuraColors.UnbanColor,
                ModLogType.Kick => IxfleuraColors.KickColor,
                ModLogType.Warn => IxfleuraColors.WarnColor,
                _ => Color.White
            };
        }
    }
}