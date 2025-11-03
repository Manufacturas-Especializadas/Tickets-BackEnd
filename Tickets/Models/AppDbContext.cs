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

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TkCategory> TkCategories { get; set; }

    public virtual DbSet<TkStatus> TkStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tickets__3214EC07A552F298");

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

            entity.HasOne(d => d.Category).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Tickets__categor__68487DD7");

            entity.HasOne(d => d.Status).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Tickets__statusI__693CA210");
        });

        modelBuilder.Entity<TkCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TkCatego__3214EC075AAFF190");

            entity.ToTable("TkCategory");

            entity.Property(e => e.Name)
                .HasMaxLength(70)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<TkStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TkStatus__3214EC07A63A019A");

            entity.ToTable("TkStatus");

            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name");
        });
        modelBuilder.HasSequence("Seq_Folio");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}