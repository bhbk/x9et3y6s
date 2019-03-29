﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class AppDbContext : DbContext
    {
        public virtual DbSet<AppActivity> AppActivity { get; set; }
        public virtual DbSet<AppClaim> AppClaim { get; set; }
        public virtual DbSet<AppClient> AppClient { get; set; }
        public virtual DbSet<AppClientRefresh> AppClientRefresh { get; set; }
        public virtual DbSet<AppClientUri> AppClientUri { get; set; }
        public virtual DbSet<AppIssuer> AppIssuer { get; set; }
        public virtual DbSet<AppLogin> AppLogin { get; set; }
        public virtual DbSet<AppRole> AppRole { get; set; }
        public virtual DbSet<AppRoleClaim> AppRoleClaim { get; set; }
        public virtual DbSet<AppSysMsg> AppSysMsg { get; set; }
        public virtual DbSet<AppUser> AppUser { get; set; }
        public virtual DbSet<AppUserClaim> AppUserClaim { get; set; }
        public virtual DbSet<AppUserCode> AppUserCode { get; set; }
        public virtual DbSet<AppUserLogin> AppUserLogin { get; set; }
        public virtual DbSet<AppUserRefresh> AppUserRefresh { get; set; }
        public virtual DbSet<AppUserRole> AppUserRole { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            modelBuilder.Entity<AppActivity>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_AppActivity")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ActivityType)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.Actor)
                    .WithMany(p => p.AppActivity)
                    .HasForeignKey(d => d.ActorId)
                    .HasConstraintName("FK_AppActivity_ID");
            });

            modelBuilder.Entity<AppClaim>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_AppClaim")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Type).IsRequired();

                entity.Property(e => e.Value).IsRequired();

                entity.Property(e => e.ValueType).IsRequired();

                entity.HasOne(d => d.Actor)
                    .WithMany(p => p.AppClaim)
                    .HasForeignKey(d => d.ActorId)
                    .HasConstraintName("FK_AppClaim_ActorID");

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.AppClaim)
                    .HasForeignKey(d => d.IssuerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AppClaim_IssuerID");
            });

            modelBuilder.Entity<AppClient>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_AppClient_ID")
                    .IsUnique();

                entity.HasIndex(e => e.IssuerId)
                    .HasName("IX_AppClient_IssuerID");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ClientKey).IsRequired();

                entity.Property(e => e.ClientType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.Name).IsRequired();

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.AppClient)
                    .HasForeignKey(d => d.IssuerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AppClient_IssuerID");
            });

            modelBuilder.Entity<AppClientRefresh>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_AppClientRefresh")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ProtectedTicket).IsRequired();

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.AppClientRefresh)
                    .HasForeignKey(d => d.ClientId)
                    .HasConstraintName("FK_AppClientRefresh_ClientID");

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.AppClientRefresh)
                    .HasForeignKey(d => d.IssuerId)
                    .HasConstraintName("FK_AppClientRefresh_IssuerID");
            });

            modelBuilder.Entity<AppClientUri>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_AppClientUri")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.AbsoluteUri).IsRequired();

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.AppClientUri)
                    .HasForeignKey(d => d.ClientId)
                    .HasConstraintName("FK_AppClientUri_ClientID");
            });

            modelBuilder.Entity<AppIssuer>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_AppIssuer_ID")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.IssuerKey).IsRequired();

                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<AppLogin>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_AppLogin")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name).IsRequired();

                entity.HasOne(d => d.Actor)
                    .WithMany(p => p.AppLogin)
                    .HasForeignKey(d => d.ActorId)
                    .HasConstraintName("FK_AppLogin_ActorID");
            });

            modelBuilder.Entity<AppRole>(entity =>
            {
                entity.HasIndex(e => e.ClientId)
                    .HasName("IX_AppRole_ClientID");

                entity.HasIndex(e => e.Id)
                    .HasName("IX_AppRole_ID")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name).IsRequired();

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.AppRole)
                    .HasForeignKey(d => d.ClientId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AppRole_ClientID");
            });

            modelBuilder.Entity<AppRoleClaim>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.ClaimId });

                entity.HasIndex(e => new { e.RoleId, e.ClaimId })
                    .HasName("IX_AppRoleClaim")
                    .IsUnique();

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.HasOne(d => d.Claim)
                    .WithMany(p => p.AppRoleClaim)
                    .HasForeignKey(d => d.ClaimId)
                    .HasConstraintName("FK_AppRoleClaim_ClaimID");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AppRoleClaim)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_AppRoleClaim_RoleID");
            });

            modelBuilder.Entity<AppSysMsg>(entity =>
            {
                entity.HasKey(e => e.ErrorId)
                    .HasName("PK_SystemErrorInfo");

                entity.Property(e => e.ErrorId)
                    .HasColumnName("ErrorID")
                    .HasColumnType("numeric(18, 0)")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ErrorDate).HasColumnType("datetime");

                entity.Property(e => e.ErrorMessage)
                    .IsRequired()
                    .HasMaxLength(2028)
                    .IsUnicode(false);

                entity.Property(e => e.ErrorProcedure)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ErrorSeverity)
                    .IsRequired()
                    .HasMaxLength(16)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_AppUser_UserName");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Email).IsRequired();

                entity.Property(e => e.FirstName).IsRequired();

                entity.Property(e => e.LastLoginFailure).HasColumnType("datetime");

                entity.Property(e => e.LastLoginSuccess).HasColumnType("datetime");

                entity.Property(e => e.LastName).IsRequired();

                entity.Property(e => e.PhoneNumber).HasMaxLength(16);

                entity.Property(e => e.PhoneNumberConfirmed).HasDefaultValueSql("((0))");
            });

            modelBuilder.Entity<AppUserClaim>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.ClaimId });

                entity.HasIndex(e => new { e.UserId, e.ClaimId })
                    .HasName("IX_AppUserClaim")
                    .IsUnique();

                entity.HasOne(d => d.Claim)
                    .WithMany(p => p.AppUserClaim)
                    .HasForeignKey(d => d.ClaimId)
                    .HasConstraintName("FK_AppUserClaim_ClaimID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AppUserClaim)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_AppUserClaim_UserID");
            });

            modelBuilder.Entity<AppUserCode>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.UserId })
                    .HasName("PK_AppUserToken");

                entity.HasIndex(e => new { e.Id, e.UserId })
                    .HasName("IX_AppUserToken")
                    .IsUnique();

                entity.Property(e => e.Code).IsRequired();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AppUserCode)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_AppUserToken_UserID");
            });

            modelBuilder.Entity<AppUserLogin>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginId });

                entity.HasIndex(e => new { e.UserId, e.LoginId })
                    .HasName("IX_AppUserLogin")
                    .IsUnique();

                entity.HasOne(d => d.Login)
                    .WithMany(p => p.AppUserLogin)
                    .HasForeignKey(d => d.LoginId)
                    .HasConstraintName("FK_AppUserLogin_LoginID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AppUserLogin)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_AppUserLogin_UserID");
            });

            modelBuilder.Entity<AppUserRefresh>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_AppUserRefresh")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ProtectedTicket).IsRequired();

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.AppUserRefresh)
                    .HasForeignKey(d => d.IssuerId)
                    .HasConstraintName("FK_AppUserRefresh_IssuerID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AppUserRefresh)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_AppUserRefresh_UserID");
            });

            modelBuilder.Entity<AppUserRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });

                entity.HasIndex(e => new { e.UserId, e.RoleId })
                    .HasName("IX_AppUserRole")
                    .IsUnique();

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AppUserRole)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_AppUserRole_RoleID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AppUserRole)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_AppUserRole_UserID");
            });
        }
    }
}
