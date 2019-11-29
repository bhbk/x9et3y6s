﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Bhbk.Lib.Identity.Data.Models
{
    public partial class IdentityEntities : DbContext
    {
        public IdentityEntities()
        {
        }

        public IdentityEntities(DbContextOptions<IdentityEntities> options)
            : base(options)
        {
        }

        public virtual DbSet<tbl_Activities> tbl_Activities { get; set; }
        public virtual DbSet<tbl_Claims> tbl_Claims { get; set; }
        public virtual DbSet<tbl_ClientRoles> tbl_ClientRoles { get; set; }
        public virtual DbSet<tbl_Clients> tbl_Clients { get; set; }
        public virtual DbSet<tbl_Issuers> tbl_Issuers { get; set; }
        public virtual DbSet<tbl_Logins> tbl_Logins { get; set; }
        public virtual DbSet<tbl_MotDType1> tbl_MotDType1 { get; set; }
        public virtual DbSet<tbl_QueueEmails> tbl_QueueEmails { get; set; }
        public virtual DbSet<tbl_QueueTexts> tbl_QueueTexts { get; set; }
        public virtual DbSet<tbl_Refreshes> tbl_Refreshes { get; set; }
        public virtual DbSet<tbl_RoleClaims> tbl_RoleClaims { get; set; }
        public virtual DbSet<tbl_Roles> tbl_Roles { get; set; }
        public virtual DbSet<tbl_Settings> tbl_Settings { get; set; }
        public virtual DbSet<tbl_States> tbl_States { get; set; }
        public virtual DbSet<tbl_Urls> tbl_Urls { get; set; }
        public virtual DbSet<tbl_UserClaims> tbl_UserClaims { get; set; }
        public virtual DbSet<tbl_UserLogins> tbl_UserLogins { get; set; }
        public virtual DbSet<tbl_UserRoles> tbl_UserRoles { get; set; }
        public virtual DbSet<tbl_Users> tbl_Users { get; set; }
        public virtual DbSet<uvw_Activities> uvw_Activities { get; set; }
        public virtual DbSet<uvw_Claims> uvw_Claims { get; set; }
        public virtual DbSet<uvw_Users> uvw_Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Data Source=bits.test.ochap.local; Initial Catalog=BhbkIdentity; User ID=Sql.BhbkIdentity; Password=Pa$$word01!");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<tbl_Activities>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_Activity")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ActivityType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.tbl_Activities)
                    .HasForeignKey(d => d.ClientId)
                    .HasConstraintName("FK_Activities_ClientID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_Activities)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_Activities_UserID");
            });

            modelBuilder.Entity<tbl_Claims>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_Claims")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Type).IsRequired();

                entity.Property(e => e.Value).IsRequired();

                entity.Property(e => e.ValueType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.HasOne(d => d.Actor)
                    .WithMany(p => p.tbl_Claims)
                    .HasForeignKey(d => d.ActorId)
                    .HasConstraintName("FK_Claims_ActorID");

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.tbl_Claims)
                    .HasForeignKey(d => d.IssuerId)
                    .HasConstraintName("FK_Claims_IssuerID");
            });

            modelBuilder.Entity<tbl_ClientRoles>(entity =>
            {
                entity.HasKey(e => new { e.ClientId, e.RoleId })
                    .HasName("PK_ClientRoles");

                entity.HasIndex(e => new { e.ClientId, e.RoleId })
                    .HasName("IX_ClientRoles")
                    .IsUnique();

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.tbl_ClientRoles)
                    .HasForeignKey(d => d.ClientId)
                    .HasConstraintName("FK_ClientRoles_ClientID");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.tbl_ClientRoles)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ClientRoles_RoleID");
            });

            modelBuilder.Entity<tbl_Clients>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_Clients")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ClientType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.Name).IsRequired();

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.tbl_Clients)
                    .HasForeignKey(d => d.IssuerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Clients_IssuerID");
            });

            modelBuilder.Entity<tbl_Issuers>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_Issuers")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.IssuerKey).IsRequired();

                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<tbl_Logins>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_Logins")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name).IsRequired();

                entity.HasOne(d => d.Actor)
                    .WithMany(p => p.tbl_Logins)
                    .HasForeignKey(d => d.ActorId)
                    .HasConstraintName("FK_Logins_ActorID");
            });

            modelBuilder.Entity<tbl_MotDType1>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_MotDType1")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.Author)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Background).IsUnicode(false);

                entity.Property(e => e.Category).IsUnicode(false);

                entity.Property(e => e.Quote)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Tags).IsUnicode(false);

                entity.Property(e => e.Title).IsUnicode(false);
            });

            modelBuilder.Entity<tbl_QueueEmails>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_QueueEmails")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.FromDisplay).IsUnicode(false);

                entity.Property(e => e.FromEmail).IsUnicode(false);

                entity.Property(e => e.HtmlContent).IsUnicode(false);

                entity.Property(e => e.PlaintextContent).IsUnicode(false);

                entity.Property(e => e.Subject)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.ToDisplay).IsUnicode(false);

                entity.Property(e => e.ToEmail).IsUnicode(false);

                entity.HasOne(d => d.From)
                    .WithMany(p => p.tbl_QueueEmails)
                    .HasForeignKey(d => d.FromId)
                    .HasConstraintName("FK_QueueEmails_UserID");
            });

            modelBuilder.Entity<tbl_QueueTexts>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_QueueTexts")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Body)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.FromPhoneNumber)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.ToPhoneNumber)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.HasOne(d => d.From)
                    .WithMany(p => p.tbl_QueueTexts)
                    .HasForeignKey(d => d.FromId)
                    .HasConstraintName("FK_QueueTexts_UserID");
            });

            modelBuilder.Entity<tbl_Refreshes>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_Refreshes")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.RefreshType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.RefreshValue).IsRequired();

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.tbl_Refreshes)
                    .HasForeignKey(d => d.ClientId)
                    .HasConstraintName("FK_Refreshes_ClientID");

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.tbl_Refreshes)
                    .HasForeignKey(d => d.IssuerId)
                    .HasConstraintName("FK_Refreshes_IssuerID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_Refreshes)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_Refreshes_UserID");
            });

            modelBuilder.Entity<tbl_RoleClaims>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.ClaimId })
                    .HasName("PK_RoleClaims");

                entity.HasIndex(e => new { e.RoleId, e.ClaimId })
                    .HasName("IX_RoleClaims")
                    .IsUnique();

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.HasOne(d => d.Claim)
                    .WithMany(p => p.tbl_RoleClaims)
                    .HasForeignKey(d => d.ClaimId)
                    .HasConstraintName("FK_RoleClaims_ClaimID");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.tbl_RoleClaims)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_RoleClaims_RoleID");
            });

            modelBuilder.Entity<tbl_Roles>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_Roles")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name).IsRequired();

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.tbl_Roles)
                    .HasForeignKey(d => d.ClientId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Roles_ClientID");
            });

            modelBuilder.Entity<tbl_Settings>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_Settings")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ConfigKey)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.ConfigValue)
                    .IsRequired()
                    .IsUnicode(false);

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.tbl_Settings)
                    .HasForeignKey(d => d.ClientId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Settings_ClientID");

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.tbl_Settings)
                    .HasForeignKey(d => d.IssuerId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Settings_IssuerID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_Settings)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Settings_UserID");
            });

            modelBuilder.Entity<tbl_States>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_States")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.StateType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.tbl_States)
                    .HasForeignKey(d => d.ClientId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_States_ClientID");

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.tbl_States)
                    .HasForeignKey(d => d.IssuerId)
                    .HasConstraintName("FK_States_IssuerID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_States)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_States_UserID");
            });

            modelBuilder.Entity<tbl_Urls>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_Urls")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.tbl_Urls)
                    .HasForeignKey(d => d.ClientId)
                    .HasConstraintName("FK_Urls_ClientID");
            });

            modelBuilder.Entity<tbl_UserClaims>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.ClaimId })
                    .HasName("PK_UserClaims");

                entity.HasIndex(e => new { e.UserId, e.ClaimId })
                    .HasName("IX_UserClaims")
                    .IsUnique();

                entity.HasOne(d => d.Claim)
                    .WithMany(p => p.tbl_UserClaims)
                    .HasForeignKey(d => d.ClaimId)
                    .HasConstraintName("FK_UserClaims_ClaimID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_UserClaims)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_UserClaims_UserID");
            });

            modelBuilder.Entity<tbl_UserLogins>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginId })
                    .HasName("PK_UserLogins");

                entity.HasIndex(e => new { e.UserId, e.LoginId })
                    .HasName("IX_UserLogins")
                    .IsUnique();

                entity.HasOne(d => d.Login)
                    .WithMany(p => p.tbl_UserLogins)
                    .HasForeignKey(d => d.LoginId)
                    .HasConstraintName("FK_UserLogins_LoginID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_UserLogins)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_UserLogins_UserID");
            });

            modelBuilder.Entity<tbl_UserRoles>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId })
                    .HasName("PK_UserRoles");

                entity.HasIndex(e => new { e.UserId, e.RoleId })
                    .HasName("IX_UserRoles")
                    .IsUnique();

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.tbl_UserRoles)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_UserRoles_RoleID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_UserRoles)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_UserRoles_UserID");
            });

            modelBuilder.Entity<tbl_Users>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_Users")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Email).IsRequired();

                entity.Property(e => e.FirstName).IsRequired();

                entity.Property(e => e.LastName).IsRequired();

                entity.Property(e => e.PhoneNumber).HasMaxLength(16);

                entity.Property(e => e.PhoneNumberConfirmed).HasDefaultValueSql("((0))");
            });

            modelBuilder.Entity<uvw_Activities>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Activities", "svc");

                entity.Property(e => e.ActivityType)
                    .IsRequired()
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<uvw_Claims>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Claims", "svc");

                entity.Property(e => e.Type).IsRequired();

                entity.Property(e => e.Value).IsRequired();

                entity.Property(e => e.ValueType)
                    .IsRequired()
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<uvw_Users>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Users", "svc");

                entity.Property(e => e.Email).IsRequired();

                entity.Property(e => e.FirstName).IsRequired();

                entity.Property(e => e.LastName).IsRequired();

                entity.Property(e => e.PhoneNumber).HasMaxLength(16);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}