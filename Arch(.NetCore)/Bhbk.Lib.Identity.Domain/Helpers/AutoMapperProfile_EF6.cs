using AutoMapper;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF6.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Models.Me;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Domain.Helpers
{
    public class AutoMapperProfile_EF6 : Profile
    {
        public AutoMapperProfile_EF6()
        {
            /*
             * activity models
             */
            CreateMap<ActivityModel, uvw_Activities>()
                .ReverseMap();

            /*
             * claim models
             */
            CreateMap<ClaimModel, uvw_Claims>()
                .ReverseMap();

            /*
             * client models
             */
            CreateMap<AudienceModel, uvw_Audiences>()
                .ReverseMap();

            /*
             * issuer models
             */
            CreateMap<IssuerModel, uvw_Issuers>()
                .ReverseMap();

            /*
             * login models
             */
            CreateMap<LoginModel, uvw_Logins>()
                .ReverseMap();

            /*
             * message of the day models
             */
            CreateMap<MOTDModel, uvw_MOTDs>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => val.id))
                .ForMember(dest => dest.Title, src => src.MapFrom(val => val.title))
                .ForMember(dest => dest.Author, src => src.MapFrom(val => val.author))
                .ForMember(dest => dest.Quote, src => src.MapFrom(val => val.quote))
                .ForMember(dest => dest.Category, src => src.MapFrom(val => val.category))
                .ForMember(dest => dest.Date, src => src.MapFrom(val => val.date))
                .ForMember(dest => dest.Tags, src => src.MapFrom(val => string.Join(",", val.tags.Select(x => x))))
                .ForMember(dest => dest.Length, src => src.MapFrom(val => val.length))
                .ForMember(dest => dest.Background, src => src.MapFrom(val => val.background));

            CreateMap<uvw_MOTDs, MOTDModel>()
                .ForMember(dest => dest.id, src => src.MapFrom(val => val.Id))
                .ForMember(dest => dest.title, src => src.MapFrom(val => val.Title))
                .ForMember(dest => dest.author, src => src.MapFrom(val => val.Author))
                .ForMember(dest => dest.quote, src => src.MapFrom(val => val.Quote))
                .ForMember(dest => dest.category, src => src.MapFrom(val => val.Category))
                .ForMember(dest => dest.date, src => src.MapFrom(val => val.Date))
                .ForMember(dest => dest.tags, src => src.MapFrom(val => val.Tags.Split(',', StringSplitOptions.None).ToList()))
                .ForMember(dest => dest.length, src => src.MapFrom(val => val.Length))
                .ForMember(dest => dest.background, src => src.MapFrom(val => val.Background));

            /*
             * refresh models
             */
            CreateMap<RefreshModel, uvw_Refreshes>()
                .ReverseMap();

            /*
             * role models
             */
            CreateMap<RoleModel, uvw_Roles>()
                .ReverseMap();

            /*
             * user models
             */
            CreateMap<UserModel, uvw_Users>()
                .ReverseMap();
        }
    }
}
