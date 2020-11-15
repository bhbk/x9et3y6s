using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EFCore.Models
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

        public virtual DbSet<uvw_Activity> uvw_Activities { get; set; }
        public virtual DbSet<uvw_Audience> uvw_Audiences { get; set; }
        public virtual DbSet<uvw_AudienceRole> uvw_AudienceRoles { get; set; }
        public virtual DbSet<uvw_Claim> uvw_Claims { get; set; }
        public virtual DbSet<uvw_EmailQueue> uvw_EmailQueues { get; set; }
        public virtual DbSet<uvw_Issuer> uvw_Issuers { get; set; }
        public virtual DbSet<uvw_Login> uvw_Logins { get; set; }
        public virtual DbSet<uvw_MOTD> uvw_MOTDs { get; set; }
        public virtual DbSet<uvw_Refresh> uvw_Refreshes { get; set; }
        public virtual DbSet<uvw_Role> uvw_Roles { get; set; }
        public virtual DbSet<uvw_Setting> uvw_Settings { get; set; }
        public virtual DbSet<uvw_State> uvw_States { get; set; }
        public virtual DbSet<uvw_TextQueue> uvw_TextQueues { get; set; }
        public virtual DbSet<uvw_Url> uvw_Urls { get; set; }
        public virtual DbSet<uvw_User> uvw_Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<uvw_Activity>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Activity", "svc");

                entity.Property(e => e.ActivityType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.TableName).HasMaxLength(256);
            });

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

            modelBuilder.Entity<uvw_EmailQueue>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_EmailQueue", "svc");

                entity.Property(e => e.Body).IsUnicode(false);

                entity.Property(e => e.FromDisplay)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.FromEmail)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.Subject)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.ToDisplay)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.ToEmail)
                    .IsRequired()
                    .HasMaxLength(128)
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
                    .HasMaxLength(256)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<uvw_State>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_State", "svc");

                entity.Property(e => e.StateType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.StateValue).HasMaxLength(1024);
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

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
