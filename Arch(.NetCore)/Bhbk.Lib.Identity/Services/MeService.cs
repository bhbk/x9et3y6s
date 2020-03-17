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
        public MeRepository Http { get; }
        public IOAuth2JwtGrant Grant { get; set; }

        public MeService(IConfiguration conf)
            : this(conf, InstanceContext.DeployedOrLocal, new HttpClient()) { }

        public MeService(IConfiguration conf, InstanceContext instance, HttpClient http)
        {
            Http = new MeRepository(conf, instance, http);
        }

        public JwtSecurityToken Jwt
        {
            get { return Grant.Jwt; }
            set { Grant.Jwt = value; }
        }

        public async ValueTask<bool> Info_DeleteCodesV1()
        {
            var response = await Http.Info_DeleteCodesV1(Grant.Jwt.RawData);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Info_DeleteCodeV1(Guid codeID)
        {
            var response = await Http.Info_DeleteCodeV1(Grant.Jwt.RawData, codeID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Info_DeleteRefreshesV1()
        {
            var response = await Http.Info_DeleteRefreshesV1(Grant.Jwt.RawData);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Info_DeleteRefreshV1(Guid refreshID)
        {
            var response = await Http.Info_DeleteRefreshV1(Grant.Jwt.RawData, refreshID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<IEnumerable<StateV1>> Info_GetCodesV1()
        {
            var response = await Http.Info_GetCodesV1(Grant.Jwt.RawData);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<IEnumerable<StateV1>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<IEnumerable<RefreshV1>> Info_GetRefreshesV1()
        {
            var response = await Http.Info_GetRefreshesV1(Grant.Jwt.RawData);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<IEnumerable<RefreshV1>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<MOTDTssV1> Info_GetMOTDV1()
        {
            var response = await Http.Info_GetMOTDV1(Grant.Jwt.RawData);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<MOTDTssV1>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<UserV1> Info_GetV1()
        {
            var response = await Http.Info_GetV1(Grant.Jwt.RawData);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<UserV1>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Info_SetPasswordV1(PasswordAddV1 model)
        {
            var response = await Http.Info_SetPasswordV1(Grant.Jwt.RawData, model);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Info_SetTwoFactorV1(bool statusValue)
        {
            var response = await Http.Info_SetTwoFactorV1(Grant.Jwt.RawData, statusValue);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<UserV1> Info_UpdateV1(UserV1 model)
        {
            var response = await Http.Info_UpdateV1(Grant.Jwt.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<UserV1>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Info_UpdateCodeV1(string codeValue, string actionValue)
        {
            var response = await Http.Info_UpdateCodeV1(Grant.Jwt.RawData, codeValue, actionValue);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }
    }
}
