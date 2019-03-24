using AutoMapper;
using Bhbk.Lib.Identity.Internal.Infrastructure;
using Xunit;

namespace Bhbk.Lib.Identity.Internal.Tests.Maps
{
    public class AutoMapperProfileTest
    {
        [Fact]
        public void Lib_CheckAutoMapper_Success()
        {
            Mapper.Initialize(x =>
            {
                x.AddProfile<IdentityMappings>();
            });
            Mapper.Configuration.AssertConfigurationIsValid();
        }
    }
}
