using System.Collections.Generic;

namespace Ixfleura.Common.Configuration
{
    public class CampaignsConfiguration
    {
        public ulong ChannelId { get; set; }
        
        public ulong LogChannelId { get; set; }
        public List<CampaignTypeConfiguration> Types { get; set; }
    }
}
