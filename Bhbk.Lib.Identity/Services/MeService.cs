using Bhbk.Lib.Core.Extensions;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;

namespace Bhbk.Lib.Identity.Services
{
    public class MeService : IMeService
    {
        private readonly ResourceOwnerGrant _ropg;
        private readonly MeRepository _http;

        public MeService(IConfiguration conf)
            : this(conf, InstanceContext.DeployedOrLocal, new HttpClient()) { }

        public MeService(IConfiguration conf, InstanceContext instance, HttpClient client)
        {
            _ropg = new ResourceOwnerGrant(instance, client);
            _http = new MeRepository(conf, instance, client);
        }

        public JwtSecurityToken Jwt
        {
            get { return _ropg.RopgV2; }
            set { _ropg.RopgV2 = value; }
        }

        public MeRepository Http
        {
            get { return _http; }
        }

        public bool Info_DeleteCodesV1()
        {
            var response = Http.Info_DeleteCodesV1(_ropg.RopgV2.RawData).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool Info_DeleteCodeV1(Guid codeID)
        {
            var response = Http.Info_DeleteCodeV1(_ropg.RopgV2.RawData, codeID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool Info_DeleteRefreshesV1()
        {
            var response = Http.Info_DeleteRefreshesV1(_ropg.RopgV2.RawData).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool Info_DeleteRefreshV1(Guid refreshID)
        {
            var response = Http.Info_DeleteRefreshV1(_ropg.RopgV2.RawData, refreshID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public IEnumerable<StateModel> Info_GetCodesV1()
        {
            var response = Http.Info_GetCodesV1(_ropg.RopgV2.RawData).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<IEnumerable<StateModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public IEnumerable<RefreshModel> Info_GetRefreshesV1()
        {
            var response = Http.Info_GetRefreshesV1(_ropg.RopgV2.RawData).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<IEnumerable<RefreshModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public MotDType1Model Info_GetMOTDV1()
        {
            var response = Http.Info_GetMOTDV1().Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<MotDType1Model>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public UserModel Info_GetV1()
        {
            var response = Http.Info_GetV1(_ropg.RopgV2.RawData).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<UserModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool Info_SetPasswordV1(UserAddPassword model)
        {
            var response = Http.Info_SetPasswordV1(_ropg.RopgV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool Info_SetTwoFactorV1(bool statusValue)
        {
            var response = Http.Info_SetTwoFactorV1(_ropg.RopgV2.RawData, statusValue).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public UserModel Info_UpdateV1(UserModel model)
        {
            var response = Http.Info_UpdateV1(_ropg.RopgV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<UserModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool Info_UpdateCodeV1(string codeValue, string actionValue)
        {
            var response = Http.Info_UpdateCodeV1(_ropg.RopgV2.RawData, codeValue, actionValue).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }
    }
}
