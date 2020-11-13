using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

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

        public virtual DbSet<tbl_Activity> tbl_Activities { get; set; }
        public virtual DbSet<tbl_Audience> tbl_Audiences { get; set; }
        public virtual DbSet<tbl_AudienceRole> tbl_AudienceRoles { get; set; }
        public virtual DbSet<tbl_Claim> tbl_Claims { get; set; }
        public virtual DbSet<tbl_EmailActivity> tbl_EmailActivities { get; set; }
        public virtual DbSet<tbl_EmailQueue> tbl_EmailQueues { get; set; }
        public virtual DbSet<tbl_Issuer> tbl_Issuers { get; set; }
        public virtual DbSet<tbl_Login> tbl_Logins { get; set; }
        public virtual DbSet<tbl_MOTD> tbl_MOTDs { get; set; }
        public virtual DbSet<tbl_Refresh> tbl_Refreshes { get; set; }
        public virtual DbSet<tbl_Role> tbl_Roles { get; set; }
        public virtual DbSet<tbl_RoleClaim> tbl_RoleClaims { get; set; }
        public virtual DbSet<tbl_Setting> tbl_Settings { get; set; }
        public virtual DbSet<tbl_State> tbl_States { get; set; }
        public virtual DbSet<tbl_TextActivity> tbl_TextActivities { get; set; }
        public virtual DbSet<tbl_TextQueue> tbl_TextQueues { get; set; }
        public virtual DbSet<tbl_Url> tbl_Urls { get; set; }
        public virtual DbSet<tbl_User> tbl_Users { get; set; }
        public virtual DbSet<tbl_UserClaim> tbl_UserClaims { get; set; }
        public virtual DbSet<tbl_UserLogin> tbl_UserLogins { get; set; }
        public virtual DbSet<tbl_UserRole> tbl_UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<tbl_Activity>(entity =>
            {
                entity.ToTable("tbl_Activity");

                entity.HasIndex(e => e.Id, "IX_tbl_Activity")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ActivityType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.TableName).HasMaxLength(256);

                entity.HasOne(d => d.Audience)
                    .WithMany(p => p.tbl_Activities)
                    .HasForeignKey(d => d.AudienceId)
                    .HasConstraintName("FK_tbl_Activity_AudienceID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_Activities)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_tbl_Activity_UserID");
            });

            modelBuilder.Entity<tbl_Audience>(entity =>
            {
                entity.ToTable("tbl_Audience");

                entity.HasIndex(e => e.Id, "IX_tbl_Audience")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ConcurrencyStamp).HasMaxLength(256);

                entity.Property(e => e.Description).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.PasswordHashPBKDF2).HasMaxLength(256);

                entity.Property(e => e.PasswordHashSHA256).HasMaxLength(256);

                entity.Property(e => e.SecurityStamp).HasMaxLength(256);

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.tbl_Audiences)
                    .HasForeignKey(d => d.IssuerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_tbl_Audience_IssuerID");
            });

            modelBuilder.Entity<tbl_AudienceRole>(entity =>
            {
                entity.HasKey(e => new { e.AudienceId, e.RoleId });

                entity.ToTable("tbl_AudienceRole");

                entity.HasIndex(e => new { e.AudienceId, e.RoleId }, "IX_tbl_AudienceRole")
                    .IsUnique();

                entity.HasOne(d => d.Audience)
                    .WithMany(p => p.tbl_AudienceRoles)
                    .HasForeignKey(d => d.AudienceId)
                    .HasConstraintName("FK_tbl_AudienceRole_AudienceID");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.tbl_AudienceRoles)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_tbl_AudienceRole_RoleID");
            });

            modelBuilder.Entity<tbl_Claim>(entity =>
            {
                entity.ToTable("tbl_Claim");

                entity.HasIndex(e => e.Id, "IX_tbl_Claim")
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
                    .HasConstraintName("FK_tbl_Claim_ActorID");

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.tbl_Claims)
                    .HasForeignKey(d => d.IssuerId)
                    .HasConstraintName("FK_tbl_Claim_IssuerID");
            });

            modelBuilder.Entity<tbl_EmailActivity>(entity =>
            {
                entity.ToTable("tbl_EmailActivity");

                entity.HasIndex(e => e.Id, "IX_tbl_EmailActivity")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.SendgridId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SendgridStatus)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.HasOne(d => d.Email)
                    .WithMany(p => p.tbl_EmailActivities)
                    .HasForeignKey(d => d.EmailId)
                    .HasConstraintName("FK_tbl_EmailActivity_EmailID");
            });

            modelBuilder.Entity<tbl_EmailQueue>(entity =>
            {
                entity.ToTable("tbl_EmailQueue");

                entity.HasIndex(e => e.Id, "IX_tbl_EmailQueue")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Body).IsUnicode(false);

                entity.Property(e => e.FromDisplay)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.FromEmail)
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

                entity.HasOne(d => d.From)
                    .WithMany(p => p.tbl_EmailQueues)
                    .HasForeignKey(d => d.FromId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_tbl_EmailQueue_UserID");
            });

            modelBuilder.Entity<tbl_Issuer>(entity =>
            {
                entity.ToTable("tbl_Issuer");

                entity.HasIndex(e => e.Id, "IX_tbl_Issuer")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Description).HasMaxLength(256);

                entity.Property(e => e.IssuerKey)
                    .IsRequired()
                    .HasMaxLength(1024);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<tbl_Login>(entity =>
            {
                entity.ToTable("tbl_Login");

                entity.HasIndex(e => e.Id, "IX_tbl_Login")
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
                    .HasConstraintName("FK_tbl_Login_ActorID");
            });

            modelBuilder.Entity<tbl_MOTD>(entity =>
            {
                entity.ToTable("tbl_MOTD");

                entity.HasIndex(e => e.Id, "IX_tbl_MOTD")
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

            modelBuilder.Entity<tbl_Refresh>(entity =>
            {
                entity.ToTable("tbl_Refresh");

                entity.HasIndex(e => e.Id, "IX_tbl_Refresh")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.RefreshType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.RefreshValue)
                    .IsRequired()
                    .HasMaxLength(2048);

                entity.HasOne(d => d.Audience)
                    .WithMany(p => p.tbl_Refreshes)
                    .HasForeignKey(d => d.AudienceId)
                    .HasConstraintName("FK_tbl_Refresh_AudienceID");

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.tbl_Refreshes)
                    .HasForeignKey(d => d.IssuerId)
                    .HasConstraintName("FK_tbl_Refresh_IssuerID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_Refreshes)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_tbl_Refresh_UserID");
            });

            modelBuilder.Entity<tbl_Role>(entity =>
            {
                entity.ToTable("tbl_Role");

                entity.HasIndex(e => e.Id, "IX_tbl_Role")
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
                    .HasConstraintName("FK_tbl_Role_AudienceID");
            });

            modelBuilder.Entity<tbl_RoleClaim>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.ClaimId });

                entity.ToTable("tbl_RoleClaim");

                entity.HasIndex(e => new { e.RoleId, e.ClaimId }, "IX_RoleClaims")
                    .IsUnique();

                entity.HasOne(d => d.Claim)
                    .WithMany(p => p.tbl_RoleClaims)
                    .HasForeignKey(d => d.ClaimId)
                    .HasConstraintName("FK_tbl_RoleClaim_ClaimID");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.tbl_RoleClaims)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_tbl_RoleClaim_RoleID");
            });

            modelBuilder.Entity<tbl_Setting>(entity =>
            {
                entity.ToTable("tbl_Setting");

                entity.HasIndex(e => e.Id, "IX_tbl_Setting")
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
                    .HasConstraintName("FK_tbl_Setting_AudienceID");

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.tbl_Settings)
                    .HasForeignKey(d => d.IssuerId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_tbl_Setting_IssuerID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_Settings)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_tbl_Setting_UserID");
            });

            modelBuilder.Entity<tbl_State>(entity =>
            {
                entity.ToTable("tbl_State");

                entity.HasIndex(e => e.Id, "IX_tbl_State")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.StateType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.StateValue).HasMaxLength(1024);

                entity.HasOne(d => d.Audience)
                    .WithMany(p => p.tbl_States)
                    .HasForeignKey(d => d.AudienceId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_tbl_State_AudienceID");

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.tbl_States)
                    .HasForeignKey(d => d.IssuerId)
                    .HasConstraintName("FK_tbl_State_IssuerID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_States)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_tbl_State_UserID");
            });

            modelBuilder.Entity<tbl_TextActivity>(entity =>
            {
                entity.ToTable("tbl_TextActivity");

                entity.HasIndex(e => e.Id, "IX_tbl_TextActivity")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.TwilioMessage)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.TwilioSid)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TwilioStatus)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.HasOne(d => d.Text)
                    .WithMany(p => p.tbl_TextActivities)
                    .HasForeignKey(d => d.TextId)
                    .HasConstraintName("FK_tbl_TextActivity_TextID");
            });

            modelBuilder.Entity<tbl_TextQueue>(entity =>
            {
                entity.ToTable("tbl_TextQueue");

                entity.HasIndex(e => e.Id, "IX_tbl_TextQueue")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Body)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.FromPhoneNumber)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.ToPhoneNumber)
                    .IsRequired()
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.HasOne(d => d.From)
                    .WithMany(p => p.tbl_TextQueues)
                    .HasForeignKey(d => d.FromId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_tbl_TextQueue_UserID");
            });

            modelBuilder.Entity<tbl_Url>(entity =>
            {
                entity.ToTable("tbl_Url");

                entity.HasIndex(e => e.Id, "IX_tbl_Url")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.UrlHost).HasMaxLength(1024);

                entity.Property(e => e.UrlPath).HasMaxLength(1024);

                entity.HasOne(d => d.Audience)
                    .WithMany(p => p.tbl_Urls)
                    .HasForeignKey(d => d.AudienceId)
                    .HasConstraintName("FK_tbl_Url_AudienceID");
            });

            modelBuilder.Entity<tbl_User>(entity =>
            {
                entity.ToTable("tbl_User");

                entity.HasIndex(e => e.Id, "IX_tbl_User")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

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

                entity.Property(e => e.PhoneNumberConfirmed).HasDefaultValueSql("((0))");

                entity.Property(e => e.SecurityStamp)
                    .IsRequired()
                    .HasMaxLength(1024);

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<tbl_UserClaim>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.ClaimId });

                entity.ToTable("tbl_UserClaim");

                entity.HasIndex(e => new { e.UserId, e.ClaimId }, "IX_tbl_UserClaim")
                    .IsUnique();

                entity.HasOne(d => d.Claim)
                    .WithMany(p => p.tbl_UserClaims)
                    .HasForeignKey(d => d.ClaimId)
                    .HasConstraintName("FK_tbl_UserClaim_ClaimID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_UserClaims)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_tbl_UserClaim_UserID");
            });

            modelBuilder.Entity<tbl_UserLogin>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginId });

                entity.ToTable("tbl_UserLogin");

                entity.HasIndex(e => new { e.UserId, e.LoginId }, "IX_tbl_UserLogin")
                    .IsUnique();

                entity.HasOne(d => d.Login)
                    .WithMany(p => p.tbl_UserLogins)
                    .HasForeignKey(d => d.LoginId)
                    .HasConstraintName("FK_tbl_UserLogin_LoginID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_UserLogins)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_tbl_UserLogin_UserID");
            });

            modelBuilder.Entity<tbl_UserRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });

                entity.ToTable("tbl_UserRole");

                entity.HasIndex(e => new { e.UserId, e.RoleId }, "IX_tbl_UserRole")
                    .IsUnique();

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.tbl_UserRoles)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_tbl_UserRole_RoleID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_UserRoles)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_tbl_UserRole_UserID");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
