using System;
using System.Linq;
using Ixfleura.Common.Configuration;
using Ixfleura.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ixfleura.Services
{
    public class CampaignService : IxfleuraService
    {
        private readonly ILogger _logger;
        private readonly CampaignsConfiguration _config;
        private readonly IDbContextFactory<IxfleuraDbContext> _db;

        public CampaignService(ILogger<CampaignService> logger, IOptions<CampaignsConfiguration> config, IDbContextFactory<IxfleuraDbContext> db) : base(logger)
        {
            _logger = logger;
            _config = config.Value;
            _db = db;

            ValidateConfiguration();
        }

        public void ValidateConfiguration()
        {
            foreach (var type in _config.Types)
                if (_config.Types.Count(x => string.Equals(x.Name, type.Name, StringComparison.CurrentCultureIgnoreCase)) > 1)
                    throw new InvalidOperationException($"There are two or more campaign types configured with the name \"{type.Name}\".");
        }
    }
}
