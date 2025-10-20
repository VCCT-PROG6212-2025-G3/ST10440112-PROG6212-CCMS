using Microsoft.EntityFrameworkCore;
using ST10440112_PROG6212_CCMS.Models;

namespace ST10440112_PROG6212_CCMS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<ProgrammeCoordinator> ProgrammeCoordinators { get; set; }
        public DbSet<AcademicManager> AcademicManagers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Lecturer-Claim relationship
            modelBuilder.Entity<Claim>()
                .HasOne(c => c.Lecturer)
                .WithMany(l => l.Claims)
                .HasForeignKey(c => c.LecturerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Claim-Document relationship
            modelBuilder.Entity<Document>()
                .HasOne(d => d.Claim)
                .WithMany(c => c.Documents)
                .HasForeignKey(d => d.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Admin inheritance (TPH - Table Per Hierarchy)
            modelBuilder.Entity<Admin>()
                .UseTphMappingStrategy();

            // Seed initial data (optional for testing)
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed a sample lecturer
            var lecturerId = Guid.NewGuid();
            modelBuilder.Entity<Lecturer>().HasData(
                new Lecturer
                {
                    LecturerId = lecturerId,
                    Name = "Michael Jones",
                    Email = "michael.jones@newlands.ac.za",
                    Department = "Faculty of Science"
                }
            );

            // Seed a Programme Coordinator
            modelBuilder.Entity<ProgrammeCoordinator>().HasData(
                new ProgrammeCoordinator
                {
                    AdminID = Guid.NewGuid(),
                    AdminName = "Ebrahim Jacobs",
                    AdminEmail = "ebrahim.jacobs@newlands.ac.za",
                    MajorDegree = "Computer Science"
                }
            );

            // Seed an Academic Manager
            modelBuilder.Entity<AcademicManager>().HasData(
                new AcademicManager
                {
                    AdminID = Guid.NewGuid(),
                    AdminName = "Janet Du Plessis",
                    AdminEmail = "janet.duplessis@newlands.ac.za",
                    Faculty = "Faculty of Science"
                }
            );
        }
    }
}
