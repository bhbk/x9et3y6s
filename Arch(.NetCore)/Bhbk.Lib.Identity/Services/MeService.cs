using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Services
{
    public class MeService : IMeService
    {
        private readonly IOAuth2JwtGrant _ropg;
        private readonly MeRepository _http;

        public MeService(IConfiguration conf)
            : this(conf, InstanceContext.DeployedOrLocal, new HttpClient()) { }

        public MeService(IConfiguration conf, InstanceContext instance, HttpClient http)
        {
            _ropg = new ResourceOwnerGrantV2(conf, instance, http);
            _http = new MeRepository(conf, instance, http);
        }

        public JwtSecurityToken Jwt
        {
            get { return _ropg.AccessToken; }
            set { _ropg.AccessToken = value; }
        }

        public MeRepository Http
        {
            get { return _http; }
        }

        public async ValueTask<bool> Info_DeleteCodesV1()
        {
            var response = await Http.Info_DeleteCodesV1(_ropg.AccessToken.RawData);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Info_DeleteCodeV1(Guid codeID)
        {
            var response = await Http.Info_DeleteCodeV1(_ropg.AccessToken.RawData, codeID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Info_DeleteRefreshesV1()
        {
            var response = await Http.Info_DeleteRefreshesV1(_ropg.AccessToken.RawData);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Info_DeleteRefreshV1(Guid refreshID)
        {
            var response = await Http.Info_DeleteRefreshV1(_ropg.AccessToken.RawData, refreshID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<IEnumerable<StateModel>> Info_GetCodesV1()
        {
            var response = await Http.Info_GetCodesV1(_ropg.AccessToken.RawData);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<IEnumerable<StateModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<IEnumerable<RefreshV1>> Info_GetRefreshesV1()
        {
            var response = await Http.Info_GetRefreshesV1(_ropg.AccessToken.RawData);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<IEnumerable<RefreshV1>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<MOTDV1> Info_GetMOTDV1()
        {
            var response = await Http.Info_GetMOTDV1();

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<MOTDV1>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<UserV1> Info_GetV1()
        {
            var response = await Http.Info_GetV1(_ropg.AccessToken.RawData);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<UserV1>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Info_SetPasswordV1(PasswordAddV1 model)
        {
            var response = await Http.Info_SetPasswordV1(_ropg.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Info_SetTwoFactorV1(bool statusValue)
        {
            var response = await Http.Info_SetTwoFactorV1(_ropg.AccessToken.RawData, statusValue);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<UserV1> Info_UpdateV1(UserV1 model)
        {
            var response = await Http.Info_UpdateV1(_ropg.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<UserV1>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Info_UpdateCodeV1(string codeValue, string actionValue)
        {
            var response = await Http.Info_UpdateCodeV1(_ropg.AccessToken.RawData, codeValue, actionValue);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }
    }
}
