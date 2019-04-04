using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class DatabaseContext : DbContext
    {
        public virtual DbSet<LKActivityTypes> LKActivityTypes { get; set; }
        public virtual DbSet<LKClaimTypes> LKClaimTypes { get; set; }
        public virtual DbSet<LKCodeTypes> LKCodeTypes { get; set; }
        public virtual DbSet<TActivities> TActivities { get; set; }
        public virtual DbSet<TClaims> TClaims { get; set; }
        public virtual DbSet<TClients> TClients { get; set; }
        public virtual DbSet<TExceptions> TExceptions { get; set; }
        public virtual DbSet<TIssuers> TIssuers { get; set; }
        public virtual DbSet<TLogins> TLogins { get; set; }
        public virtual DbSet<TRefreshes> TRefreshes { get; set; }
        public virtual DbSet<TRoleClaims> TRoleClaims { get; set; }
        public virtual DbSet<TRoles> TRoles { get; set; }
        public virtual DbSet<TStates> TStates { get; set; }
        public virtual DbSet<TUrls> TUrls { get; set; }
        public virtual DbSet<TUserClaims> TUserClaims { get; set; }
        public virtual DbSet<TUserLogins> TUserLogins { get; set; }
        public virtual DbSet<TUserRoles> TUserRoles { get; set; }
        public virtual DbSet<TUsers> TUsers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            modelBuilder.Entity<LKActivityTypes>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_ActivityTypes")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<LKClaimTypes>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<LKCodeTypes>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<TActivities>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_TActivity")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ActivityType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.TActivities)
                    .HasForeignKey(d => d.ClientId)
                    .HasConstraintName("FK_TActivities_ClientID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TActivities)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_TActivities_UserID");
            });

            modelBuilder.Entity<TClaims>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_TClaims")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Type).IsRequired();

                entity.Property(e => e.Value).IsRequired();

                entity.Property(e => e.ValueType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.HasOne(d => d.Actor)
                    .WithMany(p => p.TClaims)
                    .HasForeignKey(d => d.ActorId)
                    .HasConstraintName("FK_TClaims_ActorID");

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.TClaims)
                    .HasForeignKey(d => d.IssuerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TClaims_IssuerID");
            });

            modelBuilder.Entity<TClients>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_TClients")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ClientKey).IsRequired();

                entity.Property(e => e.ClientType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.Name).IsRequired();

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.TClients)
                    .HasForeignKey(d => d.IssuerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TClients_IssuerID");
            });

            modelBuilder.Entity<TExceptions>(entity =>
            {
                entity.HasKey(e => e.ErrorID)
                    .HasName("PK_SystemErrorInfo");

                entity.Property(e => e.ErrorID)
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

            modelBuilder.Entity<TIssuers>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_TIssuers")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.IssuerKey).IsRequired();

                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<TLogins>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_TLogins")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name).IsRequired();

                entity.HasOne(d => d.Actor)
                    .WithMany(p => p.TLogins)
                    .HasForeignKey(d => d.ActorId)
                    .HasConstraintName("FK_TLogins_ActorID");
            });

            modelBuilder.Entity<TRefreshes>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_TRefreshes")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.RefreshType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.RefreshValue).IsRequired();

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.TRefreshes)
                    .HasForeignKey(d => d.ClientId)
                    .HasConstraintName("FK_TRefreshes_ClientID");

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.TRefreshes)
                    .HasForeignKey(d => d.IssuerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TRefreshes_IssuerID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TRefreshes)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_TRefreshes_UserID");
            });

            modelBuilder.Entity<TRoleClaims>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.ClaimId });

                entity.HasIndex(e => new { e.RoleId, e.ClaimId })
                    .HasName("IX_TRoleClaims")
                    .IsUnique();

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.HasOne(d => d.Claim)
                    .WithMany(p => p.TRoleClaims)
                    .HasForeignKey(d => d.ClaimId)
                    .HasConstraintName("FK_TRoleClaims_ClaimID");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.TRoleClaims)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_TRoleClaims_RoleID");
            });

            modelBuilder.Entity<TRoles>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_TRoles")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name).IsRequired();

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.TRoles)
                    .HasForeignKey(d => d.ClientId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TRoles_ClientID");
            });

            modelBuilder.Entity<TStates>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_UserTokens")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.NonceType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.TStates)
                    .HasForeignKey(d => d.ClientId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_TCodes_TClients");

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.TStates)
                    .HasForeignKey(d => d.IssuerId)
                    .HasConstraintName("FK_TCodes_TIssuers");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TStates)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_TCodes_UserID");
            });

            modelBuilder.Entity<TUrls>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_TUrls")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.TUrls)
                    .HasForeignKey(d => d.ClientId)
                    .HasConstraintName("FK_TUrls_ClientID");
            });

            modelBuilder.Entity<TUserClaims>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.ClaimId });

                entity.HasIndex(e => new { e.UserId, e.ClaimId })
                    .HasName("IX_TUserClaims")
                    .IsUnique();

                entity.HasOne(d => d.Claim)
                    .WithMany(p => p.TUserClaims)
                    .HasForeignKey(d => d.ClaimId)
                    .HasConstraintName("FK_TUserClaims_ClaimID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TUserClaims)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_TUserClaims_UserID");
            });

            modelBuilder.Entity<TUserLogins>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginId });

                entity.HasIndex(e => new { e.UserId, e.LoginId })
                    .HasName("IX_TUserLogins")
                    .IsUnique();

                entity.HasOne(d => d.Login)
                    .WithMany(p => p.TUserLogins)
                    .HasForeignKey(d => d.LoginId)
                    .HasConstraintName("FK_TUserLogins_LoginID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TUserLogins)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_TUserLogins_UserID");
            });

            modelBuilder.Entity<TUserRoles>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });

                entity.HasIndex(e => new { e.UserId, e.RoleId })
                    .HasName("IX_TUserRoles")
                    .IsUnique();

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.TUserRoles)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_TUserRoles_RoleID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TUserRoles)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_TUserRoles_UserID");
            });

            modelBuilder.Entity<TUsers>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_TUsers")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Email).IsRequired();

                entity.Property(e => e.FirstName).IsRequired();

                entity.Property(e => e.LastLoginFailure).HasColumnType("datetime");

                entity.Property(e => e.LastLoginSuccess).HasColumnType("datetime");

                entity.Property(e => e.LastName).IsRequired();

                entity.Property(e => e.PhoneNumber).HasMaxLength(16);

                entity.Property(e => e.PhoneNumberConfirmed).HasDefaultValueSql("((0))");
            });
        }
    }
}
