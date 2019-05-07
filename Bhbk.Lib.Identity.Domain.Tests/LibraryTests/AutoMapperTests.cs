using AutoMapper;
using Bhbk.Lib.Identity.Data.Helpers;
using Xunit;

namespace Bhbk.Lib.Identity.Domain.Tests.LibraryTests
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
