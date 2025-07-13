﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace splitzy_dotnet.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Expense> Expenses { get; set; }

    public virtual DbSet<ExpenseSplit> ExpenseSplits { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<GroupMember> GroupMembers { get; set; }

    public virtual DbSet<Settlement> Settlements { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=splitzy;Username=postgres;Password=0000");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.ExpenseId).HasName("expenses_pkey");

            entity.ToTable("expenses");

            entity.Property(e => e.ExpenseId).HasColumnName("expense_id");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.PaidByUserId).HasColumnName("paid_by_user_id");
            entity.Property(e => e.SplitPer)
                .HasColumnType("jsonb")
                .HasColumnName("split_per");

            entity.HasOne(d => d.Group).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("expenses_group_id_fkey");

            entity.HasOne(d => d.PaidByUser).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.PaidByUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("expenses_paid_by_user_id_fkey");
        });

        modelBuilder.Entity<ExpenseSplit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("expense_splits_pkey");

            entity.ToTable("expense_splits");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ExpenseId).HasColumnName("expense_id");
            entity.Property(e => e.OwedAmount)
                .HasPrecision(10, 2)
                .HasColumnName("owed_amount");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Expense).WithMany(p => p.ExpenseSplits)
                .HasForeignKey(d => d.ExpenseId)
                .HasConstraintName("expense_splits_expense_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.ExpenseSplits)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("expense_splits_user_id_fkey");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("groups_pkey");

            entity.ToTable("groups");

            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<GroupMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("group_members_pkey");

            entity.ToTable("group_members");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.JoinedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("joined_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Group).WithMany(p => p.GroupMembers)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("group_members_group_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.GroupMembers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("group_members_user_id_fkey");
        });

        modelBuilder.Entity<Settlement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("settlements_pkey");

            entity.ToTable("settlements");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.PaidBy).HasColumnName("paid_by");
            entity.Property(e => e.PaidTo).HasColumnName("paid_to");

            entity.HasOne(d => d.Group).WithMany(p => p.Settlements)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("settlements_group_id_fkey");

            entity.HasOne(d => d.PaidByNavigation).WithMany(p => p.SettlementPaidByNavigations)
                .HasForeignKey(d => d.PaidBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("settlements_paid_by_fkey");

            entity.HasOne(d => d.PaidToNavigation).WithMany(p => p.SettlementPaidToNavigations)
                .HasForeignKey(d => d.PaidTo)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("settlements_paid_to_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
