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
                .HasOne(i => i.Appointment)
                .WithMany()
                .HasForeignKey(i => i.AppointmentId)
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

            // Seed Users
            var user1 = new User
            {
                Id = 1,
                Username = "admin",
                Role = "Admin",
                FacilityId = 1
            };

            var user2 = new User
            {
                Id = 2,
                Username = "superadmin",
                Role = "SuperAdmin",
                FacilityId = 1
            };

            modelBuilder.Entity<User>().HasData(user1, user2);

            // Seed Clinics
            var clinic1 = new Clinic { Id = 1, Name = "Lekki Phase 1 Branch", FacilityId = 1 };
            var clinic2 = new Clinic { Id = 2, Name = "Ikeja Branch", FacilityId = 1 };
            var clinic3 = new Clinic { Id = 3, Name = "Victoria Island Branch", FacilityId = 1 };

            modelBuilder.Entity<Clinic>().HasData(clinic1, clinic2, clinic3);

            // Seed Appointment Types
            var appointmentType1 = new AppointmentType { Id = 1, Name = "Consultation", DefaultDurationMinutes = 30, FacilityId = 1 };
            var appointmentType2 = new AppointmentType { Id = 2, Name = "Follow-up", DefaultDurationMinutes = 15, FacilityId = 1 };
            var appointmentType3 = new AppointmentType { Id = 3, Name = "Check-up", DefaultDurationMinutes = 60, FacilityId = 1 };

            modelBuilder.Entity<AppointmentType>().HasData(appointmentType1, appointmentType2, appointmentType3);

            // Seed Patients (25 patients)
            var patients = new List<Patient>();
            var patientNames = new[]
            {
               "Gadu Abdulrasheed", "Olakunbi Oshin", "	Divine Orisejobor", "Pokhae P", "Segun Dada", "Zainab Usman", "Obinna Eke", "Ifẹyínwa Adekunle", "Babatunde Kolawole", 
               "Oshin Ola", "Kelechi Nwachukwu", "Funmilayo Awolowo", "Musa Abubakar", "Amara Ozo", "Ayodele Oladeji", "Chiamaka Nwosu", "Jide Owolabi",
                "Fatima Danjuma", "Emeka Okafor", "Ibukunoluwa Shittu", "Idris Aliyu", "Chioma Agwu", "Oluwafemi Johnson", "Kemi Obafemi", "Abdulrahman Adamu"
            };
            
            var statuses = new[] { "Processing", "Awaiting Vitals", "Processing", "Awaiting Vitals" };
            var random = new Random(42); // Fixed seed for consistent data

            for (int i = 0; i < 25; i++)
            {
                patients.Add(new Patient
                {
                    Id = i + 1,
                    Code = $"P{(i + 1):D3}",
                    Name = patientNames[i],
                    Phone = $"+2341234567{i:D3}",
                    WalletBalance = random.Next(10000, 100000) + (decimal)(random.NextDouble() * 100),
                    Currency = "NGN",
                    FacilityId = 1,
                    Status = statuses[i % statuses.Length]
                });
            }

            modelBuilder.Entity<Patient>().HasData(patients);

            // Seed Appointments (10 appointments)
            var appointments = new List<Appointment>();
            var baseTime = DateTime.Now;
            // Reuse the random instance from above

            for (int i = 0; i < 10; i++)
            {
                var patientId = random.Next(1, 26); // Random patient from 1-25
                var clinicId = random.Next(1, 4); // Random clinic from 1-3
                var appointmentTypeId = random.Next(1, 4); // Random appointment type from 1-3
                var hoursOffset = random.Next(-24, 72); // Appointments from yesterday to 3 days ahead
                var appointmentTime = baseTime.AddHours(hoursOffset).AddMinutes(random.Next(0, 60));
                
                // Get duration from appointment type
                var duration = appointmentTypeId == 1 ? 30 : appointmentTypeId == 2 ? 15 : 60;

                appointments.Add(new Appointment
                {
                    Id = i + 1,
                    PatientId = patientId,
                    ClinicId = clinicId,
                    AppointmentTypeId = appointmentTypeId,
                    StartTime = appointmentTime,
                    DurationMinutes = duration,
                    Status = "Scheduled",
                    FacilityId = 1,
                    CreatedBy = 1,
                    CreatedAt = baseTime.AddDays(-random.Next(0, 7)) // Created within last week
                });
            }

            modelBuilder.Entity<Appointment>().HasData(appointments);
        }
    }
}

