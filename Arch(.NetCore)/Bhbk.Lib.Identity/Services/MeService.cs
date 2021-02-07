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
    public class MeService : IMeService
    {
        public IOAuth2JwtGrant Grant { get; set; }
        public MeServiceRepository Endpoints { get; }

        public MeService()
            : this(new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build(), 
                  InstanceContext.DeployedOrLocal, new HttpClient())
        { }

        public MeService(IConfiguration conf)
            : this(conf, InstanceContext.DeployedOrLocal, new HttpClient())
        { }

        public MeService(IConfiguration conf, InstanceContext env, HttpClient http)
        {
            Endpoints = new MeServiceRepository(conf, env, http);
        }

        public async ValueTask<bool> Info_DeleteCodesV1()
        {
            var response = await Endpoints.Info_DeleteCodesV1(Grant.AccessToken.RawData);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> Info_DeleteCodeV1(Guid codeID)
        {
            var response = await Endpoints.Info_DeleteCodeV1(Grant.AccessToken.RawData, codeID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> Info_DeleteRefreshesV1()
        {
            var response = await Endpoints.Info_DeleteRefreshesV1(Grant.AccessToken.RawData);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> Info_DeleteRefreshV1(Guid refreshID)
        {
            var response = await Endpoints.Info_DeleteRefreshV1(Grant.AccessToken.RawData, refreshID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<IEnumerable<StateV1>> Info_GetCodesV1()
        {
            var response = await Endpoints.Info_GetCodesV1(Grant.AccessToken.RawData);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<IEnumerable<StateV1>>();

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<IEnumerable<RefreshV1>> Info_GetRefreshesV1()
        {
            var response = await Endpoints.Info_GetRefreshesV1(Grant.AccessToken.RawData);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<IEnumerable<RefreshV1>>();

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<MOTDTssV1> Info_GetMOTDV1()
        {
            var response = await Endpoints.Info_GetMOTDV1(Grant.AccessToken.RawData);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<MOTDTssV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<UserV1> Info_GetV1()
        {
            var response = await Endpoints.Info_GetV1(Grant.AccessToken.RawData);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> Info_SetPasswordV1(PasswordAddV1 model)
        {
            var response = await Endpoints.Info_SetPasswordV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> Info_SetTwoFactorV1(bool statusValue)
        {
            var response = await Endpoints.Info_SetTwoFactorV1(Grant.AccessToken.RawData, statusValue);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<UserV1> Info_UpdateV1(UserV1 model)
        {
            var response = await Endpoints.Info_UpdateV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> Info_UpdateCodeV1(string codeValue, string actionValue)
        {
            var response = await Endpoints.Info_UpdateCodeV1(Grant.AccessToken.RawData, codeValue, actionValue);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }
    }
}
