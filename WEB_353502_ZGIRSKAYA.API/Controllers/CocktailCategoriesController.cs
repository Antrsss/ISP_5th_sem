using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_353502_ZGIRSKAYA.API.Data;
using WEB_353502_ZGIRSKAYA.Domain.Entities;

namespace WEB_353502_ZGIRSKAYA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CocktailCategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CocktailCategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/CocktailCategories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CocktailCategory>>> GetCocktailCategories()
        {
            return await _context.CocktailCategories.ToListAsync();
        }

        // GET: api/CocktailCategories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CocktailCategory>> GetCocktailCategory(int id)
        {
            var cocktailCategory = await _context.CocktailCategories.FindAsync(id);

            if (cocktailCategory == null)
            {
                return NotFound();
            }

            return cocktailCategory;
        }

        // PUT: api/CocktailCategories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCocktailCategory(int id, CocktailCategory cocktailCategory)
        {
            if (id != cocktailCategory.Id)
            {
                return BadRequest();
            }

            _context.Entry(cocktailCategory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CocktailCategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/CocktailCategories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CocktailCategory>> PostCocktailCategory(CocktailCategory cocktailCategory)
        {
            _context.CocktailCategories.Add(cocktailCategory);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCocktailCategory", new { id = cocktailCategory.Id }, cocktailCategory);
        }

        // DELETE: api/CocktailCategories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCocktailCategory(int id)
        {
            var cocktailCategory = await _context.CocktailCategories.FindAsync(id);
            if (cocktailCategory == null)
            {
                return NotFound();
            }

            _context.CocktailCategories.Remove(cocktailCategory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CocktailCategoryExists(int id)
        {
            return _context.CocktailCategories.Any(e => e.Id == id);
        }
    }
}
