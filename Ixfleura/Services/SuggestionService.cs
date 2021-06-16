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
    /// <summary>
    /// A service to handle suggestions.
    /// </summary>
    public class SuggestionService : IxfleuraService
    {
        private readonly IDbContextFactory<IxfleuraDbContext> _dbContextFactory;
        private readonly IConfiguration _configuration;
        
        public SuggestionService(ILogger<SuggestionService> logger, IDbContextFactory<IxfleuraDbContext> dbContextFactory, IConfiguration configuration) : base(logger)
        {
            _dbContextFactory = dbContextFactory;
            _configuration = configuration;
        }

        /// <summary>
        /// Finds a suggestion via the given id.
        /// </summary>
        /// <param name="id">
        /// The id of the suggestion.
        /// </param>
        /// <returns>
        /// A <see cref="Suggestion"/> or null when no suggestion with that id is found.
        /// </returns>
        public async Task<Suggestion> GetSuggestionAsync(int id)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            return await db.Suggestions.FindAsync(id);
        }

        /// <summary>
        /// Gets the current suggestions
        /// </summary>
        /// <param name="userId">
        /// The id of the user for which the suggestions are retrieved.
        /// </param>
        /// <returns>
        /// A list of <see cref="Suggestion"/>s.
        /// </returns>
        public async Task<List<Suggestion>> GetCurrentSuggestionsAsync(Snowflake userId)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            return await db.Suggestions.Where(x => x.SuggesterId == userId.RawValue).ToListAsync();
        }
        
        /// <summary>
        /// Add a new suggestion to the database.
        /// </summary>
        /// <param name="suggestion">
        /// The <see cref="Suggestion"/> to add.
        /// </param>
        public async Task CreateSuggestionAsync(Suggestion suggestion)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            await db.Suggestions.AddAsync(suggestion);
            await db.SaveChangesAsync();
        }
        
        /// <summary>
        /// Updates a suggestion.
        /// </summary>
        /// <param name="suggestion">
        /// The <see cref="Suggestion"/> to update.
        /// </param>
        public async Task UpdateSuggestionAsync(Suggestion suggestion)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            db.Suggestions.Update(suggestion);
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Remove a suggestion from the database.
        /// </summary>
        /// <param name="suggestion">
        /// The <see cref="Suggestion"/> to delete.
        /// </param>
        public async Task RemoveSuggestionAsync(Suggestion suggestion)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            db.Suggestions.Attach(suggestion);
            db.Suggestions.Remove(suggestion);
            await db.SaveChangesAsync();
        }
        
        /// <summary>
        /// Gets the emojis for suggestion messages.
        /// </summary>
        /// <returns>A tuple consisting of the deny and check <see cref="LocalEmoji"/>s.</returns>
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