using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Ixfleura.Data;
using Ixfleura.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ixfleura.Services
{
    public class SuggestionService : IxService
    {
        private readonly IDbContextFactory<IxfleuraDbContext> _dbContextFactory;
        private readonly IConfiguration _configuration;
        
        public SuggestionService(ILogger<SuggestionService> logger, IDbContextFactory<IxfleuraDbContext> dbContextFactory, IConfiguration configuration) : base(logger)
        {
            _dbContextFactory = dbContextFactory;
            _configuration = configuration;
        }

        public async Task<Suggestion> GetSuggestionAsync(int id)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            return await db.Suggestions.FindAsync(id);
        }

        public async Task<List<Suggestion>> GetCurrentSuggestionsAsync(Snowflake userId)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            return await db.Suggestions.Where(x => x.SuggesterId == userId.RawValue).ToListAsync();
        }
        
        public async Task CreateSuggestionAsync(Suggestion suggestion)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            await db.Suggestions.AddAsync(suggestion);
            await db.SaveChangesAsync();
        }
        
        public async Task UpdateSuggestionAsync(Suggestion suggestion)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            db.Suggestions.Update(suggestion);
            await db.SaveChangesAsync();
        }

        public async Task RemoveSuggestionAsync(Suggestion suggestion)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            db.Suggestions.Attach(suggestion);
            db.Suggestions.Remove(suggestion);
            await db.SaveChangesAsync();
        }


        public (LocalEmoji denyEmoji, LocalEmoji checkEmoji) GetSuggestionEmojis()
        {
            var denyName = _configuration["emojis:deny:name"];
            var denyId = _configuration.GetValue<ulong>("emojis:deny:id");
            
            var checkName = _configuration["emojis:check:name"];
            var checkId = _configuration.GetValue<ulong>("emojis:check:id");

            return (LocalEmoji.Custom(denyId, denyName), LocalEmoji.Custom(checkId, checkName));
        }
    }
}