using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LearmApi.Controllers
{


    public class InfoDto
    {
        public string? Name { get; set; }
        public int? Age { get; set; }
    }





    [Route("api/v1/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {

     

        [HttpGet]
        [Route("[action]")]
        public IActionResult  Listbook() {
        
          return Ok(new { message = "index"});
        
        }

        [HttpGet]
        [Route("Info")]
        public IActionResult Info([FromQuery] InfoDto dto)
        {

            return Ok(new { message = "Info", name = dto.Name, age = dto.Age });

        }


    }
}
