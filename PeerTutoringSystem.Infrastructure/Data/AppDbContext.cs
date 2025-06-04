using Microsoft.EntityFrameworkCore;
using PeerTutoringSystem.Domain.Entities.Authentication;
using PeerTutoringSystem.Domain.Entities.Booking;
using PeerTutoringSystem.Domain.Entities.Profile_Bio;
using PeerTutoringSystem.Domain.Entities.Reviews;
using PeerTutoringSystem.Domain.Entities.Skills;
using PeerTutoringSystem.Domain.Entities.PaymentEntities;

namespace PeerTutoringSystem.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
        public DbSet<TutorVerification> TutorVerifications { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<UserBio> UserBio { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<UserSkill> UserSkills { get; set; }
        public DbSet<TutorAvailability> TutorAvailabilities { get; set; }
        public DbSet<BookingSession> BookingSessions { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<PaymentEntity> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Users
            modelBuilder.Entity<User>()
                .HasKey(u => u.UserID);
            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(u => u.FirebaseUid)
                .IsUnique()
                .HasFilter("[FirebaseUid] IS NOT NULL AND [FirebaseUid] != ''");
            modelBuilder.Entity<User>()
                .Property(u => u.Gender)
                .HasConversion<string>();
            modelBuilder.Entity<User>()
                .Property(u => u.Status)
                .HasConversion<string>()
                .HasDefaultValue(UserStatus.Active);
            modelBuilder.Entity<User>()
                .Property(u => u.School)
                .HasMaxLength(255)
                .IsRequired(false);
            modelBuilder.Entity<User>()
                .Property(u => u.AvatarUrl)
                .HasMaxLength(255)
                .IsRequired(false);
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleID);

            // Roles
            modelBuilder.Entity<Role>()
                .HasKey(r => r.RoleID);
            modelBuilder.Entity<Role>()
                .Property(r => r.RoleName)
                .IsRequired()
                .HasMaxLength(50);
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.RoleName)
                .IsUnique();
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleID = 1, RoleName = "Student" },
                new Role { RoleID = 2, RoleName = "Tutor" },
                new Role { RoleID = 3, RoleName = "Admin" }
            );

            // UserTokens
            modelBuilder.Entity<UserToken>()
                .HasKey(t => t.TokenID);
            modelBuilder.Entity<UserToken>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserID);

            // TutorVerifications
            modelBuilder.Entity<TutorVerification>()
                .HasKey(tv => tv.VerificationID);
            modelBuilder.Entity<TutorVerification>()
                .HasOne(tv => tv.User)
                .WithMany()
                .HasForeignKey(tv => tv.UserID);
            modelBuilder.Entity<TutorVerification>()
                .HasIndex(tv => tv.CitizenID)
                .IsUnique();
            modelBuilder.Entity<TutorVerification>()
                .HasIndex(tv => tv.StudentID)
                .IsUnique();

            // Documents
            modelBuilder.Entity<Document>()
                .HasKey(d => d.DocumentID);
            modelBuilder.Entity<Document>()
                .HasOne(d => d.TutorVerification)
                .WithMany()
                .HasForeignKey(d => d.VerificationID);

            // UserBio
            modelBuilder.Entity<UserBio>()
                .HasKey(p => p.BioID);
            modelBuilder.Entity<UserBio>()
                .Property(p => p.HourlyRate)
                .HasColumnType("DECIMAL(18,2)")
                .HasDefaultValue(0.00);
            modelBuilder.Entity<UserBio>()
                .Property(p => p.Bio)
                .IsRequired(false);
            modelBuilder.Entity<UserBio>()
                .Property(p => p.Experience)
                .IsRequired(false);
            modelBuilder.Entity<UserBio>()
                .Property(p => p.Availability)
                .IsRequired(false);
            modelBuilder.Entity<UserBio>()
                .HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<UserBio>(p => p.UserID)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Configure Skill entity
            modelBuilder.Entity<Skill>()
                .ToTable("Skills")
                .HasKey(s => s.SkillID);

            modelBuilder.Entity<Skill>()
                .Property(s => s.SkillID)
                .HasDefaultValueSql("NEWID()");

            modelBuilder.Entity<Skill>()
                .Property(s => s.SkillName)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Skill>()
                .HasIndex(s => s.SkillName)
                .IsUnique();

            modelBuilder.Entity<Skill>()
                .Property(s => s.SkillLevel)
                .HasMaxLength(50);

            modelBuilder.Entity<Skill>()
                .Property(s => s.Description)
                .HasMaxLength(500);

            // Configure UserSkill entity
            modelBuilder.Entity<UserSkill>()
                .ToTable("UserSkills")
                .HasKey(us => us.UserSkillID);

            modelBuilder.Entity<UserSkill>()
                .Property(us => us.UserSkillID)
                .HasDefaultValueSql("NEWID()");

            modelBuilder.Entity<UserSkill>()
                .HasOne(us => us.User)
                .WithMany()
                .HasForeignKey(us => us.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserSkill>()
                .HasOne(us => us.Skill)
                .WithMany()
                .HasForeignKey(us => us.SkillID)
                .OnDelete(DeleteBehavior.Cascade);
            // Configure BookingSession entity
            modelBuilder.Entity<BookingSession>(entity =>
            {
                entity.HasKey(e => e.BookingId);
                entity.Property(e => e.Topic).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Status).HasConversion<string>();
            });

            // Configure TutorAvailability entity
            modelBuilder.Entity<TutorAvailability>(entity =>
            {
                entity.HasKey(e => e.AvailabilityId);
                entity.Property(e => e.RecurringDay).HasConversion<string>();
                entity.Property(e => e.IsDailyRecurring).HasDefaultValue(false);
            });

            // Reviews configuration
            modelBuilder.Entity<Review>()
                .HasKey(r => r.ReviewID);

            modelBuilder.Entity<Review>()
                .Property(r => r.Rating)
                .IsRequired();

            modelBuilder.Entity<Review>()
                .Property(r => r.Comment)
                .HasMaxLength(500)
                .IsRequired(false);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Booking)
                .WithMany()
                .HasForeignKey(r => r.BookingID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Student)
                .WithMany()
                .HasForeignKey(r => r.StudentID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Tutor)
                .WithMany()
                .HasForeignKey(r => r.TutorID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure PaymentEntity
            modelBuilder.Entity<PaymentEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasColumnType("DECIMAL(18,2)");
                entity.Property(e => e.Currency).HasMaxLength(10);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.TransactionId).HasMaxLength(100);
                entity.Property(e => e.Status).HasConversion<string>();
                entity.HasOne(e => e.Booking)
                      .WithMany()
                      .HasForeignKey(e => e.BookingId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }

}