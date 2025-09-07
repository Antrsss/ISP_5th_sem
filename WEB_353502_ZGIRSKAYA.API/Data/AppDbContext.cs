using Microsoft.EntityFrameworkCore;
using WEB_353502_ZGIRSKAYA.Domain.Entities;

namespace WEB_353502_ZGIRSKAYA.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Cocktail> Cocktails { get; set; }
        public DbSet<CocktailCategory> CocktailCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Cocktail>()
                .HasOne(c => c.Category)
                .WithMany();
        }
    }
}