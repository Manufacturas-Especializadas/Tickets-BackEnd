using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Tickets.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TkCategory> TkCategories { get; set; }

    public virtual DbSet<TkStatus> TkStatuses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC0702A7FA90");

            entity.HasIndex(e => e.Name, "UQ__Roles__72E12F1B1392D0A4").IsUnique();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tickets__3214EC077ADC9603");

            entity.Property(e => e.Affair)
                .HasMaxLength(80)
                .IsUnicode(false)
                .HasColumnName("affair");
            entity.Property(e => e.CategoryId).HasColumnName("categoryId");
            entity.Property(e => e.Department)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("department");
            entity.Property(e => e.Name)
                .HasMaxLength(70)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.ProblemDescription)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("problemDescription");
            entity.Property(e => e.RegistrationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("registrationDate");
            entity.Property(e => e.ResolutionDate)
                .HasColumnType("datetime")
                .HasColumnName("resolutionDate");
            entity.Property(e => e.StatusId).HasColumnName("statusId");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.Category).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Tickets__categor__0B91BA14");

            entity.HasOne(d => d.Status).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Tickets__statusI__0C85DE4D");

            entity.HasOne(d => d.User).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Tickets__userId__14270015");
        });

        modelBuilder.Entity<TkCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TkCatego__3214EC072739ECA8");

            entity.ToTable("TkCategory");

            entity.Property(e => e.Name)
                .HasMaxLength(70)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<TkStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TkStatus__3214EC074E9DE49C");

            entity.ToTable("TkStatus");

            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC0758AE4EFC");

            entity.HasIndex(e => e.PayRollNumber, "UQ__Users__9EAAFED558AF6E1C").IsUnique();

            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnName("passwordHash");
            entity.Property(e => e.PayRollNumber).HasColumnName("payRollNumber");
            entity.Property(e => e.RefreshToken)
                .HasMaxLength(256)
                .HasColumnName("refreshToken");
            entity.Property(e => e.RefreshTokenExpiryTime).HasColumnName("refreshTokenExpiryTime");
            entity.Property(e => e.RolId).HasColumnName("rolId");

            entity.HasOne(d => d.Rol).WithMany(p => p.Users)
                .HasForeignKey(d => d.RolId)
                .HasConstraintName("FK__Users__rolId__114A936A");
        });
        modelBuilder.HasSequence("Seq_Folio");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}