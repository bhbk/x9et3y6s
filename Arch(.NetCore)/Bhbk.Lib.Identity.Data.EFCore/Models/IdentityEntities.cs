using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

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

        public virtual DbSet<uvw_Activities> uvw_Activities { get; set; }
        public virtual DbSet<uvw_AudienceRoles> uvw_AudienceRoles { get; set; }
        public virtual DbSet<uvw_Audiences> uvw_Audiences { get; set; }
        public virtual DbSet<uvw_Claims> uvw_Claims { get; set; }
        public virtual DbSet<uvw_Issuers> uvw_Issuers { get; set; }
        public virtual DbSet<uvw_Logins> uvw_Logins { get; set; }
        public virtual DbSet<uvw_MOTDs> uvw_MOTDs { get; set; }
        public virtual DbSet<uvw_QueueEmails> uvw_QueueEmails { get; set; }
        public virtual DbSet<uvw_QueueTexts> uvw_QueueTexts { get; set; }
        public virtual DbSet<uvw_Refreshes> uvw_Refreshes { get; set; }
        public virtual DbSet<uvw_Roles> uvw_Roles { get; set; }
        public virtual DbSet<uvw_Settings> uvw_Settings { get; set; }
        public virtual DbSet<uvw_States> uvw_States { get; set; }
        public virtual DbSet<uvw_Urls> uvw_Urls { get; set; }
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
            modelBuilder.Entity<uvw_Activities>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Activities", "svc");

                entity.Property(e => e.ActivityType)
                    .IsRequired()
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<uvw_AudienceRoles>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_AudienceRoles", "svc");
            });

            modelBuilder.Entity<uvw_Audiences>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Audiences", "svc");

                entity.Property(e => e.AudienceType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.Name).IsRequired();
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

            modelBuilder.Entity<uvw_Issuers>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Issuers", "svc");

                entity.Property(e => e.IssuerKey).IsRequired();

                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<uvw_Logins>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Logins", "svc");

                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<uvw_MOTDs>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_MOTDs", "svc");

                entity.Property(e => e.Author)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Background).IsUnicode(false);

                entity.Property(e => e.Category).IsUnicode(false);

                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.Quote)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Tags).IsUnicode(false);

                entity.Property(e => e.Title).IsUnicode(false);
            });

            modelBuilder.Entity<uvw_QueueEmails>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_QueueEmails", "svc");

                entity.Property(e => e.FromDisplay).IsUnicode(false);

                entity.Property(e => e.FromEmail).IsUnicode(false);

                entity.Property(e => e.HtmlContent).IsUnicode(false);

                entity.Property(e => e.PlaintextContent).IsUnicode(false);

                entity.Property(e => e.Subject)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.ToDisplay).IsUnicode(false);

                entity.Property(e => e.ToEmail).IsUnicode(false);
            });

            modelBuilder.Entity<uvw_QueueTexts>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_QueueTexts", "svc");

                entity.Property(e => e.Body)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.FromPhoneNumber)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.ToPhoneNumber)
                    .HasMaxLength(15)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<uvw_Refreshes>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Refreshes", "svc");

                entity.Property(e => e.RefreshType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.RefreshValue).IsRequired();
            });

            modelBuilder.Entity<uvw_Roles>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Roles", "svc");

                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<uvw_Settings>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Settings", "svc");

                entity.Property(e => e.ConfigKey)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.ConfigValue)
                    .IsRequired()
                    .IsUnicode(false);
            });

            modelBuilder.Entity<uvw_States>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_States", "svc");

                entity.Property(e => e.StateType)
                    .IsRequired()
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<uvw_Urls>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Urls", "svc");
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
