using Blog.Data;
using Blog.Models;
using BlogApi.Extensions;
using BlogApi.ViewModels;
using BlogApi.ViewModels.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BlogApi.Controllers
{
    [ApiController]
    public class CategoryController : ControllerBase
    {
        [HttpGet("v1/categories")]
        public async Task<IActionResult> GetAsync(
            [FromServices]BlogDataContext context,
            [FromServices]IMemoryCache cache)
        {
            try
            {
                 var categories = cache.GetOrCreate("CategoriesCache", entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                    return context.Categories.ToList();
                });
                
                return Ok(new ResultViewModel<List<Category>>(categories));
            }            
            catch
            {
                return StatusCode(500, new ResultViewModel<List<Category>>("05X16 - Falha interna no servidor"));
            }
        }        
    
        [HttpGet("v1/categories/{id:int}")]
        public async Task<IActionResult> GetByIdAsync(
            [FromRoute] int id,
            [FromServices]BlogDataContext context)
        {            
            try
            {
                var category = await context.Categories.FirstOrDefaultAsync(x=>x.Id == id);

                if (category == null)
                    return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado!"));

                return Ok(new ResultViewModel<Category>(category));
            }            
            catch
            {
                return StatusCode(500, new ResultViewModel<Category>("05X15 - Falha interna no servidor!"));
            }
        } 

        [HttpPost("v1/categories")]
        public async Task<IActionResult> PostAsync(
            [FromBody] EditorCategoryViewModel model,
            [FromServices]BlogDataContext context)
        {      
            if (!ModelState.IsValid) 
                return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));

            try
            {                
                var category = new Category
                {
                    Id = 0,                   
                    Name = model.Name,
                    Slug = model.Slug.ToLower(),
                };

                await context.Categories.AddAsync(category);
                await context.SaveChangesAsync();
                return Created($"v1/categories/{category.Id}", new ResultViewModel<Category>(category));
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new ResultViewModel<Category>("05X09 - Não foi possível incluir a categoria!"));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<Category>("05X10 - Falha interna no servidor"));
            }
        } 

        private List<Category> GetCategories(BlogDataContext context)
        {
            return context.Categories.ToList();
        }

        [HttpPut("v1/categories/{id:int}")]
        public async Task<IActionResult> PutdAsync(
            [FromRoute] int id,
            [FromBody] EditorCategoryViewModel model,
            [FromServices]BlogDataContext context)
        {     
            if (!ModelState.IsValid) 
                return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));           
            try
            {
                var category = await context.Categories.FirstOrDefaultAsync(x=>x.Id == id);

                if (category == null)
                    return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado!"));

                category.Name = model.Name;
                category.Slug = model.Slug;
                context.Categories.Update(category);
                await context.SaveChangesAsync();
                return Ok(new ResultViewModel<Category>(category));
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new ResultViewModel<Category>("05X11 - Não foi possível alterar a categoria!"));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<Category>("05X12 - Falha interna no servidor"));
            }
        } 
        

        [HttpDelete("v1/categories/{id:int}")]
        public async Task<IActionResult> DeletedAsync(
            [FromRoute] int id,
            [FromServices]BlogDataContext context)
        {                  
            try
            {
                var category = await context.Categories.FirstOrDefaultAsync(x=>x.Id == id);

                if (category == null)
                    return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado!"));

                context.Categories.Remove(category);
                await context.SaveChangesAsync();
                return Ok(new ResultViewModel<Category>(category));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ResultViewModel<Category>("05X13 - Não foi possível deletar a categoria!"));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<Category>("05X14 - Falha interna no servidor"));
            }
        } 
    }
}