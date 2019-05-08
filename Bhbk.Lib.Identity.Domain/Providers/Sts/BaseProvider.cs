﻿using AutoMapper;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Helpers;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Lib.Identity.Domain.Providers.Sts
{
    public class BaseProvider : IDisposable
    {
        protected IUoWService UoW;
        protected IMapper Mapper => new MapperConfiguration(x => x.AddProfile<MapperProfile>()).CreateMapper();

        protected BaseProvider(IConfiguration conf, IContextService instance)
        {
            UoW = new UoWService(conf, instance);
        }

        public void Dispose()
        {
            UoW.Dispose();
        }
    }
}