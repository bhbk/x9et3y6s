using AutoMapper;
using Bhbk.Lib.Identity.Domain.Helpers;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EF6.Tests.LibraryTests
{
    public class AutoMapperTests
    {
        private IMapper Mapper => new MapperConfiguration(
                    x => x.AddProfile<AutoMapperProfile_EF6>()).CreateMapper();

        [Fact]
        public void Lib_AutoMapper_Profile_Success()
        {
            Mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}
