using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Ixfleura.Data
{
    public class IxfleuraDbContextFactory : IDesignTimeDbContextFactory<IxfleuraDbContext>
    {
        public IxfleuraDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile(Directory.GetCurrentDirectory() + "/../Ixfleura/config.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<IxfleuraDbContext>();
            optionsBuilder.UseNpgsql(config["database:connection"])
                .UseSnakeCaseNamingConvention();

            return new IxfleuraDbContext(optionsBuilder.Options);
        }
    }
}