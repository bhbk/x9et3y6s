﻿using Bhbk.Lib.Identity.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

//TODO https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-2.1
namespace Bhbk.Lib.Identity.Factory
{
    public class UserClaimFactory<T> : AppUserClaim
    {
        public UserClaimFactory(AppUserClaim claim)
        {
            this.UserId = claim.UserId;
            this.ClaimType = claim.ClaimType;
            this.ClaimValue = claim.ClaimValue;
            this.ClaimValueType = claim.ClaimValueType;
        }

        public UserClaimFactory(UserClaimCreate claim)
        {

        }

        public UserClaimFactory(UserClaimUpdate claim)
        {

        }

        public AppUserClaim ToStore()
        {
            return new AppUserClaim
            {
                UserId = this.UserId,
                ClaimType = this.ClaimType,
                ClaimValue = this.ClaimValue,
                ClaimValueType = this.ClaimValueType
            };
        }

        public Claim ToClient()
        {
            return new Claim(this.ClaimType,
                this.ClaimValue,
                this.ClaimValueType);
        }
    }

    public abstract class UserClaimBase
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string ClaimType { get; set; }

        [Required]
        public string ClaimValue { get; set; }

        public string ClaimValueType { get; set; }

        public string Issuer { get; set; }

        public string OriginalIssuer { get; set; }

        public string Subject { get; set; }

        public IDictionary<string, string> Properties { get; set; }

        [DefaultValue(false)]
        public bool Immutable { get; set; }
    }

    public class UserClaimCreate: UserClaimBase { }

    public class UserClaimResult : UserClaimBase
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }
    }

    public class UserClaimUpdate : UserClaimBase
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }
    }
}
