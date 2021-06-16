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
                ModLogType.Ban => IxColors.BanColor,
                ModLogType.Unban => IxColors.UnbanColor,
                ModLogType.Kick => IxColors.KickColor,
                ModLogType.Warn => IxColors.WarnColor,
                _ => Color.White
            };
        }
    }
}