using LearmApi.Dto;
using LearmApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LearmApi.Controllers
{
    //[Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)}")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly EntityContext _context;

        public CategoryController(EntityContext context)
        {
            _context = context;
           
        }

        
        [HttpGet]
        [Route("list-category")]
        public async Task<IActionResult> GetCategories([FromQuery] PaginationDto p)
        {
            try
            {
                var  categories = await  _context.Categories.Select(
                        c => new
                        {
                            c.Id,
                            c.Name,
                            createdAt = c.CreatedDate,
                            updatedAt = c.UpdatedDate
                        }
                    ).OrderBy(c => c.Name)
                    .Skip((p.Page - 1) * p.PageSize)
                    .Take(p.PageSize)
                    .AsNoTracking()
                    .ToListAsync();

                var total = await _context.Categories.CountAsync();

                var response = new
                {
                    mssage = "Fetch categories successfully",
                    rows = categories,
                    total,
                    hasNextPage = total > p.Page * p.PageSize,
                    p.Page,
                    p.PageSize
                };

                return Ok(response);

            }
            catch (Exception ex) {
                return StatusCode(500, "Internal server error");
            }

           
        }



        [HttpGet]
        [Route("get-category/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {

            try
            {
                var category = await _context.Categories
                    .Where(c => c.Id == id)
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        createdAt = c.CreatedDate,
                        updatedAt = c.UpdatedDate
                    })
                    .FirstOrDefaultAsync();
                if (category is null)
                {
                    return NotFound(new { statusCodes = 404, message = "Category not found" });
                }

                return Ok(new {  data = category, statusCodes = 200, message = "GetByID Category" });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(500, "Internal server error");
            }

            
        }

        [HttpPost]
        [Route("create-category")]
        public async Task<IActionResult> Create([FromBody] CategoryDto model )
        {
            try
            {
                var category =  new Categories
                {
                    Name = model.Name,

                };

                await _context.Categories.AddAsync(category);
                await _context.SaveChangesAsync();


                return Ok(new { statusCodes = 200, message = "Category created successfully" });
            }
            catch (Exception ex) {
                return StatusCode(500, "Internal server error");
            }
           
        }


        [HttpPut]
        [Route("update-category/{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CategoryDto model)
        {
            try

            {

                var categoryExist = await _context.Categories.FindAsync(id);

                if (categoryExist == null)
                {
                    return NotFound(new { msg = "CategoryId not found" });
                }
                categoryExist.Name = model.Name;
                _context.Entry(categoryExist).State = EntityState.Modified;

                await _context.SaveChangesAsync();


                return Ok(new { statusCodes = 200, message = "Update Category" });

            }


             catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(500, "Internal server error");
            }
            
        }


        [HttpDelete]
        [Route("delete-category/{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            try

            {

                var categoryExist = await _context.Categories.FindAsync(id);

                if (categoryExist == null)
                {
                    return NotFound(new { msg = "CategoryId not found" });
                }
                categoryExist.IsActive = false;
                _context.Entry(categoryExist).State = EntityState.Modified;

                await _context.SaveChangesAsync();


                return Ok(new { statusCodes = 200, message = "Delete Category successfully" });

            }


            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(500, "Internal server error");
            }

        }

    }
}
