using Microsoft.EntityFrameworkCore;
using SimpleCRM.Models;
using SimpleCRM.Services;

namespace SimpleCRM.Data
{
    public class CrmDbContext : DbContext
    {
        public CrmDbContext(DbContextOptions<CrmDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Suppress the non-deterministic model warning
                optionsBuilder.ConfigureWarnings(warnings =>
                    warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            }
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<Timesheet> Timesheets { get; set; }
        public DbSet<TimeEntry> TimeEntries { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure global soft delete filter
            modelBuilder.Entity<Customer>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<User>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<Agent>().HasQueryFilter(e => e.DeletedAt == null);

            // Configure Customer entity
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("PENDING");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                
                // Configure relationship with Agent
                entity.HasOne(e => e.Agent)
                    .WithOne(a => a.User)
                    .HasForeignKey<User>(e => e.AgentId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                // Create unique index on username and email
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.AgentId).IsUnique();
            });

            // Configure Agent entity
            modelBuilder.Entity<Agent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Department).HasMaxLength(50);
                entity.Property(e => e.Position).HasMaxLength(50);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
            });

            // Configure Timesheet entity
            modelBuilder.Entity<Timesheet>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ProjectName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Category).HasMaxLength(50);
                entity.Property(e => e.ApprovalComments).HasMaxLength(500);
                entity.Property(e => e.RejectionReason).HasMaxLength(500);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("DRAFT");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                
                // Configure relationships
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Timesheets)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.ApprovedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.ApprovedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Add indexes for better performance
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.Status);
            });

            // Configure TimeEntry entity
            modelBuilder.Entity<TimeEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ProjectName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Category).HasMaxLength(50);
                entity.Property(e => e.CustomerName).HasMaxLength(100);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                
                // Configure relationships
                entity.HasOne(e => e.Timesheet)
                    .WithMany(t => t.TimeEntries)
                    .HasForeignKey(e => e.TimesheetId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Add indexes
                entity.HasIndex(e => e.TimesheetId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Date);
            });

            // Configure AuditLog entity
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Details).HasMaxLength(1000);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                
                // Configure relationships
                entity.HasOne(e => e.User)
                    .WithMany(u => u.AuditLogs)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Add indexes
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Action);
                entity.HasIndex(e => e.EntityType);
                entity.HasIndex(e => e.CreatedDate);
            });

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            var seedDate = new DateTime(2024, 1, 1); // Static date to avoid non-deterministic model

            // Use plain text passwords for seed data (PasswordService will handle both BCrypt and plain text)
            var adminPasswordHash = "admin123"; // Will be handled by hybrid PasswordService
            var agentPasswordHash = "agent123"; // Will be handled by hybrid PasswordService

            // Seed Admin User (admin/admin123)
            var adminUser = new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@crm.local",
                Password = adminPasswordHash,
                Role = "Admin",
                Status = "ACTIVE",
                EmailVerified = true,
                AgentId = null, // Admin is not an agent
                CreatedDate = seedDate
            };

            // Seed Agent User (agent/agent123)
            var agentUser = new User
            {
                Id = 2,
                Username = "agent",
                Email = "agent@crm.local",
                Password = agentPasswordHash,
                Role = "User",
                Status = "ACTIVE",
                EmailVerified = true,
                AgentId = 1, // Linked to agent
                CreatedDate = seedDate
            };

            modelBuilder.Entity<User>().HasData(adminUser, agentUser);

            // Seed the one Agent (linked to user account)
            var agent = new Agent
            {
                Id = 1,
                Name = "Agent User",
                Email = "agent@crm.local",
                Phone = "555-0100",
                Department = "Sales",
                Position = "Agent",
                HireDate = seedDate.AddMonths(-6),
                IsActive = true,
                CreatedDate = seedDate
            };

            modelBuilder.Entity<Agent>().HasData(agent);

            // Seed some sample customers
            var customers = new List<Customer>();
            for (int i = 1; i <= 20; i++)
            {
                customers.Add(new Customer
                {
                    Id = i,
                    Name = $"Customer {i}",
                    Email = $"customer{i}@example.com",
                    Phone = $"555-{1000 + i:D4}",
                    IsActive = i % 4 != 0, // Most active, some inactive
                    CreatedDate = seedDate.AddDays(-i)
                });
            }

            modelBuilder.Entity<Customer>().HasData(customers);

            // Seed some sample timesheets for the agent
            var timesheets = new List<Timesheet>();
            for (int i = 1; i <= 10; i++)
            {
                timesheets.Add(new Timesheet
                {
                    Id = i,
                    UserId = 2, // Agent user
                    Date = seedDate.AddDays(-i),
                    HoursWorked = Math.Round(6.0 + (i % 4) * 0.5, 1),
                    Description = $"Work completed on {seedDate.AddDays(-i):MMM dd}",
                    ProjectName = "Customer Project",
                    Category = "Development",
                    Status = i % 3 == 0 ? "PENDING" : "APPROVED",
                    IsBillable = true,
                    StartTime = seedDate.AddDays(-i).AddHours(9),
                    EndTime = seedDate.AddDays(-i).AddHours(9 + (int)Math.Round(6.0 + (i % 4) * 0.5)),
                    CreatedDate = seedDate.AddDays(-i)
                });
            }

            modelBuilder.Entity<Timesheet>().HasData(timesheets);
        }
    }
}
