using Microsoft.EntityFrameworkCore;
using ClinicQueueSystem.Models;

namespace ClinicQueueSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Admin> Admins { get; set; }
    }
}
