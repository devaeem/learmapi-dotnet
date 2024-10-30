using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LearmApi.Dto
{
    public class PaginationDto
    {
        [Required(ErrorMessage = "page ห้ามเป็นค่าว่างต้องระบุเป็นตัวเลข 1 ถึง 9999 เท่านั้น")]
        [Range(1, 9999, ConvertValueInInvariantCulture = false, ErrorMessage = "ต้องระบุเป็นตัวเลข 1 ถึง 9999 เท่านั้น")]
        [FromQuery(Name = "page")]
        [Display(Name = "page must be a Required")]
        public int Page { get; set; }


        [Required(ErrorMessage = "pageSize ห้ามเป็นค่าว่างต้องระบุเป็นตัวเลข 1 ถึง 9999 เท่านั้น")]
        [Range(1, 9999, ConvertValueInInvariantCulture = false, ErrorMessage = "ต้องระบุเป็นตัวเลข 1 ถึง 9999 เท่านั้น")]
        [FromQuery(Name = "pageSize")]
        [Display(Name = "pageSize must be a Required")]
        public int PageSize { get; set; }



        public string? Search { get; set; }
    }
}
