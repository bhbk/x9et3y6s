using AutoMapper;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.EntityModels;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Maps
{
    public class ClientMaps : Profile
    {
        public ClientMaps()
        {
            //client models
            CreateMap<ClientCreate, AppClient>()
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.Description, src => src.NullSubstitute(string.Empty))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.AppClientUri, src => src.Ignore())
                .ForMember(dest => dest.AppRole, src => src.Ignore());

            CreateMap<ClientUpdate, AppClient>()
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.AppClientUri, src => src.Ignore())
                .ForMember(dest => dest.AppRole, src => src.Ignore())
                .ReverseMap();

            CreateMap<ClientModel, AppClient>()
                .ForMember(dest => dest.Issuer, src => src.Ignore())
                .ForMember(dest => dest.AppClientUri, src => src.Ignore())
                .ForMember(dest => dest.AppRole, src => src.Ignore())
                .ReverseMap();

            CreateMap<AppClient, ClientModel>()
                .ForMember(dest => dest.Roles, src => src.MapFrom(val => val.AppRole
                    .ToDictionary(x => x.Id, x => x.Name)));

            //client uri models
            CreateMap<ClientUriCreate, AppClientUri>()
                    .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                    .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now))
                    .ForMember(dest => dest.LastUpdated, src => src.Ignore())
                    .ForMember(dest => dest.Actor, src => src.Ignore())
                    .ForMember(dest => dest.Client, src => src.Ignore());

            CreateMap<AppClientUri, ClientUriModel>();

            CreateMap<ClientUriUpdate, AppClientUri>()
                .ForMember(dest => dest.Actor, src => src.Ignore())
                .ForMember(dest => dest.Client, src => src.Ignore());
        }
    }
}
