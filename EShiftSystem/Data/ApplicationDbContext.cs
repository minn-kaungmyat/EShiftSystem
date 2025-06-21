using EShiftSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EShiftSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Load> Loads { get; set; }
        public DbSet<TransportUnit> TransportUnits { get; set; }
        public DbSet<Lorry> Lorries { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Assistant> Assistants { get; set; }
        public DbSet<Container> Containers { get; set; }
        //public DbSet<Product> Products { get; set; }

        public DbSet<LoadItem> LoadItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //// Configure TPH inheritance for Users
            //modelBuilder.Entity<User>()
            //    .HasDiscriminator<UserRole>("Role")
            //    .HasValue<Admin>(UserRole.Admin)
            //    .HasValue<Customer>(UserRole.Customer);

            // Configure Job entity
            modelBuilder.Entity<Job>()
                .HasIndex(j => j.JobNumber)
                .IsUnique();

            // One Customer has many Jobs
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Jobs)
                .WithOne(j => j.Customer)
                .HasForeignKey(j => j.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // One Job has many Loads
            modelBuilder.Entity<Job>()
                .HasMany(j => j.Loads)
                .WithOne(l => l.Job)
                .HasForeignKey(l => l.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            // One Load has many Products
            //modelBuilder.Entity<Load>()
            //    .HasMany(l => l.Products)
            //    .WithOne(p => p.Load)
            //    .HasForeignKey(p => p.LoadId)
            //    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Load>()
               .HasMany(l => l.LoadItems) // Changed from l.Products
               .WithOne(p => p.Load)
               .HasForeignKey(p => p.LoadId)
               .OnDelete(DeleteBehavior.Cascade);

            // One Load has one TransportUnit
            modelBuilder.Entity<Load>()
                .HasOne(l => l.TransportUnit)
                .WithMany()
                .HasForeignKey(l => l.TransportUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            // One TransportUnit has one Lorry
            modelBuilder.Entity<TransportUnit>()
                .HasOne(t => t.Lorry)
                .WithMany()
                .HasForeignKey(t => t.LorryId)
                .OnDelete(DeleteBehavior.Restrict);

            // One TransportUnit has one Driver
            modelBuilder.Entity<TransportUnit>()
                .HasOne(t => t.Driver)
                .WithMany()
                .HasForeignKey(t => t.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            // One TransportUnit has one Container
            modelBuilder.Entity<TransportUnit>()
                .HasOne(t => t.Container)
                .WithMany()
                .HasForeignKey(t => t.ContainerId)
                .OnDelete(DeleteBehavior.Restrict);

            // One TransportUnit has many Assistants
            modelBuilder.Entity<TransportUnit>()
                .HasMany(t => t.Assistants)
                .WithOne(a => a.TransportUnit)
                .HasForeignKey(a => a.TransportUnitId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }

}
