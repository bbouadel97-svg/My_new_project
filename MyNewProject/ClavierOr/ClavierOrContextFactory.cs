using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClavierOr;

public class ClavierOrContextFactory : IDesignTimeDbContextFactory<ClavierOrContext>
{
    public ClavierOrContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ClavierOrContext>();
        ClavierOrContext.ConfigureSqlite(optionsBuilder);

        return new ClavierOrContext(optionsBuilder.Options);
    }
}
