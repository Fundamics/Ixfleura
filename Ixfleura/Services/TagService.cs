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
    public class TagService : IxService
    {
        private readonly IDbContextFactory<IxfleuraDbContext> _dbContextFactory;
        
        public TagService(ILogger<TagService> logger, IDbContextFactory<IxfleuraDbContext> dbContextFactory) : base(logger)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<Tag> GetTagAsync(Snowflake guildId, string name)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            return await db.Tags.FirstOrDefaultAsync(x => x.GuildId == guildId.RawValue && EF.Functions.ILike(x.Name, name));
        }

        public async Task<List<Tag>> GetTagsAsync(Snowflake guildId)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            return await db.Tags.Where(x => x.GuildId == guildId.RawValue).ToListAsync();
        }
        
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

        public async Task CreateTagAsync(Tag tag)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            await db.Tags.AddAsync(tag);
            await db.SaveChangesAsync();
        }
        
        public async Task UpdateTagAsync(Tag tag)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            db.Tags.Update(tag);
            await db.SaveChangesAsync();
        }
        
        public async Task RemoveTagAsync(Tag tag)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            db.Tags.Attach(tag);
            db.Tags.Remove(tag);
            await db.SaveChangesAsync();
        }
    }
}