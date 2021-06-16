using Ixfleura.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ixfleura.Data
{
    /// <summary>
    /// The <see cref="DbContext"/> for IxFleura.
    /// </summary>
    public class IxfleuraDbContext : DbContext
    {
        public IxfleuraDbContext(DbContextOptions<IxfleuraDbContext> options) : base(options)
        { }
        
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<Suggestion> Suggestions { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // modelBuilder.UseValueConverterForType<Snowflake>(new ValueConverter<Snowflake, ulong>(x => x.RawValue, x => new Snowflake(x)));
            modelBuilder.Entity<Tag>().HasKey("GuildId", "Name");
            modelBuilder.Entity<Campaign>().HasCheckConstraint("campaigns_type_lowercase_ck", "type = lower(type)");
        }
    }
}
