﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

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

        public virtual DbSet<uvw_Audience> uvw_Audiences { get; set; }
        public virtual DbSet<uvw_AudienceRole> uvw_AudienceRoles { get; set; }
        public virtual DbSet<uvw_AuthActivity> uvw_AuthActivities { get; set; }
        public virtual DbSet<uvw_Claim> uvw_Claims { get; set; }
        public virtual DbSet<uvw_EmailActivity> uvw_EmailActivities { get; set; }
        public virtual DbSet<uvw_EmailQueue> uvw_EmailQueues { get; set; }
        public virtual DbSet<uvw_Issuer> uvw_Issuers { get; set; }
        public virtual DbSet<uvw_Login> uvw_Logins { get; set; }
        public virtual DbSet<uvw_MOTD> uvw_MOTDs { get; set; }
        public virtual DbSet<uvw_Refresh> uvw_Refreshes { get; set; }
        public virtual DbSet<uvw_Role> uvw_Roles { get; set; }
        public virtual DbSet<uvw_RoleClaim> uvw_RoleClaims { get; set; }
        public virtual DbSet<uvw_Setting> uvw_Settings { get; set; }
        public virtual DbSet<uvw_State> uvw_States { get; set; }
        public virtual DbSet<uvw_TextActivity> uvw_TextActivities { get; set; }
        public virtual DbSet<uvw_TextQueue> uvw_TextQueues { get; set; }
        public virtual DbSet<uvw_Url> uvw_Urls { get; set; }
        public virtual DbSet<uvw_User> uvw_Users { get; set; }
        public virtual DbSet<uvw_UserClaim> uvw_UserClaims { get; set; }
        public virtual DbSet<uvw_UserLogin> uvw_UserLogins { get; set; }
        public virtual DbSet<uvw_UserRole> uvw_UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<uvw_Audience>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Audience", "svc");

                entity.Property(e => e.ConcurrencyStamp).HasMaxLength(256);

                entity.Property(e => e.Description).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.PasswordHashPBKDF2).HasMaxLength(256);

                entity.Property(e => e.PasswordHashSHA256).HasMaxLength(256);

                entity.Property(e => e.SecurityStamp).HasMaxLength(256);
            });

            modelBuilder.Entity<uvw_AudienceRole>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_AudienceRole", "svc");
            });

            modelBuilder.Entity<uvw_AuthActivity>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_AuthActivity", "svc");

                entity.Property(e => e.LocalEndpoint).HasMaxLength(128);

                entity.Property(e => e.LoginOutcome).HasMaxLength(16);

                entity.Property(e => e.LoginType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.RemoteEndpoint).HasMaxLength(128);
            });

            modelBuilder.Entity<uvw_Claim>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Claim", "svc");

                entity.Property(e => e.Subject)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.ValueType)
                    .IsRequired()
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<uvw_EmailActivity>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_EmailActivity", "svc");

                entity.Property(e => e.SendgridId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SendgridStatus)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<uvw_EmailQueue>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_EmailQueue", "svc");

                entity.Property(e => e.Body).IsUnicode(false);

                entity.Property(e => e.FromDisplay)
                    .HasMaxLength(512)
                    .IsUnicode(false);

                entity.Property(e => e.FromEmail)
                    .IsRequired()
                    .HasMaxLength(320)
                    .IsUnicode(false);

                entity.Property(e => e.Subject)
                    .IsRequired()
                    .HasMaxLength(1024)
                    .IsUnicode(false);

                entity.Property(e => e.ToDisplay)
                    .HasMaxLength(512)
                    .IsUnicode(false);

                entity.Property(e => e.ToEmail)
                    .IsRequired()
                    .HasMaxLength(320)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<uvw_Issuer>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Issuer", "svc");

                entity.Property(e => e.Description).HasMaxLength(256);

                entity.Property(e => e.IssuerKey)
                    .IsRequired()
                    .HasMaxLength(1024);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<uvw_Login>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Login", "svc");

                entity.Property(e => e.Description).HasMaxLength(256);

                entity.Property(e => e.LoginKey).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<uvw_MOTD>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_MOTD", "svc");

                entity.Property(e => e.Author)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.Quote)
                    .IsRequired()
                    .HasMaxLength(4096)
                    .IsUnicode(false);

                entity.Property(e => e.TssBackground)
                    .HasMaxLength(512)
                    .IsUnicode(false);

                entity.Property(e => e.TssCategory)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.TssId)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.TssTags)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.TssTitle)
                    .HasMaxLength(256)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<uvw_Refresh>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Refresh", "svc");

                entity.Property(e => e.RefreshType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.RefreshValue)
                    .IsRequired()
                    .HasMaxLength(2048);
            });

            modelBuilder.Entity<uvw_Role>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Role", "svc");

                entity.Property(e => e.Description).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<uvw_RoleClaim>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_RoleClaim", "svc");
            });

            modelBuilder.Entity<uvw_Setting>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Setting", "svc");

                entity.Property(e => e.ConfigKey)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.ConfigValue)
                    .IsRequired()
                    .HasMaxLength(1024)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<uvw_State>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_State", "svc");

                entity.Property(e => e.StateType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.StateValue).HasMaxLength(2048);
            });

            modelBuilder.Entity<uvw_TextActivity>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_TextActivity", "svc");

                entity.Property(e => e.TwilioMessage)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.TwilioSid)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TwilioStatus)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<uvw_TextQueue>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_TextQueue", "svc");

                entity.Property(e => e.Body)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.FromPhoneNumber)
                    .IsRequired()
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.ToPhoneNumber)
                    .IsRequired()
                    .HasMaxLength(15)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<uvw_Url>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Url", "svc");

                entity.Property(e => e.UrlHost).HasMaxLength(1024);

                entity.Property(e => e.UrlPath).HasMaxLength(1024);
            });

            modelBuilder.Entity<uvw_User>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_User", "svc");

                entity.Property(e => e.ConcurrencyStamp)
                    .IsRequired()
                    .HasMaxLength(1024);

                entity.Property(e => e.EmailAddress).HasMaxLength(128);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.PasswordHashPBKDF2).HasMaxLength(2048);

                entity.Property(e => e.PasswordHashSHA256).HasMaxLength(2048);

                entity.Property(e => e.PhoneNumber).HasMaxLength(16);

                entity.Property(e => e.SecurityStamp)
                    .IsRequired()
                    .HasMaxLength(1024);

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<uvw_UserClaim>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_UserClaim", "svc");
            });

            modelBuilder.Entity<uvw_UserLogin>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_UserLogin", "svc");
            });

            modelBuilder.Entity<uvw_UserRole>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_UserRole", "svc");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
