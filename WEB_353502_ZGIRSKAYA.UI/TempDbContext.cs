using Microsoft.EntityFrameworkCore;
using WEB_353502_ZGIRSKAYA.Domain.Entities;

public class TempDbContext : DbContext
{
    public DbSet<Cocktail> Cocktails { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder
   optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlite("");
    }
}