using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WEB_353502_ZGIRSKAYA.Domain.Entities;

    public class CocktailContext : DbContext
    {
        public CocktailContext (DbContextOptions<CocktailContext> options)
            : base(options)
        {
        }

        public DbSet<WEB_353502_ZGIRSKAYA.Domain.Entities.Cocktail> Cocktail { get; set; } = default!;
    }
