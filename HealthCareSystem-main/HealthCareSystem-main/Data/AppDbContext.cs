using Microsoft.EntityFrameworkCore;
using HealthCareSystem.Models;

namespace HealthCareSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        public DbSet<Admin> Admins { get; set; }

        public DbSet<Hospital> Hospitals { get; set; }

        public DbSet<Prescription> Prescriptions { get; set; }

    }
}
