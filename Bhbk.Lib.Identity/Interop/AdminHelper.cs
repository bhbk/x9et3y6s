using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Core.Primitives.Enums;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Interop
{
    public class AdminClient : AdminHelper
    {
        public AdminClient(IConfigurationRoot conf, ContextType context)
            : base(conf, context) { }
    }

    public class AdminTester : AdminHelper
    {
        public AdminTester(IConfigurationRoot conf, TestServer connect)
            : base(conf, connect) { }
    }

    //https://oauth.com/playground/
    public class AdminHelper
    {
        protected readonly IConfigurationRoot _conf;
        protected readonly ContextType _context;
        protected readonly HttpClient _connect;

        public AdminHelper(IConfigurationRoot conf, ContextType context)
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

        public AdminHelper(IConfigurationRoot conf, TestServer connect)
        {
            if (conf == null)
                throw new ArgumentNullException();

            _context = ContextType.UnitTest;
            _conf = conf;
            _connect = connect.CreateClient();
        }

        public async Task<HttpResponseMessage> AudienceCreateV1(JwtSecurityToken jwt, AudienceCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/audience/v1";

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> AudienceDeleteV1(JwtSecurityToken jwt, Guid audienceID)
        {
            var endpoint = "/audience/v1/" + audienceID.ToString();

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.DeleteAsync(endpoint);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> AudienceGetV1(JwtSecurityToken jwt, string audience)
        {
            var endpoint = "/audience/v1/" + audience;

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> ClientCreateV1(JwtSecurityToken jwt, ClientCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/client/v1";

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> ClientDeleteV1(JwtSecurityToken jwt, Guid clientID)
        {
            var endpoint = "/client/v1/" + clientID.ToString();

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.DeleteAsync(endpoint);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> ClientGetV1(JwtSecurityToken jwt, string client)
        {
            var endpoint = "/client/v1/" + client;

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> LoginAddUserV1(JwtSecurityToken jwt, Guid loginID, Guid userID, UserLoginCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/login/v1/" + loginID.ToString() + "/add/" + userID.ToString();

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> LoginCreateV1(JwtSecurityToken jwt, LoginCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/login/v1";

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> LoginDeleteV1(JwtSecurityToken jwt, Guid loginID)
        {
            var endpoint = "/login/v1/" + loginID.ToString();

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.DeleteAsync(endpoint);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> LoginGetV1(JwtSecurityToken jwt, string login)
        {
            var endpoint = "/login/v1/" + login;

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RoleAddToUserV1(JwtSecurityToken jwt, Guid roleID, Guid userID)
        {
            var endpoint = "/role/v1/" + roleID.ToString() + "/add/" + userID.ToString();

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RoleCreateV1(JwtSecurityToken jwt, RoleCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/role/v1";

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RoleDeleteV1(JwtSecurityToken jwt, Guid roleID)
        {
            var endpoint = "/role/v1/" + roleID.ToString();

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.DeleteAsync(endpoint);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RoleGetV1(JwtSecurityToken jwt, string role)
        {
            var endpoint = "/role/v1/" + role;

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RoleRemoveFromUserV1(JwtSecurityToken jwt, Guid roleID, Guid userID)
        {
            var endpoint = "/role/v1/" + roleID.ToString() + "/remove/" + userID.ToString();

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> UserCreateV1(JwtSecurityToken jwt, UserCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/user/v1";

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> UserCreateV1NoConfirm(JwtSecurityToken jwt, UserCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/user/v1/no-confirm";

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> UserDeleteV1(JwtSecurityToken jwt, Guid userID)
        {
            var endpoint = "/user/v1/" + userID.ToString();

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.DeleteAsync(endpoint);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> UserGetV1(JwtSecurityToken jwt, string user)
        {
            var endpoint = "/user/v1/" + user;

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> UserSetPasswordV1(JwtSecurityToken jwt, UserAddPassword model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/user/v1/" + model.Id.ToString() + "/set-password";

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.PutAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }
    }
}
