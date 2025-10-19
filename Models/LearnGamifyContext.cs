using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LearnGamify.Models;

public partial class LearnGamifyContext : DbContext
{
    public LearnGamifyContext(DbContextOptions<LearnGamifyContext> options)
        : base(options)
    {
    }

    public virtual DbSet<GameTask> GameTask { get; set; }

    public virtual DbSet<TeachingLesson> TeachingLesson { get; set; }

    public virtual DbSet<TeachingUnit> TeachingUnit { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GameTask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).HasColumnType("int(11)");
        });

        modelBuilder.Entity<TeachingLesson>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id)
                .HasComment("課件 ID")
                .HasColumnType("int(11)");
        });

        modelBuilder.Entity<TeachingUnit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id)
                .HasComment("單元 ID")
                .HasColumnType("int(11)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
