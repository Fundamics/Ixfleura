using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Rest;
using Humanizer;
using Ixfleura.Common.Configuration;
using Ixfleura.Data;
using Ixfleura.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ixfleura.Services
{
    public class CampaignService : IxfleuraService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly CampaignsConfiguration _config;
        private readonly IDbContextFactory<IxfleuraDbContext> _dbContextFactory;
        private readonly DiscordClientBase _client;

        public CampaignService(
            ILogger<CampaignService> logger, 
            IConfiguration configuration,
            IOptions<CampaignsConfiguration> config,
            IDbContextFactory<IxfleuraDbContext> dbContextFactory,
            DiscordClientBase client) : base(logger)
        {
            _logger = logger;
            _configuration = configuration;
            _config = config.Value;
            _dbContextFactory = dbContextFactory;
            _client = client;

            ValidateConfiguration();
        }

        public void ValidateConfiguration()
        {
            foreach (var type in _config.Types)
                if (_config.Types.Count(x => string.Equals(x.Name, type.Name, StringComparison.CurrentCultureIgnoreCase)) > 1)
                    throw new InvalidOperationException($"There are two or more campaign types configured with the name \"{type.Name}\".");
        }

        public async Task<List<Campaign>> GetCampaignsForCandidateAsync(ulong candidateId)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            return await db.Campaigns.Where(x => x.CandidateId == candidateId).ToListAsync();
        }

        public async Task<Campaign> CreateCampaignAsync(IMember candidate, IMember advocate, CampaignTypeConfiguration campaignType)
        {
            var campaign = new Campaign
            {
                Type = campaignType.Name.ToLower(),
                CandidateId = candidate.Id,
                AdvocateId = advocate.Id,
                CreatedAt = DateTime.UtcNow
            };

            var message = await _client.SendMessageAsync(_config.ChannelId, 
                new LocalMessage()
                    .WithEmbed(
                        new LocalEmbed()
                        .WithAuthor(candidate)
                        .WithTitle($"Campaign for {campaignType.Name}")
                        .WithDescription($"This campaign will end after {campaignType.Duration.Humanize(2)}.\n" +
                                         $"For the campaign to be accepted, at least {campaignType.MinimumVotes} votes need to be cast," +
                                         $" with {(int)Math.Round(campaignType.MinimumRatio * 100)}% of them being in favour of acceptance.")
                        .AddField(
                            campaignType.RoleIds.Length == 1 ? "Role" : "Roles", 
                            campaignType.RoleIds.Any() 
                                ? string.Join(", ", campaignType.RoleIds.Select(x => Mention.Role(x))) 
                                : "None",
                            true)
                        .WithFooter(
                            new LocalEmbedFooter()
                                .WithText($"Started by {advocate.Tag} | Ends at")
                                .WithIconUrl(advocate.GetAvatarUrl())
                            )
                        .WithTimestamp(DateTimeOffset.UtcNow.Add(campaignType.Duration))
                    ));
            
            campaign.MessageId = message.Id;

            await using var db = _dbContextFactory.CreateDbContext();
            //db.Campaigns.Add(campaign);
            await db.SaveChangesAsync();
            
            var (denyEmoji, checkEmoji) = GetCampaignEmojis();
            await message.AddReactionAsync(checkEmoji);
            await message.AddReactionAsync(denyEmoji);

            return campaign;
        }
        
        private (LocalEmoji denyEmoji, LocalEmoji checkEmoji) GetCampaignEmojis()
        {
            var denyName = _configuration["emojis:deny:name"];
            var denyId = _configuration.GetValue<ulong>("emojis:deny:id");
            
            var checkName = _configuration["emojis:check:name"];
            var checkId = _configuration.GetValue<ulong>("emojis:check:id");

            return (LocalEmoji.Custom(denyId, denyName), LocalEmoji.Custom(checkId, checkName));
        }
    }
}
