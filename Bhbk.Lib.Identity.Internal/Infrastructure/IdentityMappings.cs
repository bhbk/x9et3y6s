using AutoMapper;
using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Internal.Infrastructure
{
    public class IdentityMappings : Profile
    {
        public IdentityMappings()
        {
            //activity models
            CreateMap<ActivityCreate, TActivities>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Client, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore())
                .ForMember(dest => dest.KeyValues, src => src.MapFrom(x => JsonConvert.SerializeObject(x.KeyValues)))
                .ForMember(dest => dest.OriginalValues, src => src.MapFrom(x => JsonConvert.SerializeObject(x.OriginalValues)))
                .ForMember(dest => dest.CurrentValues, src => src.MapFrom(x => JsonConvert.SerializeObject(x.CurrentValues)))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now));

            CreateMap<ActivityModel, TActivities>()
                .ForMember(dest => dest.Client, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore());

            //claim models
            CreateMap<ClaimCreate, TClaims>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.Actor, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.TRoleClaims, src => src.Ignore())
                .ForMember(dest => dest.TUserClaims, src => src.Ignore());

            CreateMap<ClaimModel, TClaims>()
                .ForMember(dest => dest.Actor, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.TRoleClaims, src => src.Ignore())
                .ForMember(dest => dest.TUserClaims, src => src.Ignore());

            CreateMap<TClaims, ClaimModel>();

            //client models
            CreateMap<ClientCreate, TClients>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Description, src => src.NullSubstitute(string.Empty))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.TActivities, src => src.Ignore())
                .ForMember(dest => dest.TClientUrls, src => src.Ignore())
                .ForMember(dest => dest.TCodes, src => src.Ignore())
                .ForMember(dest => dest.TRefreshes, src => src.Ignore())
                .ForMember(dest => dest.TRoles, src => src.Ignore());

            CreateMap<ClientModel, TClients>()
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.TActivities, src => src.Ignore())
                .ForMember(dest => dest.TClientUrls, src => src.Ignore())
                .ForMember(dest => dest.TCodes, src => src.Ignore())
                .ForMember(dest => dest.TRefreshes, src => src.Ignore())
                .ForMember(dest => dest.TRoles, src => src.Ignore());

            CreateMap<TClients, ClientModel>()
                .ForMember(dest => dest.Roles, src => src.MapFrom(val => val.TRoles
                    .ToDictionary(x => x.Id, x => x.Name)));

            //client uri models
            CreateMap<ClientUrlsCreate, TClientUrls>()
                    .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                    .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                    .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                    .ForMember(dest => dest.Client, src => src.Ignore());

            CreateMap<TClientUrls, ClientUrlsModel>();

            //code models
            CreateMap<CodeCreate, TCodes>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.Client, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore());

            CreateMap<TCodes, CodeModel>();

            //issuer models
            CreateMap<IssuerCreate, TIssuers>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Description, src => src.NullSubstitute(string.Empty))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.TClaims, src => src.Ignore())
                .ForMember(dest => dest.TClients, src => src.Ignore())
                .ForMember(dest => dest.TCodes, src => src.Ignore())
                .ForMember(dest => dest.TRefreshes, src => src.Ignore());

            CreateMap<TIssuers, IssuerModel>()
                .ForMember(dest => dest.Clients, src => src.MapFrom(val => val.TClients
                    .ToDictionary(x => x.Id, x => x.Name)));

            CreateMap<IssuerModel, TIssuers>()
                .ForMember(dest => dest.TClaims, src => src.Ignore())
                .ForMember(dest => dest.TClients, src => src.Ignore())
                .ForMember(dest => dest.TCodes, src => src.Ignore())
                .ForMember(dest => dest.TRefreshes, src => src.Ignore());

            //login models
            CreateMap<LoginCreate, TLogins>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Description, src => src.NullSubstitute(string.Empty))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.Actor, src => src.Ignore())
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.TUserLogins, src => src.Ignore());

            CreateMap<TLogins, LoginModel>()
                .ForMember(dest => dest.Users, src => src.MapFrom(val => val.TUserLogins.Where(x => x.LoginId == val.Id)
                    .ToDictionary(x => x.User.Id, x => x.User.Email)));

            CreateMap<LoginModel, TLogins>()
                .ForMember(dest => dest.Actor, src => src.Ignore())
                .ForMember(dest => dest.TUserLogins, src => src.Ignore());

            //refresh models
            CreateMap<RefreshCreate, TRefreshes>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.Client, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore());

            CreateMap<TRefreshes, RefreshModel>();

            //role models
            CreateMap<RoleCreate, TRoles>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Description, src => src.NullSubstitute(string.Empty))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, src => src.MapFrom(val => RandomValues.CreateBase64String(32)))
                .ForMember(dest => dest.Client, src => src.Ignore())
                .ForMember(dest => dest.TRoleClaims, src => src.Ignore())
                .ForMember(dest => dest.TUserRoles, src => src.Ignore());

            CreateMap<TRoles, RoleModel>()
                .ForMember(dest => dest.Users, src => src.MapFrom(val => val.TUserRoles.Where(x => x.RoleId == val.Id)
                    .ToDictionary(x => x.User.Id, x => x.User.Email)));

            CreateMap<RoleModel, TRoles>()
                .ForMember(dest => dest.Client, src => src.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, src => src.Ignore())
                .ForMember(dest => dest.TRoleClaims, src => src.Ignore())
                .ForMember(dest => dest.TUserRoles, src => src.Ignore());

            //user models
            CreateMap<UserCreate, TUsers>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Email, src => src.MapFrom(val => val.Email))
                .ForMember(dest => dest.EmailConfirmed, src => src.MapFrom(val => false))
                .ForMember(dest => dest.PhoneNumberConfirmed, src => src.MapFrom(val => false))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.LockoutEnd, src => src.Ignore())
                .ForMember(dest => dest.LastLoginFailure, src => src.Ignore())
                .ForMember(dest => dest.LastLoginSuccess, src => src.Ignore())
                .ForMember(dest => dest.AccessFailedCount, src => src.MapFrom(val => 0))
                .ForMember(dest => dest.AccessSuccessCount, src => src.MapFrom(val => 0))
                .ForMember(dest => dest.ConcurrencyStamp, src => src.MapFrom(val => RandomValues.CreateBase64String(32)))
                .ForMember(dest => dest.PasswordHash, src => src.Ignore())
                .ForMember(dest => dest.PasswordConfirmed, src => src.MapFrom(val => false))
                .ForMember(dest => dest.SecurityStamp, src => src.MapFrom(val => RandomValues.CreateBase64String(32)))
                .ForMember(dest => dest.TwoFactorEnabled, src => src.MapFrom(val => false))
                .ForMember(dest => dest.TActivities, src => src.Ignore())
                .ForMember(dest => dest.TClaims, src => src.Ignore())
                .ForMember(dest => dest.TCodes, src => src.Ignore())
                .ForMember(dest => dest.TLogins, src => src.Ignore())
                .ForMember(dest => dest.TRefreshes, src => src.Ignore())
                .ForMember(dest => dest.TUserClaims, src => src.Ignore())
                .ForMember(dest => dest.TUserLogins, src => src.Ignore())
                .ForMember(dest => dest.TUserRoles, src => src.Ignore());

            CreateMap<TUsers, UserModel>()
                .ForMember(dest => dest.Logins, src => src.MapFrom(val => val.TUserLogins.Where(x => x.UserId == val.Id)
                    .ToDictionary(x => x.LoginId, x => x.Login.Name)))
                .ForMember(dest => dest.Roles, src => src.MapFrom(val => val.TUserRoles.Where(x => x.UserId == val.Id)
                    .ToDictionary(x => x.Role.Id, x => x.Role.Name)));

            CreateMap<UserModel, TUsers>()
                .ForMember(dest => dest.ConcurrencyStamp, src => src.Ignore())
                .ForMember(dest => dest.PasswordHash, src => src.Ignore())
                .ForMember(dest => dest.TActivities, src => src.Ignore())
                .ForMember(dest => dest.TClaims, src => src.Ignore())
                .ForMember(dest => dest.TCodes, src => src.Ignore())
                .ForMember(dest => dest.TLogins, src => src.Ignore())
                .ForMember(dest => dest.TRefreshes, src => src.Ignore())
                .ForMember(dest => dest.TUserClaims, src => src.Ignore())
                .ForMember(dest => dest.TUserLogins, src => src.Ignore())
                .ForMember(dest => dest.TUserRoles, src => src.Ignore());
        }
    }
}
