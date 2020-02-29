using AutoMapper;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EFCore.Tests.LibraryTests
{
    public class AutoMapperTests
    {
        private IMapper Mapper => new MapperConfiguration(
                    x => x.AddProfile<AutoMapperProfile_EFCore>()).CreateMapper();

        [Fact]
        public void Lib_AutoMapper_Profile_Success()
        {
            Mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}
