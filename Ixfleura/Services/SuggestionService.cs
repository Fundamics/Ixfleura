using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Ixfleura.Data;
using Ixfleura.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ixfleura.Services
{
    public class SuggestionService : IxService
    {
        private readonly IDbContextFactory<IxfleuraDbContext> _dbContextFactory;
        
        public SuggestionService(ILogger<SuggestionService> logger, IDbContextFactory<IxfleuraDbContext> dbContextFactory) : base(logger)
        {
            _dbContextFactory = dbContextFactory;
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

        public async Task RemoveSuggestionAsync(Suggestion suggestion)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            db.Suggestions.Attach(suggestion);
            db.Suggestions.Remove(suggestion);
            await db.SaveChangesAsync();
        }
    }
}