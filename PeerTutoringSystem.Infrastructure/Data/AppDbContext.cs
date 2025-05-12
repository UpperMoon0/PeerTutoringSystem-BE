using Microsoft.EntityFrameworkCore;
using PeerTutoringSystem.Domain.Entities.Authentication;
using System;

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
            // Chỉ áp dụng chỉ mục duy nhất cho FirebaseUid khi nó không rỗng
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
        }
    }
}