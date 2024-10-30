using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LearmApi.Models
{
    [Table("products")]
    public class Products : BaseModel
    {


        [MaxLength(255)]
        [Column("name", TypeName = "varchar(255)")]
        public string? Name { get; set; }

        [Column("price", TypeName = "int")]
        public int Price { get; set; }

        [MaxLength(255)]
        [Column("desc", TypeName = "varchar(255)")]
        public string? Description { get; set; }

        [MaxLength(255)]
        [Column("image", TypeName = "varchar(255)")]
        public string? Image { get; set; }


        [Column("unit", TypeName = "varchar(255)")]
        public string? Unit { get; set; }

        //[Column("category_id", TypeName = "varchar(255)")]
        //public string CategoryId { get; set; }


        [ForeignKey("category_id")]
        public Categories CategoryId { get; set; }

    }
}
