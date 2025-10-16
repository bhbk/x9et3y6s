using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Services
{
    public class UserService : IUserService
    {
        public IOAuth2JwtGrant Grant { get; set; }
        public UserServiceRepository Endpoints { get; }

        public UserService()
            : this(new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build(),
                  InstanceContext.DeployedOrLocal, new HttpClient())
        { }

        public UserService(IConfiguration conf)
            : this(conf, InstanceContext.DeployedOrLocal, new HttpClient())
        { }

        public UserService(IConfiguration conf, InstanceContext env, HttpClient http)
        {
            Endpoints = new UserServiceRepository(conf, env, http);
        }

        #region Profile

        public async ValueTask<UserV1> Profile_GetV1()
        {
            var response = await Endpoints.Profile_GetV1(Grant.AccessToken.RawData);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<UserV1> Profile_UpdateV1(UserV1 model)
        {
            var response = await Endpoints.Profile_UpdateV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        #endregion

        #region Session

        public async ValueTask<bool> Session_DeleteCodesV1()
        {
            var response = await Endpoints.Session_DeleteCodesV1(Grant.AccessToken.RawData);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> Session_DeleteCodeV1(Guid codeID)
        {
            var response = await Endpoints.Session_DeleteCodeV1(Grant.AccessToken.RawData, codeID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> Session_DeleteRefreshesV1()
        {
            var response = await Endpoints.Session_DeleteRefreshesV1(Grant.AccessToken.RawData);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> Session_DeleteRefreshV1(Guid refreshID)
        {
            var response = await Endpoints.Session_DeleteRefreshV1(Grant.AccessToken.RawData, refreshID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<IEnumerable<StateV1>> Session_GetCodesV1()
        {
            var response = await Endpoints.Session_GetCodesV1(Grant.AccessToken.RawData);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<IEnumerable<StateV1>>();

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<IEnumerable<RefreshV1>> Session_GetRefreshesV1()
        {
            var response = await Endpoints.Session_GetRefreshesV1(Grant.AccessToken.RawData);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<IEnumerable<RefreshV1>>();

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> Session_UpdateCodeV1(string codeValue, string actionValue)
        {
            var response = await Endpoints.Session_UpdateCodeV1(Grant.AccessToken.RawData, codeValue, actionValue);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        #endregion

        #region Credentials

        public async ValueTask<bool> Credentials_SetPasswordV1(PasswordAddV1 model)
        {
            var response = await Endpoints.Credentials_SetPasswordV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        #endregion

        #region Quote

        public async ValueTask<QuoteV1> Quote_GetV1()
        {
            var response = await Endpoints.Quote_GetV1(Grant.AccessToken.RawData);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<QuoteV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        #endregion
    }
}
