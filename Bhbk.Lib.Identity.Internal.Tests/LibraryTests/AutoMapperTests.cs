﻿using AutoMapper;
using Bhbk.Lib.Identity.Internal.Helpers;
using Xunit;

namespace Bhbk.Lib.Identity.Internal.Tests.LibraryTests
{
    public class AutoMapperTests
    {
        [Fact]
        public void Lib_AutoMapper_Profile_Success()
        {
            Mapper.Initialize(x =>
            {
                x.AddProfile<MapperProfile>();
            });

            Mapper.Configuration.AssertConfigurationIsValid();
        }
    }
}
