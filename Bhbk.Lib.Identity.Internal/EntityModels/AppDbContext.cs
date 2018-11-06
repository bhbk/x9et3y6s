using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Bhbk.Lib.Identity.EntityModels
{
    public partial class AppDbContext
    {
        public virtual DbSet<AppActivity> AppActivity { get; set; }
        public virtual DbSet<AppClient> AppClient { get; set; }
        public virtual DbSet<AppClientUri> AppClientUri { get; set; }
        public virtual DbSet<AppIssuer> AppIssuer { get; set; }
        public virtual DbSet<AppLogin> AppLogin { get; set; }
        public virtual DbSet<AppRole> AppRole { get; set; }
        public virtual DbSet<AppRoleClaim> AppRoleClaim { get; set; }
        public virtual DbSet<AppUser> AppUser { get; set; }
        public virtual DbSet<AppUserClaim> AppUserClaim { get; set; }
        public virtual DbSet<AppUserLogin> AppUserLogin { get; set; }
        public virtual DbSet<AppUserRefresh> AppUserRefresh { get; set; }
        public virtual DbSet<AppUserRole> AppUserRole { get; set; }
        public virtual DbSet<AppUserToken> AppUserToken { get; set; }
        public virtual DbSet<SystemError> SystemError { get; set; }

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

                entity.Property(e => e.Description).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.AppClient)
                    .HasForeignKey(d => d.IssuerId)
                    .HasConstraintName("FK_AppClient_IssuerID");
            });

            modelBuilder.Entity<AppClientUri>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_AppClientUri")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.AbsoluteUri).IsRequired();

                entity.HasOne(d => d.Actor)
                    .WithMany(p => p.AppClientUri)
                    .HasForeignKey(d => d.ActorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AppClientUri_ActorID");

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.AppClientUri)
                    .HasForeignKey(d => d.ClientId)
                    .HasConstraintName("FK_AppClientUri_ID");
            });

            modelBuilder.Entity<AppIssuer>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_AppIssuer_ID")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Description).HasMaxLength(256);

                entity.Property(e => e.IssuerKey).IsRequired();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<AppLogin>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_AppLogin")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.LoginProvider)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<AppRole>(entity =>
            {
                entity.HasIndex(e => e.ClientId)
                    .HasName("IX_AppRole_ClientID");

                entity.HasIndex(e => e.Id)
                    .HasName("IX_AppRole_ID")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Description).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.NormalizedName).HasMaxLength(128);

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.AppRole)
                    .HasForeignKey(d => d.ClientId)
                    .HasConstraintName("FK_AppRole_ClientID");
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
                    .HasMaxLength(128);

                entity.Property(e => e.ClaimTypeValue).HasMaxLength(50);

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

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.LastLoginFailure).HasColumnType("datetime");

                entity.Property(e => e.LastLoginSuccess).HasColumnType("datetime");

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.NormalizedEmail).HasMaxLength(128);

                entity.Property(e => e.NormalizedUserName).HasMaxLength(128);

                entity.Property(e => e.PhoneNumber).HasMaxLength(16);

                entity.Property(e => e.PhoneNumberConfirmed).HasDefaultValueSql("((0))");

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<AppUserClaim>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.UserId });

                entity.HasIndex(e => new { e.Id, e.UserId })
                    .HasName("IX_AppUserClaim");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.ClaimType).IsRequired();

                entity.Property(e => e.ClaimValue).IsRequired();

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
                    .HasMaxLength(128);

                entity.Property(e => e.ProviderDescription).HasMaxLength(256);

                entity.Property(e => e.ProviderDisplayName)
                    .IsRequired()
                    .HasMaxLength(128);

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

            modelBuilder.Entity<AppUserToken>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.UserId });

                entity.HasIndex(e => new { e.Id, e.UserId })
                    .HasName("IX_AppUserToken")
                    .IsUnique();

                entity.Property(e => e.Code).IsRequired();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AppUserToken)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_AppUserToken_UserID");
            });

            modelBuilder.Entity<SystemError>(entity =>
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
        }
    }
}
