using AutoMapper;
using Bhbk.Lib.Identity.Domain.Helpers;
using Xunit;

namespace Bhbk.Lib.Identity.Domain.Tests.LibraryTests
{
    public class AutoMapperTests
    {
        private IMapper Mapper => new MapperConfiguration(
                    x => x.AddProfile<MapperProfile>()).CreateMapper();

        [Fact]
        public void Lib_AutoMapper_Profile_Success()
        {
            Mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}
