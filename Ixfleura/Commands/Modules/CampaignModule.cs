using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Ixfleura.Commands.Checks;
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

        /// <summary>
        /// Starts a new campaign.
        /// </summary>
        /// <param name="candidate">
        /// The candidate of the campaign.
        /// </param>
        /// /// <param name="campaignType">
        /// The type of the campaign.
        /// </param>
        [Command("start")]
        [Description("Start a new campaign")]
        public async Task<DiscordCommandResult> StartAsync([RequireNotBot] IMember candidate, [Remainder] string campaignType)
        {
            var campaignTypeConfig = _config.Types.FirstOrDefault(x => x.Name.ToLower() == campaignType.ToLower());
            if (campaignTypeConfig is null)
                return Response($"I couldn't find a campaign type with the name \"{campaignType}\"");
            
            if (!campaignTypeConfig.Enabled)
                return Response($"The {campaignType} campaign is currently disabled");
            
            if(campaignTypeConfig.RoleIds.All(roleId => candidate.RoleIds.Contains(roleId)))
                return Response($"{candidate} already has all roles for the {campaignType} campaign");

            if (campaignTypeConfig.RequiredRoleIds.Any(roleId => !candidate.RoleIds.Contains(roleId)))
                return Response($"{candidate} is not eligible for the {campaignType} campaign");

            var ongoingCampaigns = await _campaignService.GetCampaignsForCandidateAsync(candidate.Id);
            if (ongoingCampaigns.Any(x => x.Type == campaignType.ToLower()))
                return Response($"A {campaignType} campaign is already ongoing for {candidate}");

            await _campaignService.CreateCampaignAsync(candidate, Context.Author, campaignTypeConfig);
            return Response("The campaign has been started!");
        }
        
        /// <summary>
        /// Cancels a campaign.
        /// </summary>
        /// <param name="campaignId">
        /// The id of the campaign to cancel.
        /// </param>
        /// /// <param name="reason">
        /// The reason for the campaign being cancelled.
        /// </param>
        [Command("cancel")]
        [Description("Cancel a campaign")]
        [RequireModOrAdmin]
        public async Task<DiscordCommandResult> CancelAsync(
            [Description("The id of the campaign to cancel")] int campaignId, 
            [Description("The reason for the campaign being cancelled"), Remainder] string reason = "No reason provided")
        {
            var campaign = await _campaignService.GetCampaignAsync(campaignId);
            if(campaign is null)
                return Response($"I couldn't find a campaign with the id {campaignId}");

            await _campaignService.CancelCampaignAsync(campaign, Context.Author, reason);
            return Response("The campaign has been cancelled");
        }
    }
}
