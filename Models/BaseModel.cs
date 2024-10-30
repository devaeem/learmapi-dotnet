using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LearmApi.Models
{
    public class BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id", TypeName = "uuid")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("is_Active", TypeName = "boolean")]
        public bool IsActive { get; set; } = true;


        [Column("createdAt", TypeName = "timestamp with time zone")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;


        [Column("updatedAt", TypeName = "timestamp with time zone")]
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;


    }
}
