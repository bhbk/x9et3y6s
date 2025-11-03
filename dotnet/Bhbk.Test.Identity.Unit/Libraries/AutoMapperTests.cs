using AutoMapper;
using Bhbk.Lib.Identity.Domain.Profiles;
using Xunit;

namespace Bhbk.Test.Identity.Unit.Libraries
{
    [Collection("LibraryTests")]
    public class AutoMapperTests : BaseLibraryTests
    {
        [Fact]
        public void Lib_AutoMapper_Profile_Success()
        {
            var Mapper = new MapperConfiguration(
                    x => x.AddProfile<AutoMapperProfile>()).CreateMapper();

            Mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

    }
}
