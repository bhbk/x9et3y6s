using AutoMapper;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EFCore.Tests.LibraryTests
{
    public class AutoMapperTests
    {
        [Fact]
        public void Lib_AutoMapper_Profile_Success()
        {
            var Mapper = new MapperConfiguration(
                    x => x.AddProfile<AutoMapperProfile_EFCore>()).CreateMapper();

            Mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Fact]
        public void Lib_AutoMapper_Profile_Success_Direct()
        {
            var Mapper = new MapperConfiguration(
                    x => x.AddProfile<AutoMapperProfile_EFCore_DIRECT>()).CreateMapper();

            Mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}
