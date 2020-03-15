﻿using Bhbk.Lib.DataAccess.EFCore.UnitOfWorks;
using Bhbk.Lib.Identity.Data.EFCore.Repositories_DIRECT;

namespace Bhbk.Lib.Identity.Data.EFCore.Infrastructure_DIRECT
{
    public interface IUnitOfWork : IGenericUnitOfWork
    {
        public ActivityRepository Activities { get; }
        public AudienceRepository Audiences { get; }
        public ClaimRepository Claims { get; }
        public IssuerRepository Issuers { get; }
        public LoginRepository Logins { get; }
        public MOTDRepository MOTDs { get; }
        public QueueEmailRepository QueueEmails { get; }
        public QueueTextRepository QueueTexts { get; }
        public RoleRepository Roles { get; }
        public RefreshRepository Refreshes { get; }
        public SettingRepository Settings { get; }
        public StateRepository States { get; }
        public UrlRepository Urls { get; }
        public UserRepository Users { get; }
    }
}