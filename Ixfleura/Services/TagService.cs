using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using FuzzySharp;
using Microsoft.EntityFrameworkCore;
using Ixfleura.Data;
using Ixfleura.Data.Entities;
using Microsoft.Extensions.Logging;

namespace Ixfleura.Services
{
    /// <summary>
    /// A service to handle tags.
    /// </summary>
    public class TagService : IxfleuraService
    {
        private readonly IDbContextFactory<IxfleuraDbContext> _dbContextFactory;
        
        public TagService(ILogger<TagService> logger, IDbContextFactory<IxfleuraDbContext> dbContextFactory) : base(logger)
        {
            _dbContextFactory = dbContextFactory;
        }

        /// <summary>
        /// Gets a tag.
        /// </summary>
        /// <param name="guildId">The id of the guild for the tag.</param>
        /// <param name="name">The name of the tag.</param>
        /// <returns>A <see cref="Tag"/> or null if no tag was found.</returns>
        public async Task<Tag> GetTagAsync(Snowflake guildId, string name)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            return await db.Tags.FirstOrDefaultAsync(x => x.GuildId == guildId.RawValue && EF.Functions.ILike(x.Name, name));
        }

        /// <summary>
        /// Gets all the tags of a guild.
        /// </summary>
        /// <param name="guildId">
        /// The id of the guild to retrieve tags for.
        /// </param>
        /// <returns>A list of <see cref="Tag"/>s.</returns>
        public async Task<List<Tag>> GetTagsAsync(Snowflake guildId)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            return await db.Tags.Where(x => x.GuildId == guildId.RawValue).ToListAsync();
        }
        
        /// <summary>
        /// Searches tags.
        /// </summary>
        /// <param name="guildId">The id of the guild for which tags are to be searched.</param>
        /// <param name="query">The search query.</param>
        /// <returns>A List of <see cref="Tag"/>s.</returns>
        public async Task<List<Tag>> SearchTagsAsync(Snowflake guildId, string query)
        {
            var tags = await GetTagsAsync(guildId);
            return tags
                .Select(x => (Levenshtein.EditDistance(x.Name, query, 2), x))
                .Where(x => x.Item1 <= 5)
                .OrderBy(x => x.Item1)
                .Select(x => x.Item2)
                .ToList();
        }

        /// <summary>
        /// Adds a tag to the database.
        /// </summary>
        /// <param name="tag">The tag to add.</param>
        public async Task CreateTagAsync(Tag tag)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            await db.Tags.AddAsync(tag);
            await db.SaveChangesAsync();
        }
        
        /// <summary>
        /// Updates a tag.
        /// </summary>
        /// <param name="tag">
        /// The tag to update.
        /// </param>
        public async Task UpdateTagAsync(Tag tag)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            db.Tags.Update(tag);
            await db.SaveChangesAsync();
        }
        
        /// <summary>
        /// Removes a tag from the database.
        /// </summary>
        /// <param name="tag">The <see cref="Tag"/> to remove.</param>
        public async Task RemoveTagAsync(Tag tag)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            db.Tags.Attach(tag);
            db.Tags.Remove(tag);
            await db.SaveChangesAsync();
        }
    }
}
