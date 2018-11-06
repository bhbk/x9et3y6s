using AutoMapper;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.EntityModels;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Maps
{
    public class IssuerMaps : Profile
    {
        public IssuerMaps()
        {
            //issuer models
            CreateMap<IssuerCreate, AppIssuer>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Description, src => src.NullSubstitute(string.Empty))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.AppClient, src => src.Ignore())
                .ForMember(dest => dest.AppUserRefresh, src => src.Ignore());

            CreateMap<AppIssuer, IssuerModel>()
                .ForMember(dest => dest.Clients, src => src.MapFrom(val => val.AppClient
                    .ToDictionary(x => x.Id, x => x.Name)));

            CreateMap<IssuerUpdate, AppIssuer>()
                .ForMember(dest => dest.AppClient, src => src.Ignore())
                .ForMember(dest => dest.AppUserRefresh, src => src.Ignore());
        }
    }
}
