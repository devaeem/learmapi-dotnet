using LearmApi.Dto;
using LearmApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace LearmApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly EntityContext _context;


        private readonly IWebHostEnvironment _env;

        public ProductController(EntityContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;


        }

        [HttpGet]
        [Route("list-product")]
        public async Task<IActionResult> ListProduc([FromQuery] PaginationDto pagination)
        {
            try
            {
                var query = await _context.Products
            .Include(product => product.CategoryId)
            .Select(product => new
            {
                product.Id,
                product.Name,
                categoryName = product.CategoryId.Name,
                createdAt = product.CreatedDate,
                updatedAt = product.UpdatedDate
            })
            .OrderBy(product => product.Name)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .AsNoTracking()
            .ToListAsync();


                if (!string.IsNullOrEmpty(pagination.Search))
                {
                    string searchTerm = pagination.Search.ToLower();
                    query = query.Where(q => q.Name!.ToLower().Contains(searchTerm)).ToList();
                }
                var total = query.Count();

                var response = new
                {
                    mssage = "Fetch product successfully",
                    rows = query,
                    total,
                    hasNextPage = total > pagination.Page * pagination.PageSize,
                    pagination.Page,
                    pagination.PageSize
                };

                return Ok(response);
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error:{ex.Message}");
            }
        }


        [HttpGet]
        [Route("get-product/{id}")]
        public async Task<IActionResult> GetByProducId(Guid id)
        {
            try
            {
                var product = await _context.Products
    .Include(p => p.CategoryId)
    .Where(p => p.Id == id)
    .Select(p => new
    {
        p.Id,
        p.Name,
        categoryName = new
        {
            id = p.CategoryId.Id,
            name = p.CategoryId.Name
        },
        createdAt = p.CreatedDate,
        updatedAt = p.UpdatedDate
    })
    .FirstOrDefaultAsync();

                if (product is null)
                {
                    return NotFound(new { statusCodes = 404, message = "Product not found" });
                }

                return Ok(new { data = product, statusCodes = 200, message = "GetByID Product" });
            }
            catch (Exception ex)
            {

                return StatusCode(500, "Internal server error");
            }
        }



        [HttpPost]
        [Route("create-product")]
        public async Task<IActionResult> CreateProduct([FromForm] ProductDto product, IFormFile? image)
        {
            try

            {

                // Declare fileName here




                var category = await _context.Categories.FindAsync(product.CategoryId);
                if (category == null)
                {
                    return BadRequest(new { message = "หมวดหมู่ที่ระบุไม่ถูกต้อง" });
                }
                var data = new Products
                {
                    Name = product.Name,
                    Price = product.Price,
                    Description = product.Description,
                    CategoryId = category,

                };

                await _context.Products.AddAsync(data);

                if (image != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);

                    string uploadFolder = Path.Combine(_env.WebRootPath, "uploads/images");
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }
                    var filePath = Path.Combine(_env.WebRootPath, "uploads/images", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    data.Image = fileName;
                }
                else
                {
                    data.Image = "no-image.jpg";
                }
                await _context.SaveChangesAsync();


                return Ok(new { statusCodes = 200, message = "Product created successfully" });
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error:{ex.Message}");
            }
        }



        [HttpPut]
        [Route("update-product/{id}")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductDto product)
        {
            try
            {
                var productExist = await _context.Products.FindAsync(id);

                if (productExist == null)
                {
                    return NotFound(new { msg = "ProductId not found" });
                }
                productExist.Name = product.Name;
                productExist.Price = product.Price;
                productExist.Description = product.Description;
                // productExist.Image = product.Image;
                //productExist.CategoryId = product.CategoryId;
                productExist.UpdatedDate = DateTime.UtcNow;



                _context.Entry(productExist).State = EntityState.Modified;

                await _context.SaveChangesAsync();


                return Ok(new { statusCodes = 200, message = "Update Product successfully" });

            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error:{ex.Message}");
            }
        }


        [HttpDelete]
        [Route("delete-product/{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            try

            {

                var productsExist = await _context.Products.FindAsync(id);

                if (productsExist == null)
                {
                    return NotFound(new { msg = "ProductId not found" });
                }
                productsExist.IsActive = false;
                _context.Entry(productsExist).State = EntityState.Modified;

                await _context.SaveChangesAsync();


                return Ok(new { statusCodes = 200, message = "Delete Product successfully" });

            }


            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(500, "Internal server error");
            }

        }



    }
}