using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EF.Models
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

        public virtual DbSet<tbl_Audience> tbl_Audiences { get; set; }
        public virtual DbSet<tbl_AudienceRole> tbl_AudienceRoles { get; set; }
        public virtual DbSet<tbl_UserAuthActivity> tbl_UserAuthActivities { get; set; }
        public virtual DbSet<tbl_AudienceAuthActivity> tbl_AudienceAuthActivities { get; set; }
        public virtual DbSet<tbl_ChatConversation> tbl_ChatConversations { get; set; }
        public virtual DbSet<tbl_ChatFavorite> tbl_ChatFavorites { get; set; }
        public virtual DbSet<tbl_ChatFile> tbl_ChatFiles { get; set; }
        public virtual DbSet<tbl_ChatMessage> tbl_ChatMessages { get; set; }
        public virtual DbSet<tbl_ChatPrompt> tbl_ChatPrompts { get; set; }
        public virtual DbSet<tbl_ChatPromptHistory> tbl_ChatPromptHistories { get; set; }
        public virtual DbSet<tbl_Claim> tbl_Claims { get; set; }
        public virtual DbSet<tbl_EmailActivity> tbl_EmailActivities { get; set; }
        public virtual DbSet<tbl_EmailQueue> tbl_EmailQueues { get; set; }
        public virtual DbSet<tbl_UserEntitlement> tbl_UserEntitlements { get; set; }
        public virtual DbSet<tbl_AudienceEntitlement> tbl_AudienceEntitlements { get; set; }
        public virtual DbSet<tbl_EntitlementScope> tbl_EntitlementScopes { get; set; }
        public virtual DbSet<tbl_EntitlementType> tbl_EntitlementTypes { get; set; }
        public virtual DbSet<tbl_Issuer> tbl_Issuers { get; set; }
        public virtual DbSet<tbl_Job> tbl_Jobs { get; set; }
        public virtual DbSet<tbl_JobSetting> tbl_JobSettings { get; set; }
        public virtual DbSet<tbl_LLMProvider> tbl_LLMProviders { get; set; }
        public virtual DbSet<tbl_LLMProviderSetting> tbl_LLMProviderSettings { get; set; }
        public virtual DbSet<tbl_LoginProvider> tbl_LoginProviders { get; set; }
        public virtual DbSet<tbl_Quote> tbl_Quotes { get; set; }
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
        public virtual DbSet<tbl_UserLoginProvider> tbl_UserLoginProviders { get; set; }
        public virtual DbSet<tbl_UserRole> tbl_UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

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

            modelBuilder.Entity<tbl_UserAuthActivity>(entity =>
            {
                entity.ToTable("tbl_UserAuthActivity");

                entity.HasIndex(e => e.Id, "IX_tbl_UserAuthActivity")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.LocalEndpoint).HasMaxLength(128);

                entity.Property(e => e.LoginOutcome)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.LoginType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.RemoteEndpoint).HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_UserAuthActivities)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_tbl_UserAuthActivity_UserID");
            });

            modelBuilder.Entity<tbl_AudienceAuthActivity>(entity =>
            {
                entity.HasKey(e => new { e.UserAuthActivityId, e.AudienceId });

                entity.ToTable("tbl_AudienceAuthActivity");

                entity.HasIndex(e => new { e.UserAuthActivityId, e.AudienceId }, "IX_tbl_AudienceAuthActivity")
                    .IsUnique();

                entity.HasOne(d => d.UserAuthActivity)
                    .WithMany(p => p.tbl_AudienceAuthActivities)
                    .HasForeignKey(d => d.UserAuthActivityId)
                    .HasConstraintName("FK_tbl_AudienceAuthActivity_UserAuthActivityID");

                entity.HasOne(d => d.Audience)
                    .WithMany(p => p.tbl_AudienceAuthActivities)
                    .HasForeignKey(d => d.AudienceId)
                    .HasConstraintName("FK_tbl_AudienceAuthActivity_AudienceID");
            });

            modelBuilder.Entity<tbl_ChatConversation>(entity =>
            {
                entity.ToTable("tbl_ChatConversation");

                entity.HasIndex(e => e.Id, "IX_tbl_ChatConversation")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Title).HasMaxLength(256);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_ChatConversations)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_tbl_ChatConversation_UserID");
            });

            modelBuilder.Entity<tbl_ChatFile>(entity =>
            {
                entity.ToTable("tbl_ChatFile");

                entity.HasIndex(e => e.Id, "IX_tbl_ChatFile")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.ContentType)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.FileContent).IsRequired();

                entity.Property(e => e.Summary).HasMaxLength(512);

                entity.HasOne(d => d.Conversation)
                    .WithMany(p => p.tbl_ChatFiles)
                    .HasForeignKey(d => d.ConversationId)
                    .HasConstraintName("FK_tbl_ChatFile_ConversationID");

                entity.HasOne(d => d.Message)
                    .WithMany()
                    .HasForeignKey(d => d.MessageId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_tbl_ChatFile_MessageID");
            });

            modelBuilder.Entity<tbl_ChatMessage>(entity =>
            {
                entity.ToTable("tbl_ChatMessage");

                entity.HasIndex(e => e.Id, "IX_tbl_ChatMessage")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.HasOne(d => d.Conversation)
                    .WithMany(p => p.tbl_ChatMessages)
                    .HasForeignKey(d => d.ConversationId)
                    .HasConstraintName("FK_tbl_ChatMessage_ConversationID");
            });

            modelBuilder.Entity<tbl_ChatPrompt>(entity =>
            {
                entity.ToTable("tbl_ChatPrompt");

                entity.HasIndex(e => e.Id, "IX_tbl_ChatPrompt")
                    .IsUnique();

                entity.HasIndex(e => e.Name, "IX_tbl_ChatPrompt_Name")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.PromptType)
                    .IsRequired()
                    .HasMaxLength(32);

                entity.Property(e => e.Content).IsRequired();
            });

            modelBuilder.Entity<tbl_ChatFavorite>(entity =>
            {
                entity.ToTable("tbl_ChatFavorite");

                entity.HasIndex(e => e.Id, "IX_tbl_ChatFavorite")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.Prompt)
                    .IsRequired()
                    .HasMaxLength(2048);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_ChatFavorites)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_tbl_ChatFavorite_UserID");
            });

            modelBuilder.Entity<tbl_ChatPromptHistory>(entity =>
            {
                entity.ToTable("tbl_ChatPromptHistory");

                entity.HasIndex(e => e.Id, "IX_tbl_ChatPromptHistory")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.PromptText)
                    .IsRequired()
                    .HasMaxLength(2048);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_ChatPromptHistories)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_tbl_ChatPromptHistory_UserID");
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

            modelBuilder.Entity<tbl_UserEntitlement>(entity =>
            {
                entity.ToTable("tbl_UserEntitlement");

                entity.HasIndex(e => e.Id, "IX_tbl_UserEntitlement")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_UserEntitlements)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_tbl_UserEntitlement_UserID");

                entity.HasOne(d => d.EntitlementType)
                    .WithMany(p => p.tbl_UserEntitlements)
                    .HasForeignKey(d => d.EntitlementTypeId)
                    .HasConstraintName("FK_tbl_UserEntitlement_EntitlementTypeID");

                entity.HasOne(d => d.EntitlementScope)
                    .WithMany(p => p.tbl_UserEntitlements)
                    .HasForeignKey(d => d.EntitlementScopeId)
                    .HasConstraintName("FK_tbl_UserEntitlement_EntitlementScopeID");

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.tbl_UserEntitlements)
                    .HasForeignKey(d => d.IssuerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_tbl_UserEntitlement_IssuerID");

                entity.HasOne(d => d.Audience)
                    .WithMany(p => p.tbl_UserEntitlements)
                    .HasForeignKey(d => d.AudienceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_tbl_UserEntitlement_AudienceID");
            });

            modelBuilder.Entity<tbl_AudienceEntitlement>(entity =>
            {
                entity.ToTable("tbl_AudienceEntitlement");

                entity.HasIndex(e => e.Id, "IX_tbl_AudienceEntitlement")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.Audience)
                    .WithMany(p => p.tbl_AudienceEntitlements)
                    .HasForeignKey(d => d.AudienceId)
                    .HasConstraintName("FK_tbl_AudienceEntitlement_AudienceID");

                entity.HasOne(d => d.EntitlementType)
                    .WithMany(p => p.tbl_AudienceEntitlements)
                    .HasForeignKey(d => d.EntitlementTypeId)
                    .HasConstraintName("FK_tbl_AudienceEntitlement_EntitlementTypeID");

                entity.HasOne(d => d.EntitlementScope)
                    .WithMany(p => p.tbl_AudienceEntitlements)
                    .HasForeignKey(d => d.EntitlementScopeId)
                    .HasConstraintName("FK_tbl_AudienceEntitlement_EntitlementScopeID");

                entity.HasOne(d => d.Issuer)
                    .WithMany(p => p.tbl_AudienceEntitlements)
                    .HasForeignKey(d => d.IssuerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_tbl_AudienceEntitlement_IssuerID");
            });

            modelBuilder.Entity<tbl_EntitlementScope>(entity =>
            {
                entity.ToTable("tbl_EntitlementScope");

                entity.HasIndex(e => e.Id, "IX_tbl_EntitlementScope")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Description).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<tbl_EntitlementType>(entity =>
            {
                entity.ToTable("tbl_EntitlementType");

                entity.HasIndex(e => e.Id, "IX_tbl_EntitlementType")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Description).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(64);
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

            modelBuilder.Entity<tbl_Job>(entity =>
            {
                entity.ToTable("tbl_Job");

                entity.HasIndex(e => e.Id, "IX_tbl_Job")
                    .IsUnique();

                entity.HasIndex(e => e.Name, "IX_tbl_Job_Name")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.Description).HasMaxLength(256);
            });

            modelBuilder.Entity<tbl_JobSetting>(entity =>
            {
                entity.ToTable("tbl_JobSetting");

                entity.HasIndex(e => e.Id, "IX_tbl_JobSetting")
                    .IsUnique();

                entity.HasIndex(e => new { e.JobId, e.ConfigKey }, "IX_tbl_JobSetting_JobKey")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ConfigKey)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.ConfigValue)
                    .IsRequired()
                    .HasMaxLength(1024);

                entity.HasOne(d => d.Job)
                    .WithMany(p => p.tbl_JobSettings)
                    .HasForeignKey(d => d.JobId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_tbl_JobSetting_JobID");
            });

            modelBuilder.Entity<tbl_LLMProvider>(entity =>
            {
                entity.ToTable("tbl_LLMProvider");

                entity.HasIndex(e => e.Id, "IX_tbl_LLMProvider")
                    .IsUnique();

                entity.HasIndex(e => e.Name, "IX_tbl_LLMProvider_Name")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<tbl_LLMProviderSetting>(entity =>
            {
                entity.ToTable("tbl_LLMProviderSetting");

                entity.HasIndex(e => e.Id, "IX_tbl_LLMProviderSetting")
                    .IsUnique();

                entity.HasIndex(e => new { e.ProviderId, e.ConfigKey }, "IX_tbl_LLMProviderSetting_ProviderKey")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ConfigKey)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.ConfigValue)
                    .IsRequired()
                    .HasMaxLength(1024);

                entity.HasOne(d => d.Provider)
                    .WithMany(p => p.tbl_LLMProviderSettings)
                    .HasForeignKey(d => d.ProviderId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_tbl_LLMProviderSetting_ProviderID");
            });

            modelBuilder.Entity<tbl_LoginProvider>(entity =>
            {
                entity.ToTable("tbl_LoginProvider");

                entity.HasIndex(e => e.Id, "IX_tbl_LoginProvider")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Description).HasMaxLength(256);

                entity.Property(e => e.ProviderKey).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<tbl_Quote>(entity =>
            {
                entity.ToTable("tbl_Quote");

                entity.HasIndex(e => e.Id, "IX_tbl_Quote")
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
                    .HasMaxLength(1024)
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

                entity.Property(e => e.StateValue).HasMaxLength(2048);

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
                    .IsRequired()
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.ToPhoneNumber)
                    .IsRequired()
                    .HasMaxLength(15)
                    .IsUnicode(false);
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

            modelBuilder.Entity<tbl_UserLoginProvider>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProviderId });

                entity.ToTable("tbl_UserLoginProvider");

                entity.HasIndex(e => new { e.UserId, e.LoginProviderId }, "IX_tbl_UserLoginProvider")
                    .IsUnique();

                entity.HasOne(d => d.LoginProvider)
                    .WithMany(p => p.tbl_UserLoginProviders)
                    .HasForeignKey(d => d.LoginProviderId)
                    .HasConstraintName("FK_tbl_UserLoginProvider_LoginProviderID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_UserLoginProviders)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_tbl_UserLoginProvider_UserID");
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
