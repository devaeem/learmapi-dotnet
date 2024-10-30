using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LearmApi.Models
{
    public class EntityContext : IdentityDbContext<IdentityUser>

    {
        public EntityContext(DbContextOptions<EntityContext> options) : base(options)
        {
        }
           public virtual DbSet<Categories> Categories { get; set; }

           public virtual DbSet<Products> Products { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

    }
}
