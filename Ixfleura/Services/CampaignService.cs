using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Hosting;
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
    public class CampaignService : DiscordClientService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly CampaignsConfiguration _config;
        private readonly IDbContextFactory<IxfleuraDbContext> _dbContextFactory;
        private readonly DiscordClientBase _client;
        private readonly ulong _guildId;
        
        private List<(int, Timer)> _timers;
        
        public CampaignService(
            ILogger<CampaignService> logger, 
            IConfiguration configuration,
            IOptions<CampaignsConfiguration> config,
            IDbContextFactory<IxfleuraDbContext> dbContextFactory,
            DiscordClientBase client) : base(logger, client)
        {
            _logger = logger;
            _configuration = configuration;
            _config = config.Value;
            _dbContextFactory = dbContextFactory;
            _client = client;

            _guildId = ulong.Parse(_configuration["fundamics:guild_id"]);

            _timers = new();
            
            ValidateConfiguration();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _client.WaitUntilReadyAsync(stoppingToken);
            await ScheduleCampaignsAsync();
        }

        private void ValidateConfiguration()
        {
            foreach (var type in _config.Types)
                if (_config.Types.Count(x => string.Equals(x.Name, type.Name, StringComparison.CurrentCultureIgnoreCase)) > 1)
                    throw new InvalidOperationException($"There are two or more campaign types configured with the name \"{type.Name}\".");
        }

        /// <summary>
        /// Gets a list of active campaigns for a candidate.
        /// </summary>
        /// <param name="candidateId">
        /// The id of the candidate to fetch campaigns for.
        /// </param>
        /// <returns>A list of <see cref="Campaign"/>s.</returns>
        public async Task<List<Campaign>> GetCampaignsForCandidateAsync(ulong candidateId)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            return await db.Campaigns.Where(x => x.CandidateId == candidateId).ToListAsync();
        }
        
        /// <summary>
        /// Gets a campaign by its id.
        /// </summary>
        /// <param name="id">
        /// The id of the campaign to get.
        /// </param>
        /// <returns>The <see cref="Campaign"/> with the id provided.</returns>
        public async Task<Campaign> GetCampaignAsync(int id)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            return await db.Campaigns.FirstOrDefaultAsync(x => x.Id == id);
        }

        /// <summary>
        /// Created a new campaign.
        /// </summary>
        /// <param name="candidate">
        /// The candidate of the campaign.
        /// </param>
        /// <param name="advocate">
        /// The member which created the campaign.
        /// </param>
        /// <param name="campaignType">
        /// The type of the campaign.
        /// </param>
        public async Task CreateCampaignAsync(IMember candidate, IMember advocate, CampaignTypeConfiguration campaignType)
        {
            var campaign = new Campaign
            {
                Type = campaignType.Name.ToLower(),
                CandidateId = candidate.Id,
                AdvocateId = advocate.Id,
                CreatedAt = DateTime.UtcNow
            };
            
            await using var db = _dbContextFactory.CreateDbContext();
            db.Campaigns.Add(campaign);
            await db.SaveChangesAsync();

            var message = await _client.SendMessageAsync(_config.ChannelId, 
                new LocalMessage()
                    .WithEmbeds(
                        new LocalEmbed()
                        .WithAuthor(candidate)
                        .WithTitle($"Campaign for {campaignType.Name}")
                        .WithDescription($"Duration: {campaignType.Duration.Humanize(2)}.\n" +
                                         $"Minimum votes: {campaignType.MinimumVotes}\n" +
                                         $"Minimum approval: {(int)Math.Round(campaignType.MinimumRatio * 100)}%")
                        .AddField(
                            campaignType.RoleIds.Length == 1 ? "Role" : "Roles", 
                            campaignType.RoleIds.Any() 
                                ? string.Join(", ", campaignType.RoleIds.Select(x => Mention.Role(x))) 
                                : "None",
                            true)
                        .WithFooter(
                            new LocalEmbedFooter()
                                .WithText($"#{campaign.Id} | Started by {advocate.Tag} | Ends at")
                                .WithIconUrl(advocate.GetAvatarUrl())
                            )
                        .WithTimestamp(DateTimeOffset.UtcNow.Add(campaignType.Duration))
                    ));
            
            campaign.MessageId = message.Id;
            db.Campaigns.Update(campaign);
            await db.SaveChangesAsync();
            
            var (denyEmoji, checkEmoji) = GetCampaignEmojis();
            await message.AddReactionAsync(checkEmoji);
            await message.AddReactionAsync(denyEmoji);

            var endsAt = campaign.CreatedAt.Add(campaignType.Duration);
                
            if (endsAt > DateTime.UtcNow)
            {
                var timer = new Timer(async _ =>
                {
                    await EndCampaignAsync(campaign, campaignType);
                }, null, endsAt - DateTime.UtcNow, Timeout.InfiniteTimeSpan);
                _timers.Add((campaign.Id, timer));
            }
            else
            {
                _ = EndCampaignAsync(campaign, campaignType);
            }
        }

        /// <summary>
        /// Cancels a campaign.
        /// </summary>
        /// <param name="campaign">
        /// The campaign to cancel.
        /// </param>
        /// <param name="canceller">
        /// The member which cancelled the campaign.
        /// </param>
        /// <param name="reason">
        /// The reason the campaign was cancelled.
        /// </param>
        public async Task CancelCampaignAsync(Campaign campaign, IMember canceller, string reason)
        {
            var timer = _timers.First(x => x.Item1 == campaign.Id).Item2;
            await timer.DisposeAsync();
            
            var message = await _client.FetchMessageAsync(_config.ChannelId, campaign.MessageId);
            var candidate = _client.GetMember(_guildId, campaign.CandidateId) ?? await _client.FetchMemberAsync(_guildId, campaign.CandidateId);
            var advocate = _client.GetMember(_guildId, campaign.AdvocateId) ?? await _client.FetchMemberAsync(_guildId, campaign.AdvocateId);
            var campaignType = _config.Types.FirstOrDefault(x => x.Name.ToLower() == campaign.Type);
            
            await message.DeleteAsync();
            
            await using var db = _dbContextFactory.CreateDbContext();
            db.Campaigns.Remove(campaign);
            await db.SaveChangesAsync();
            
            await _client.SendMessageAsync(_config.LogChannelId,
                new LocalMessage()
                    .WithContent(candidate.Mention)
                    .WithEmbeds(
                        new LocalEmbed()
                            .WithAuthor(candidate)
                            .WithTitle($"Campaign for {campaignType?.Name ?? "Unknown"} denied")
                            .WithDescription(
                                $"Cancelled by: {canceller.Mention}\n" +
                                $"{reason}")
                            .WithFooter(
                                new LocalEmbedFooter()
                                    .WithText($"Started by {advocate.Tag}")
                                    .WithIconUrl(advocate.GetAvatarUrl())
                            )
                            .WithColor(Color.Red)
                    ));
        }
        
        /// <summary>
        /// Schedules all campaigns in the database to be ended.
        /// </summary>
        private async Task ScheduleCampaignsAsync()
        {
            await using var db = _dbContextFactory.CreateDbContext();
            var campaigns = await db.Campaigns.ToListAsync();

            foreach (var campaign in campaigns)
            {
                var campaignType = _config.Types.FirstOrDefault(x => x.Name.ToLower() == campaign.Type);
                if (campaignType is null)
                {
                    _logger.LogWarning("The campaign with id {Id} could not be scheduled because the type configuration {Type} could not be found", campaign.Id, campaign.Type);
                    continue;
                }

                var endsAt = campaign.CreatedAt.Add(campaignType.Duration);
                
                if (endsAt > DateTime.UtcNow)
                {
                    var timer = new Timer(async _ =>
                    {
                        await EndCampaignAsync(campaign, campaignType);
                    }, null, endsAt - DateTime.UtcNow, Timeout.InfiniteTimeSpan);
                    _timers.Add((campaign.Id, timer));
                }
                else
                {
                    _ = EndCampaignAsync(campaign, campaignType);
                }
            }
        }

        /// <summary>
        /// Ends a campaign.
        /// </summary>
        /// <param name="campaign">
        /// The campaign to end.
        /// </param>
        /// <param name="campaignType">
        /// The type of the campaign.
        /// </param>
        private async Task EndCampaignAsync(Campaign campaign, CampaignTypeConfiguration campaignType)
        {
            var message = await _client.FetchMessageAsync(_config.ChannelId, campaign.MessageId);
            if (message is null)
            {
                _logger.LogWarning("The campaign with id {Id} could not be finished because the message {MessageId} could not be fetched", campaign.Id, campaign.MessageId);
                return;
            }
            
            var candidate = _client.GetMember(_guildId, campaign.CandidateId) ?? await _client.FetchMemberAsync(_guildId, campaign.CandidateId);
            var advocate = _client.GetMember(_guildId, campaign.AdvocateId) ?? await _client.FetchMemberAsync(_guildId, campaign.AdvocateId);

            // Fetch users which votes for or against the campaign
            var (denyEmoji, checkEmoji) = GetCampaignEmojis();
            var reactionsFor = (await message.FetchReactionsAsync(checkEmoji, int.MaxValue)).ToList();
            var reactionsAgainst = (await message.FetchReactionsAsync(denyEmoji, int.MaxValue)).ToList();

            // Ignore the bot's vote
            reactionsFor.RemoveAll(x => x.Id == _client.CurrentUser.Id);
            reactionsAgainst.RemoveAll(x => x.Id == _client.CurrentUser.Id);
            
            // Ignore double-votes
            var doubleVoteUserIds = reactionsFor.Where(x => reactionsAgainst.Any(y => y.Id == x.Id)).Select(x => x.Id).ToList();
            reactionsFor.RemoveAll(x => doubleVoteUserIds.Contains(x.Id));
            reactionsAgainst.RemoveAll(x => doubleVoteUserIds.Contains(x.Id));

            // Calculate the required values
            var totalVotes = reactionsFor.Count + reactionsAgainst.Count;
            var ratio = totalVotes > 0 ? (decimal)reactionsFor.Count / totalVotes : 0;

            await message.DeleteAsync();
            
            await using var db = _dbContextFactory.CreateDbContext();
            db.Campaigns.Remove(campaign);
            await db.SaveChangesAsync();
            
            if (totalVotes < campaignType.MinimumVotes)
            {
                await _client.SendMessageAsync(_config.LogChannelId, GetCampaignEndedMessage(false, campaign, campaignType, candidate, advocate, totalVotes, ratio, "Too few votes were cast."));
                return;
            }

            if (ratio < campaignType.MinimumRatio)
            {
                await _client.SendMessageAsync(_config.LogChannelId, GetCampaignEndedMessage(false, campaign, campaignType, candidate, advocate, totalVotes, ratio, "The approval rating was too low."));
                return;
            }

            foreach (var roleId in campaignType.RoleIds)
                await candidate.GrantRoleAsync(roleId);
            
            await _client.SendMessageAsync(_config.LogChannelId, GetCampaignEndedMessage(true, campaign, campaignType, candidate, advocate, totalVotes, ratio));
        }
        
        private LocalMessage GetCampaignEndedMessage(
            bool accepted,
            Campaign campaign,
            CampaignTypeConfiguration campaignType, 
            IMember candidate, 
            IMember advocate,
            int totalVotes,
            decimal ratio,
            string denialReason = null)
        {
            if (accepted)
            {
                return new LocalMessage()
                    .WithContent(candidate.Mention)
                    .WithEmbeds(
                        new LocalEmbed()
                            .WithAuthor(candidate)
                            .WithTitle($"Campaign for {campaignType.Name} accepted!")
                            .WithDescription(
                                $"Votes cast: {totalVotes}\n" +
                                $"Approval: {(int) Math.Round(ratio * 100)}%")
                            .AddField(
                                campaignType.RoleIds.Length == 1 ? "Role Granted" : "Roles Granted",
                                campaignType.RoleIds.Any()
                                    ? string.Join(", ", campaignType.RoleIds.Select(x => Mention.Role(x)))
                                    : "None",
                                true)
                            .WithFooter(
                                new LocalEmbedFooter()
                                    .WithText($"#{campaign.Id} | Started by {advocate.Tag}")
                                    .WithIconUrl(advocate.GetAvatarUrl())
                            )
                            .WithColor(Color.Green)
                    );
            }
            
            return new LocalMessage()
                .WithContent(candidate.Mention)
                .WithEmbeds(
                    new LocalEmbed()
                        .WithAuthor(candidate)
                        .WithTitle($"Campaign for {campaignType.Name} denied")
                        .WithDescription(
                            $"{denialReason}\n" +
                            $"Votes cast: {totalVotes}\n" +
                            $"Approval: {(int) Math.Round(ratio * 100)}%")
                        .WithFooter(
                            new LocalEmbedFooter()
                                .WithText($"#{campaign.Id} | Started by {advocate.Tag}")
                                .WithIconUrl(advocate.GetAvatarUrl())
                        )
                        .WithColor(Color.Red)
                );
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
