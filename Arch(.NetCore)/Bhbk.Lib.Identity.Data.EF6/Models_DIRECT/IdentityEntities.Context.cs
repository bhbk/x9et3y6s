﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Bhbk.Lib.Identity.Data.EF6.Models_DIRECT
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class IdentityEntities_DIRECT : DbContext
    {
        public IdentityEntities_DIRECT()
            : base("name=IdentityEntities_DIRECT")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
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
    }
}
