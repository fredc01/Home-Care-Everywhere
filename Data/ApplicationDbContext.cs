using HomeCarePH.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeCarePH.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Household> Households { get; set; }
    }
}