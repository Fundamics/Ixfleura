using Ixfleura.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ixfleura.Data
{
    public class IxfleuraDbContext : DbContext
    {
        public IxfleuraDbContext(DbContextOptions<IxfleuraDbContext> options) : base(options)
        { }
        
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Suggestion> Suggestions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // modelBuilder.UseValueConverterForType<Snowflake>(new ValueConverter<Snowflake, ulong>(x => x.RawValue, x => new Snowflake(x)));
            modelBuilder.Entity<Tag>().HasKey("GuildId", "Name");
        }
    }
}