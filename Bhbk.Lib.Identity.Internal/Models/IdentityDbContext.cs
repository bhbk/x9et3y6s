using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Bhbk.Lib.Identity.Internal.Models
{
    public partial class IdentityDbContext : DbContext
    {
        public virtual DbSet<tbl_Activities> tbl_Activities { get; set; }
        public virtual DbSet<tbl_ActivityTypes> tbl_ActivityTypes { get; set; }
        public virtual DbSet<tbl_ClaimTypes> tbl_ClaimTypes { get; set; }
        public virtual DbSet<tbl_Claims> tbl_Claims { get; set; }
        public virtual DbSet<tbl_Clients> tbl_Clients { get; set; }
        public virtual DbSet<tbl_Exceptions> tbl_Exceptions { get; set; }
        public virtual DbSet<tbl_Issuers> tbl_Issuers { get; set; }
        public virtual DbSet<tbl_Logins> tbl_Logins { get; set; }
        public virtual DbSet<tbl_Refreshes> tbl_Refreshes { get; set; }
        public virtual DbSet<tbl_RoleClaims> tbl_RoleClaims { get; set; }
        public virtual DbSet<tbl_Roles> tbl_Roles { get; set; }
        public virtual DbSet<tbl_StateTypes> tbl_StateTypes { get; set; }
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

            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

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

            modelBuilder.Entity<tbl_ActivityTypes>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_ActivityTypes")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<tbl_ClaimTypes>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(64);
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
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Claims_IssuerID");
            });

            modelBuilder.Entity<tbl_Clients>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_Clients")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ClientKey).IsRequired();

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

            modelBuilder.Entity<tbl_Exceptions>(entity =>
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
                    .OnDelete(DeleteBehavior.ClientSetNull)
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

            modelBuilder.Entity<tbl_StateTypes>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(64);
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

                entity.Property(e => e.LastLoginFailure).HasColumnType("datetime");

                entity.Property(e => e.LastLoginSuccess).HasColumnType("datetime");

                entity.Property(e => e.LastName).IsRequired();

                entity.Property(e => e.PhoneNumber).HasMaxLength(16);

                entity.Property(e => e.PhoneNumberConfirmed).HasDefaultValueSql("((0))");
            });
        }
    }
}
