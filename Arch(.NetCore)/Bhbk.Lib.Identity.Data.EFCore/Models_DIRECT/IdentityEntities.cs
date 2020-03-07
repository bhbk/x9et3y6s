using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT
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
        public virtual DbSet<tbl_AudienceRoles> tbl_AudienceRoles { get; set; }
        public virtual DbSet<tbl_Audiences> tbl_Audiences { get; set; }
        public virtual DbSet<tbl_Claims> tbl_Claims { get; set; }
        public virtual DbSet<tbl_Issuers> tbl_Issuers { get; set; }
        public virtual DbSet<tbl_Logins> tbl_Logins { get; set; }
        public virtual DbSet<tbl_MOTDs> tbl_MOTDs { get; set; }
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

                entity.Property(e => e.TableName).HasMaxLength(256);

                entity.HasOne(d => d.Audience)
                    .WithMany(p => p.tbl_Activities)
                    .HasForeignKey(d => d.AudienceId)
                    .HasConstraintName("FK_Activities_AudienceID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_Activities)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_Activities_UserID");
            });

            modelBuilder.Entity<tbl_AudienceRoles>(entity =>
            {
                entity.HasKey(e => new { e.AudienceId, e.RoleId })
                    .HasName("PK_AudienceRoles");

                entity.HasIndex(e => new { e.AudienceId, e.RoleId })
                    .HasName("IX_AudienceRoles")
                    .IsUnique();

                entity.HasOne(d => d.Audience)
                    .WithMany(p => p.tbl_AudienceRoles)
                    .HasForeignKey(d => d.AudienceId)
                    .HasConstraintName("FK_AudienceRoles_AudienceID");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.tbl_AudienceRoles)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_AudienceRoles_RoleID");
            });

            modelBuilder.Entity<tbl_Audiences>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_Audiences")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.AudienceType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.ConcurrencyStamp).HasMaxLength(256);

                entity.Property(e => e.Description).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.PasswordHash).HasMaxLength(256);

                entity.Property(e => e.SecurityStamp).HasMaxLength(256);

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.tbl_Audiences)
                    .HasForeignKey(d => d.IssuerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Audiences_IssuerID");
            });

            modelBuilder.Entity<tbl_Claims>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_Claims")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

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

                entity.HasOne(d => d.Actor)
                    .WithMany(p => p.tbl_Claims)
                    .HasForeignKey(d => d.ActorId)
                    .HasConstraintName("FK_Claims_ActorID");

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.tbl_Claims)
                    .HasForeignKey(d => d.IssuerId)
                    .HasConstraintName("FK_Claims_IssuerID");
            });

            modelBuilder.Entity<tbl_Issuers>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_Issuers")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Description).HasMaxLength(256);

                entity.Property(e => e.IssuerKey).IsRequired();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<tbl_Logins>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_Logins")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Description).HasMaxLength(256);

                entity.Property(e => e.LoginKey).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.Actor)
                    .WithMany(p => p.tbl_Logins)
                    .HasForeignKey(d => d.ActorId)
                    .HasConstraintName("FK_Logins_ActorID");
            });

            modelBuilder.Entity<tbl_MOTDs>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_MOTDs")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

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

            modelBuilder.Entity<tbl_QueueEmails>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_QueueEmails")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.FromDisplay)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.FromEmail)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.HtmlContent).IsUnicode(false);

                entity.Property(e => e.PlaintextContent).IsUnicode(false);

                entity.Property(e => e.Subject)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.ToDisplay)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.ToEmail)
                    .HasMaxLength(128)
                    .IsUnicode(false);

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

                entity.HasOne(d => d.Audience)
                    .WithMany(p => p.tbl_Refreshes)
                    .HasForeignKey(d => d.AudienceId)
                    .HasConstraintName("FK_Refreshes_AudienceID");

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

                entity.Property(e => e.Description).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.Audience)
                    .WithMany(p => p.tbl_Roles)
                    .HasForeignKey(d => d.AudienceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Roles_AudienceID");
            });

            modelBuilder.Entity<tbl_Settings>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_Settings")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ConfigKey)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.ConfigValue)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.HasOne(d => d.Audience)
                    .WithMany(p => p.tbl_Settings)
                    .HasForeignKey(d => d.AudienceId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Settings_AudienceID");

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

                entity.Property(e => e.StateValue).HasMaxLength(512);

                entity.HasOne(d => d.Audience)
                    .WithMany(p => p.tbl_States)
                    .HasForeignKey(d => d.AudienceId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_States_AudienceID");

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

                entity.Property(e => e.UrlHost).HasMaxLength(512);

                entity.Property(e => e.UrlPath).HasMaxLength(512);

                entity.HasOne(d => d.Audience)
                    .WithMany(p => p.tbl_Urls)
                    .HasForeignKey(d => d.AudienceId)
                    .HasConstraintName("FK_Urls_AudienceID");
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

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
