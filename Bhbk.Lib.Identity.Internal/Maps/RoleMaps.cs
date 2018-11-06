using AutoMapper;
using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.EntityModels;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Maps
{
    public class RoleMaps : Profile
    {
        public RoleMaps()
        {
            //role models
            CreateMap<RoleCreate, AppRole>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.NormalizedName, src => src.MapFrom(val => val.Name))
                .ForMember(dest => dest.Description, src => src.NullSubstitute(string.Empty))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, src => src.MapFrom(val => RandomValues.CreateBase64String(32)))
                .ForMember(dest => dest.Client, src => src.Ignore())
                .ForMember(dest => dest.AppRoleClaim, src => src.Ignore())
                .ForMember(dest => dest.AppUserRole, src => src.Ignore());

            CreateMap<AppRole, RoleModel>()
                .ForMember(dest => dest.Users, src => src.MapFrom(val => val.AppUserRole.Where(x => x.RoleId == val.Id)
                    .ToDictionary(x => x.User.Id, x => x.User.Email)));

            CreateMap<RoleUpdate, AppRole>()
                .ForMember(dest => dest.NormalizedName, src => src.MapFrom(val => val.Name))
                .ForMember(dest => dest.ConcurrencyStamp, src => src.MapFrom(val => RandomValues.CreateBase64String(32)))
                .ForMember(dest => dest.Client, src => src.Ignore())
                .ForMember(dest => dest.AppRoleClaim, src => src.Ignore())
                .ForMember(dest => dest.AppUserRole, src => src.Ignore());
        }
    }
}
