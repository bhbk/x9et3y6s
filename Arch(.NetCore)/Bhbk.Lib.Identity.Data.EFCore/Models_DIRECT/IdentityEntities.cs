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

        public virtual DbSet<tbl_Activity> tbl_Activity { get; set; }
        public virtual DbSet<tbl_Audience> tbl_Audience { get; set; }
        public virtual DbSet<tbl_AudienceRole> tbl_AudienceRole { get; set; }
        public virtual DbSet<tbl_Claim> tbl_Claim { get; set; }
        public virtual DbSet<tbl_Issuer> tbl_Issuer { get; set; }
        public virtual DbSet<tbl_Login> tbl_Login { get; set; }
        public virtual DbSet<tbl_MOTD> tbl_MOTD { get; set; }
        public virtual DbSet<tbl_QueueEmail> tbl_QueueEmail { get; set; }
        public virtual DbSet<tbl_QueueText> tbl_QueueText { get; set; }
        public virtual DbSet<tbl_Refresh> tbl_Refresh { get; set; }
        public virtual DbSet<tbl_Role> tbl_Role { get; set; }
        public virtual DbSet<tbl_RoleClaim> tbl_RoleClaim { get; set; }
        public virtual DbSet<tbl_Setting> tbl_Setting { get; set; }
        public virtual DbSet<tbl_State> tbl_State { get; set; }
        public virtual DbSet<tbl_Url> tbl_Url { get; set; }
        public virtual DbSet<tbl_User> tbl_User { get; set; }
        public virtual DbSet<tbl_UserClaim> tbl_UserClaim { get; set; }
        public virtual DbSet<tbl_UserLogin> tbl_UserLogin { get; set; }
        public virtual DbSet<tbl_UserRole> tbl_UserRole { get; set; }

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
            modelBuilder.Entity<tbl_Activity>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_Activity")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ActivityType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.TableName).HasMaxLength(256);

                entity.HasOne(d => d.Audience)
                    .WithMany(p => p.tbl_Activity)
                    .HasForeignKey(d => d.AudienceId)
                    .HasConstraintName("FK_tbl_Activity_AudienceID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_Activity)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_tbl_Activity_UserID");
            });

            modelBuilder.Entity<tbl_Audience>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_Audience")
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
                    .WithMany(p => p.tbl_Audience)
                    .HasForeignKey(d => d.IssuerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_tbl_Audience_IssuerID");
            });

            modelBuilder.Entity<tbl_AudienceRole>(entity =>
            {
                entity.HasKey(e => new { e.AudienceId, e.RoleId });

                entity.HasIndex(e => new { e.AudienceId, e.RoleId })
                    .HasName("IX_tbl_AudienceRole")
                    .IsUnique();

                entity.HasOne(d => d.Audience)
                    .WithMany(p => p.tbl_AudienceRole)
                    .HasForeignKey(d => d.AudienceId)
                    .HasConstraintName("FK_tbl_AudienceRole_AudienceID");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.tbl_AudienceRole)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_tbl_AudienceRole_RoleID");
            });

            modelBuilder.Entity<tbl_Claim>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_Claim")
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
                    .WithMany(p => p.tbl_Claim)
                    .HasForeignKey(d => d.ActorId)
                    .HasConstraintName("FK_tbl_Claim_ActorID");

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.tbl_Claim)
                    .HasForeignKey(d => d.IssuerId)
                    .HasConstraintName("FK_tbl_Claim_IssuerID");
            });

            modelBuilder.Entity<tbl_Issuer>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_Issuer")
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
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_Login")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Description).HasMaxLength(256);

                entity.Property(e => e.LoginKey).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.Actor)
                    .WithMany(p => p.tbl_Login)
                    .HasForeignKey(d => d.ActorId)
                    .HasConstraintName("FK_tbl_Login_ActorID");
            });

            modelBuilder.Entity<tbl_MOTD>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_MOTD")
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

            modelBuilder.Entity<tbl_QueueEmail>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_QueueEmail")
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
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.HasOne(d => d.From)
                    .WithMany(p => p.tbl_QueueEmail)
                    .HasForeignKey(d => d.FromId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_tbl_QueueEmail_UserID");
            });

            modelBuilder.Entity<tbl_QueueText>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_QueueText")
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
                    .WithMany(p => p.tbl_QueueText)
                    .HasForeignKey(d => d.FromId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_tbl_QueueText_UserID");
            });

            modelBuilder.Entity<tbl_Refresh>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_Refresh")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.RefreshType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.RefreshValue)
                    .IsRequired()
                    .HasMaxLength(2048);

                entity.HasOne(d => d.Audience)
                    .WithMany(p => p.tbl_Refresh)
                    .HasForeignKey(d => d.AudienceId)
                    .HasConstraintName("FK_tbl_Refresh_AudienceID");

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.tbl_Refresh)
                    .HasForeignKey(d => d.IssuerId)
                    .HasConstraintName("FK_tbl_Refresh_IssuerID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_Refresh)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_tbl_Refresh_UserID");
            });

            modelBuilder.Entity<tbl_Role>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_Role")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Description).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.Audience)
                    .WithMany(p => p.tbl_Role)
                    .HasForeignKey(d => d.AudienceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_tbl_Role_AudienceID");
            });

            modelBuilder.Entity<tbl_RoleClaim>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.ClaimId });

                entity.HasIndex(e => new { e.RoleId, e.ClaimId })
                    .HasName("IX_RoleClaims")
                    .IsUnique();

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.HasOne(d => d.Claim)
                    .WithMany(p => p.tbl_RoleClaim)
                    .HasForeignKey(d => d.ClaimId)
                    .HasConstraintName("FK_tbl_RoleClaim_ClaimID");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.tbl_RoleClaim)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_tbl_RoleClaim_RoleID");
            });

            modelBuilder.Entity<tbl_Setting>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_Setting")
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
                    .WithMany(p => p.tbl_Setting)
                    .HasForeignKey(d => d.AudienceId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_tbl_Setting_AudienceID");

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.tbl_Setting)
                    .HasForeignKey(d => d.IssuerId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_tbl_Setting_IssuerID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_Setting)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_tbl_Setting_UserID");
            });

            modelBuilder.Entity<tbl_State>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_State")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.StateType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.StateValue).HasMaxLength(1024);

                entity.HasOne(d => d.Audience)
                    .WithMany(p => p.tbl_State)
                    .HasForeignKey(d => d.AudienceId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_tbl_State_AudienceID");

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.tbl_State)
                    .HasForeignKey(d => d.IssuerId)
                    .HasConstraintName("FK_tbl_State_IssuerID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_State)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_tbl_State_UserID");
            });

            modelBuilder.Entity<tbl_Url>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_Url")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.UrlHost).HasMaxLength(1024);

                entity.Property(e => e.UrlPath).HasMaxLength(1024);

                entity.HasOne(d => d.Audience)
                    .WithMany(p => p.tbl_Url)
                    .HasForeignKey(d => d.AudienceId)
                    .HasConstraintName("FK_tbl_Url_AudienceID");
            });

            modelBuilder.Entity<tbl_User>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_User")
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

                entity.HasIndex(e => new { e.UserId, e.ClaimId })
                    .HasName("IX_tbl_UserClaim")
                    .IsUnique();

                entity.HasOne(d => d.Claim)
                    .WithMany(p => p.tbl_UserClaim)
                    .HasForeignKey(d => d.ClaimId)
                    .HasConstraintName("FK_tbl_UserClaim_ClaimID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_UserClaim)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_tbl_UserClaim_UserID");
            });

            modelBuilder.Entity<tbl_UserLogin>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginId });

                entity.HasIndex(e => new { e.UserId, e.LoginId })
                    .HasName("IX_tbl_UserLogin")
                    .IsUnique();

                entity.HasOne(d => d.Login)
                    .WithMany(p => p.tbl_UserLogin)
                    .HasForeignKey(d => d.LoginId)
                    .HasConstraintName("FK_tbl_UserLogin_LoginID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_UserLogin)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_tbl_UserLogin_UserID");
            });

            modelBuilder.Entity<tbl_UserRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });

                entity.HasIndex(e => new { e.UserId, e.RoleId })
                    .HasName("IX_tbl_UserRole")
                    .IsUnique();

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.tbl_UserRole)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_tbl_UserRole_RoleID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_UserRole)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_tbl_UserRole_UserID");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
