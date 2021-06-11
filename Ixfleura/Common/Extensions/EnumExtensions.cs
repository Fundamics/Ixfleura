using Disqord;
using Ixfleura.Common.Types;

namespace Ixfleura.Common.Extensions
{
    public static class EnumExtensions
    {
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