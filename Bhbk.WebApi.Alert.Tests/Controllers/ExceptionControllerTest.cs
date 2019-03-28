using Bhbk.Lib.Identity.Providers;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Alert.Tests.Controllers
{
    [Collection("AlertTestCollection")]
    public class ExceptionControllerTest
    {
        private readonly StartupTest _factory;
        private readonly HttpClient _client;
        private readonly AlertClient _endpoints;

        public ExceptionControllerTest(StartupTest factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _endpoints = new AlertClient(_factory.Conf, _factory.UoW.Situation, _client);
        }

        [Fact(Skip = "NotImplemented")]
        public async Task Api_ExceptionV1_Enqueue_Fail()
        {

        }

        [Fact(Skip = "NotImplemented")]
        public async Task Api_ExceptionV1_Enqueue_Success()
        {

        }
    }
}
