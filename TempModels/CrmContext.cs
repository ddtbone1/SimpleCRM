using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SimpleCRM.TempModels;

public partial class CrmContext : DbContext
{
    public CrmContext()
    {
    }

    public CrmContext(DbContextOptions<CrmContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Agent> Agents { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<EfmigrationsLock> EfmigrationsLocks { get; set; }

    public virtual DbSet<Timesheet> Timesheets { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlite("Data Source=crm.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Agent>(entity =>
        {
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
        });

        modelBuilder.Entity<EfmigrationsLock>(entity =>
        {
            entity.ToTable("__EFMigrationsLock");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Timesheet>(entity =>
        {
            entity.HasIndex(e => e.ApprovedByUserId, "IX_Timesheets_ApprovedByUserId");

            entity.HasIndex(e => e.UserId, "IX_Timesheets_UserId");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");

            entity.HasOne(d => d.ApprovedByUser).WithMany(p => p.TimesheetApprovedByUsers)
                .HasForeignKey(d => d.ApprovedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.User).WithMany(p => p.TimesheetUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "IX_Users_Email").IsUnique();

            entity.HasIndex(e => e.Username, "IX_Users_Username").IsUnique();

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
