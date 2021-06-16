using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Ixfleura.Common.Configuration;
using Ixfleura.Services;
using Microsoft.Extensions.Options;
using Qmmands;

namespace Ixfleura.Commands.Modules
{
    [Group("campaign")]
    public class CampaignModule : DiscordGuildModuleBase
    {
        private readonly CampaignsConfiguration _config;
        private readonly CampaignService _campaignService;

        public CampaignModule(IOptions<CampaignsConfiguration> config, CampaignService campaignService)
        {
            _config = config.Value;
            _campaignService = campaignService;
        }

        [Command("start")]
        public async Task<DiscordCommandResult> StartAsync([RequireNotBot] IMember candidate, [Remainder] string campaignType)
        {
            var campaignTypeConfig = _config.Types.FirstOrDefault(x => x.Name.ToLower() == campaignType.ToLower());
            if (campaignTypeConfig is null)
                return Response($"I couldn't find a campaign type with the name \"{campaignType}\"");
            
            if (campaignTypeConfig.Enabled)
                return Response($"The {campaignType} campaign is currently disabled");
            
            if(campaignTypeConfig.RoleIds.All(roleId => candidate.RoleIds.Contains(roleId)))
                return Response($"{candidate} already has all roles for the {campaignType} campaign");

            if (campaignTypeConfig.RequiredRoleIds.All(roleId => !candidate.RoleIds.Contains(roleId)))
                return Response($"{candidate} is not eligible for the {campaignType} campaign");

            var ongoingCampaigns = await _campaignService.GetCampaignsForCandidateAsync(candidate.Id);
            if (ongoingCampaigns.Any(x => x.Type == campaignType.ToLower()))
                return Response($"A {campaignType} campaign is already ongoing for {candidate}");

            return Response("Ok");
        }
    }
}
