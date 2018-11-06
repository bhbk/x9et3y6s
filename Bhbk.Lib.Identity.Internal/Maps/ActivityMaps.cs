using AutoMapper;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.EntityModels;
using Newtonsoft.Json;
using System;

namespace Bhbk.Lib.Identity.Maps
{
    public class ActivityMaps : Profile
    {
        public ActivityMaps()
        {
            //activity models
            CreateMap<ActivityCreate, AppActivity>()
                .ForMember(dest => dest.Actor, src => src.Ignore())
                .ForMember(dest => dest.Id, src => src.MapFrom(val => Guid.NewGuid()))
                .ForMember(dest => dest.KeyValues, src => src.MapFrom(x => JsonConvert.SerializeObject(x.KeyValues)))
                .ForMember(dest => dest.OriginalValues, src => src.MapFrom(x => JsonConvert.SerializeObject(x.OriginalValues)))
                .ForMember(dest => dest.CurrentValues, src => src.MapFrom(x => JsonConvert.SerializeObject(x.CurrentValues)))
                .ForMember(dest => dest.Created, src => src.MapFrom(val => DateTime.Now));

            CreateMap<ActivityModel, AppActivity>()
                .ForMember(dest => dest.Actor, src => src.Ignore())
                .ReverseMap();
        }
    }
}
