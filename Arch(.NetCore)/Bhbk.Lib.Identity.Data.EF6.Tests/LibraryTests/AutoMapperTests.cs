using AutoMapper;
using Bhbk.Lib.Identity.Domain.Profiles;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EF6.Tests.LibraryTests
{
    [Collection("LibraryTests")]
    public class AutoMapperTests : BaseLibraryTests
    {
        [Fact]
        public void Lib_AutoMapper_Profile_Success()
        {
            var Mapper = new MapperConfiguration(
                    x => x.AddProfile<AutoMapperProfile_EF6>()).CreateMapper();

            Mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Fact]
        public void Lib_AutoMapper_Profile_Success_Direct()
        {
            var Mapper = new MapperConfiguration(
                    x => x.AddProfile<AutoMapperProfile_EF6_DIRECT>()).CreateMapper();

            Mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}
