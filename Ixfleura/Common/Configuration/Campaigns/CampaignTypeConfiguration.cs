using System;

namespace Ixfleura.Common.Configuration
{
    public class CampaignTypeConfiguration
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public ulong[] RoleIds { get; set; }
        public ulong[] RequiredRoleIds { get; set; }
        public TimeSpan Duration { get; set; }
        public int MinimumVotes { get; set; }
        public decimal MinimumRatio { get; set; }
    }
}
