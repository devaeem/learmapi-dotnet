using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearmApi.Models
{

    [Table("categories")]
    public class Categories : BaseModel
    {
        [MaxLength(255)]
        [Column("name", TypeName = "varchar(255)")]
        public string Name { get; set; } = null!;


        public ICollection<Products>? Products { get; set; }
    }
}
