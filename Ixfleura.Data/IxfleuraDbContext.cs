using System;
using Microsoft.EntityFrameworkCore;

namespace Ixfleura.Data
{
    public class IxfleuraDbContext : DbContext
    {
        public IxfleuraDbContext(DbContextOptions<IxfleuraDbContext> options) : base(options)
        { }
    }
}