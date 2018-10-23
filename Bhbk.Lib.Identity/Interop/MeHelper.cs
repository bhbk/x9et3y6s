using Bhbk.Lib.Core.Primitives.Enums;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Interop
{
    public class MeHelper
    {
        protected readonly IConfigurationRoot _conf;
        protected readonly ContextType _context;
        protected readonly HttpClient _connect;

        public MeHelper(IConfigurationRoot conf, ContextType context)
        {
            if (conf == null)
                throw new ArgumentNullException();

            var connect = new HttpClientHandler();

            //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
            connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

            _context = context;
            _conf = conf;
            _connect = new HttpClient(connect);
        }

        public MeHelper(IConfigurationRoot conf, TestServer connect)
        {
            if (conf == null)
                throw new ArgumentNullException();

            _context = ContextType.UnitTest;
            _conf = conf;
            _connect = connect.CreateClient();
        }

        public async Task<HttpResponseMessage> DetailGetV1(JwtSecurityToken jwt)
        {
            var endpoint = "/me/v1";

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }
    }
}
