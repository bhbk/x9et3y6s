using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Bhbk.Lib.Identity.Models
{
    public partial class AppDbContext
    {
        public virtual DbSet<AppActivity> AppActivity { get; set; }
        public virtual DbSet<AppAudience> AppAudience { get; set; }
        public virtual DbSet<AppClient> AppClient { get; set; }
        public virtual DbSet<AppLogin> AppLogin { get; set; }
        public virtual DbSet<AppRole> AppRole { get; set; }
        public virtual DbSet<AppRoleClaim> AppRoleClaim { get; set; }
        public virtual DbSet<AppUser> AppUser { get; set; }
        public virtual DbSet<AppUserClaim> AppUserClaim { get; set; }
        public virtual DbSet<AppUserLogin> AppUserLogin { get; set; }
        public virtual DbSet<AppUserRefresh> AppUserRefresh { get; set; }
        public virtual DbSet<AppUserRole> AppUserRole { get; set; }
        public virtual DbSet<AppUserToken> AppUserToken { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppActivity>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_AppActivity")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ActivityType)
                    .IsRequired()
                    .HasMaxLength(32)
                    .IsUnicode(false);

                entity.Property(e => e.CurrentValues).IsUnicode(false);

                entity.Property(e => e.KeyValues).IsUnicode(false);

                entity.Property(e => e.OriginalValues).IsUnicode(false);

                entity.Property(e => e.TableName).IsUnicode(false);
            });

            modelBuilder.Entity<AppAudience>(entity =>
            {
                entity.HasIndex(e => e.ClientId)
                    .HasName("IX_AppAudience_ClientID");

                entity.HasIndex(e => e.Id)
                    .HasName("IX_AppAudience_ID")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.AudienceType)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.Description).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.AppAudience)
                    .HasForeignKey(d => d.ClientId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AppAudience_ClientID");
            });

            modelBuilder.Entity<AppClient>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_AppClient_ID")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ClientKey).IsRequired();

                entity.Property(e => e.Description).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<AppLogin>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_AppLogin")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.LoginProvider)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<AppRole>(entity =>
            {
                entity.HasIndex(e => e.AudienceId)
                    .HasName("IX_AppRole_AudienceID");

                entity.HasIndex(e => e.Id)
                    .HasName("IX_AppRole_ID")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ConcurrencyStamp).HasMaxLength(512);

                entity.Property(e => e.Description).HasMaxLength(10);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.NormalizedName).HasMaxLength(256);

                entity.HasOne(d => d.Audience)
                    .WithMany(p => p.AppRole)
                    .HasForeignKey(d => d.AudienceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AppRole_AudienceID");
            });

            modelBuilder.Entity<AppRoleClaim>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.RoleId });

                entity.HasIndex(e => new { e.Id, e.RoleId })
                    .HasName("IX_AppRoleClaim")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.ClaimType)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.ClaimTypeValue).IsUnicode(false);

                entity.Property(e => e.ClaimValue)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.LastUpdated).HasColumnType("datetime");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AppRoleClaim)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_AppRoleClaim_RoleID");
            });

            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_AppUser_UserName");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ConcurrencyStamp).HasMaxLength(512);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.FirstName).IsRequired();

                entity.Property(e => e.LastLoginFailure).HasColumnType("datetime");

                entity.Property(e => e.LastLoginSuccess).HasColumnType("datetime");

                entity.Property(e => e.LastName).IsRequired();

                entity.Property(e => e.NormalizedEmail).HasMaxLength(256);

                entity.Property(e => e.NormalizedUserName).HasMaxLength(256);

                entity.Property(e => e.PasswordHash).HasMaxLength(512);

                entity.Property(e => e.PhoneNumber).HasMaxLength(256);

                entity.Property(e => e.PhoneNumberConfirmed).HasDefaultValueSql("((0))");

                entity.Property(e => e.SecurityStamp).HasMaxLength(512);

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<AppUserClaim>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.UserId });

                entity.HasIndex(e => new { e.Id, e.UserId })
                    .HasName("IX_AppUserClaim");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.ClaimType)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.ClaimValue)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.ClaimValueType).IsUnicode(false);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AppUserClaim)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_AppUserClaim_UserID");
            });

            modelBuilder.Entity<AppUserLogin>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginId });

                entity.HasIndex(e => new { e.UserId, e.LoginId })
                    .HasName("IX_AppUserLogin")
                    .IsUnique();

                entity.Property(e => e.LoginProvider)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.ProviderDescription).HasMaxLength(256);

                entity.Property(e => e.ProviderDisplayName)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.ProviderKey).HasMaxLength(256);

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
                    .HasName("IX_AppUserRefresh_ID")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ProtectedTicket)
                    .IsRequired()
                    .IsUnicode(false);

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.AppUserRefresh)
                    .HasForeignKey(d => d.ClientId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AppUserRefresh_ClientID");

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

            modelBuilder.Entity<AppUserToken>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.UserId });

                entity.HasIndex(e => new { e.Id, e.UserId })
                    .HasName("IX_AppUserToken")
                    .IsUnique();

                entity.Property(e => e.Code)
                    .IsRequired()
                    .IsUnicode(false);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AppUserToken)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_AppUserToken_UserID");
            });
        }
    }
}
