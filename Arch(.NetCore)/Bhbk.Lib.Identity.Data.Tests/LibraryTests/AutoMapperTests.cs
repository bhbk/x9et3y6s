﻿using AutoMapper;
using Bhbk.Lib.Identity.Domain.Profiles;
using Xunit;

namespace Bhbk.Lib.Identity.Data.Tests.LibraryTests
{
    [Collection("LibraryTests")]
    public class AutoMapperTests : BaseLibraryTests
    {
        [Fact]
        public void Lib_AutoMapper_Profile_Success()
        {
            var Mapper = new MapperConfiguration(
                    x => x.AddProfile<AutoMapperProfile_EFCore>()).CreateMapper();

            Mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Fact]
        public void Lib_AutoMapper_Profile_Success_TBL()
        {
            var Mapper = new MapperConfiguration(
                    x => x.AddProfile<AutoMapperProfile_EFCore_TBL>()).CreateMapper();

            Mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}
