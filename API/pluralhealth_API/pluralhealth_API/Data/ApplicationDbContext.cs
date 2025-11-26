using Microsoft.EntityFrameworkCore;
using pluralhealth_API.Models;

namespace pluralhealth_API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Facility> Facilities { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<AppointmentType> AppointmentTypes { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.Facility)
                .WithMany()
                .HasForeignKey(p => p.FacilityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Clinic>()
                .HasOne(c => c.Facility)
                .WithMany()
                .HasForeignKey(c => c.FacilityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AppointmentType>()
                .HasOne(at => at.Facility)
                .WithMany()
                .HasForeignKey(at => at.FacilityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Clinic)
                .WithMany(c => c.Appointments)
                .HasForeignKey(a => a.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.AppointmentType)
                .WithMany(at => at.Appointments)
                .HasForeignKey(a => a.AppointmentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Facility)
                .WithMany()
                .HasForeignKey(a => a.FacilityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Patient)
                .WithMany(p => p.Invoices)
                .HasForeignKey(i => i.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Facility)
                .WithMany()
                .HasForeignKey(i => i.FacilityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InvoiceItem>()
                .HasOne(ii => ii.Invoice)
                .WithMany(i => i.Items)
                .HasForeignKey(ii => ii.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Invoice)
                .WithMany(i => i.Payments)
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Facility)
                .WithMany()
                .HasForeignKey(p => p.FacilityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            modelBuilder.Entity<Appointment>()
                .HasIndex(a => new { a.FacilityId, a.StartTime });

            modelBuilder.Entity<Patient>()
                .HasIndex(p => new { p.FacilityId, p.Code });

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.InvoiceNumber)
                .IsUnique()
                .HasFilter("[InvoiceNumber] IS NOT NULL");

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Facility
            var facility1 = new Facility
            {
                Id = 1,
                Name = "Main Clinic - Lekki, Lagos",
                Timezone = "Africa/Lagos",
                Currency = "NGN"
            };

            modelBuilder.Entity<Facility>().HasData(facility1);

            // Seed User
            var user1 = new User
            {
                Id = 1,
                Username = "admin",
                Role = "FrontDesk",
                FacilityId = 1
            };

            modelBuilder.Entity<User>().HasData(user1);

            // Seed Clinics
            var clinic1 = new Clinic { Id = 1, Name = "General Medicine", FacilityId = 1 };
            var clinic2 = new Clinic { Id = 2, Name = "Pediatrics", FacilityId = 1 };
            var clinic3 = new Clinic { Id = 3, Name = "Cardiology", FacilityId = 1 };

            modelBuilder.Entity<Clinic>().HasData(clinic1, clinic2, clinic3);

            // Seed Appointment Types
            var appointmentType1 = new AppointmentType { Id = 1, Name = "Consultation", DefaultDurationMinutes = 30, FacilityId = 1 };
            var appointmentType2 = new AppointmentType { Id = 2, Name = "Follow-up", DefaultDurationMinutes = 15, FacilityId = 1 };
            var appointmentType3 = new AppointmentType { Id = 3, Name = "Check-up", DefaultDurationMinutes = 60, FacilityId = 1 };

            modelBuilder.Entity<AppointmentType>().HasData(appointmentType1, appointmentType2, appointmentType3);

            // Seed Patients
            var patient1 = new Patient
            {
                Id = 1,
                Code = "P001",
                Name = "John Doe",
                Phone = "+2341234567890",
                WalletBalance = 50000.00m,
                Currency = "NGN",
                FacilityId = 1,
                Status = "Processing"
            };

            var patient2 = new Patient
            {
                Id = 2,
                Code = "P002",
                Name = "Jane Smith",
                Phone = "+2341234567891",
                WalletBalance = 75000.50m,
                Currency = "NGN",
                FacilityId = 1,
                Status = "Processing"
            };

            var patient3 = new Patient
            {
                Id = 3,
                Code = "P003",
                Name = "Bob Johnson",
                Phone = "+2341234567892",
                WalletBalance = 20000.00m,
                Currency = "NGN",
                FacilityId = 1,
                Status = "Awaiting Vitals"
            };

            modelBuilder.Entity<Patient>().HasData(patient1, patient2, patient3);

            // Seed Appointments
            var appointment1 = new Appointment
            {
                Id = 1,
                PatientId = 1,
                ClinicId = 1,
                AppointmentTypeId = 1,
                StartTime = DateTime.Now.AddHours(2),
                DurationMinutes = 30,
                Status = "Scheduled",
                FacilityId = 1,
                CreatedBy = 1,
                CreatedAt = DateTime.Now
            };

            var appointment2 = new Appointment
            {
                Id = 2,
                PatientId = 2,
                ClinicId = 2,
                AppointmentTypeId = 3,
                StartTime = DateTime.Now.AddHours(4),
                DurationMinutes = 60,
                Status = "Scheduled",
                FacilityId = 1,
                CreatedBy = 1,
                CreatedAt = DateTime.Now
            };

            modelBuilder.Entity<Appointment>().HasData(appointment1, appointment2);
        }
    }
}

