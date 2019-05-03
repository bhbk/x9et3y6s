using Bhbk.Lib.Core.Extensions;
using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Repositories;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;

namespace Bhbk.Lib.Identity.Services
{
    public class AdminService : IAdminService
    {
        private readonly ResourceOwnerHelper _jwt;

        public AdminService()
            : this(InstanceContext.DeployedOrLocal, new HttpClient()) { }

        public AdminService(InstanceContext instance, HttpClient client)
        {
            _jwt = new ResourceOwnerHelper(instance, client);
            Http = new AdminRepository(instance, client);
        }

        public JwtSecurityToken Jwt
        {
            get { return _jwt.JwtV2; }
            set { _jwt.JwtV2 = value; }
        }

        public AdminRepository Http { get; }

        public Tuple<int, IEnumerable<ActivityModel>> Activity_GetV1(CascadePager model)
        {
            var response = Http.Activity_GetV1(_jwt.JwtV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
            {
                var ok = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<ActivityModel>>();
                var count = (int)ok["count"];

                return new Tuple<int, IEnumerable<ActivityModel>>(count, list);
            }

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public ClaimModel Claim_CreateV1(ClaimCreate model)
        {
            var response = Http.Claim_CreateV1(_jwt.JwtV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<ClaimModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool Claim_DeleteV1(Guid claimID)
        {
            var response = Http.Claim_DeleteV1(_jwt.JwtV2.RawData, claimID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public Tuple<int, IEnumerable<ClaimModel>> Claim_GetV1(CascadePager model)
        {
            var response = Http.Claim_GetV1(_jwt.JwtV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
            {
                var ok = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<ClaimModel>>();
                var count = (int)ok["count"];

                return new Tuple<int, IEnumerable<ClaimModel>>(count, list);
            }

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public ClaimModel Claim_GetV1(string claimValue)
        {
            var response = Http.Claim_GetV1(_jwt.JwtV2.RawData, claimValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<ClaimModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public ClaimModel Claim_UpdateV1(ClaimModel model)
        {
            var response = Http.Claim_UpdateV1(_jwt.JwtV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<ClaimModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public ClientModel Client_CreateV1(ClientCreate model)
        {
            var response = Http.Client_CreateV1(_jwt.JwtV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<ClientModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool Client_DeleteV1(Guid clientID)
        {
            var response = Http.Client_DeleteV1(_jwt.JwtV2.RawData, clientID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool Client_DeleteRefreshesV1(Guid clientID)
        {
            var response = Http.Client_DeleteRefreshesV1(_jwt.JwtV2.RawData, clientID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool Client_DeleteRefreshV1(Guid clientID, Guid refreshID)
        {
            var response = Http.Client_DeleteRefreshV1(_jwt.JwtV2.RawData, clientID, refreshID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public Tuple<int, IEnumerable<ClientModel>> Client_GetV1(CascadePager model)
        {
            var response = Http.Client_GetV1(_jwt.JwtV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
            {
                var ok = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<ClientModel>>();
                var count = (int)ok["count"];

                return new Tuple<int, IEnumerable<ClientModel>>(count, list);
            }

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public ClientModel Client_GetV1(string clientValue)
        {
            var response = Http.Client_GetV1(_jwt.JwtV2.RawData, clientValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<ClientModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public IEnumerable<RefreshModel> Client_GetRefreshesV1(string clientValue)
        {
            var response = Http.Client_GetRefreshesV1(_jwt.JwtV2.RawData, clientValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<IEnumerable<RefreshModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public ClientModel Client_UpdateV1(ClientModel model)
        {
            var response = Http.Client_UpdateV1(_jwt.JwtV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<ClientModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public IssuerModel Issuer_CreateV1(IssuerCreate model)
        {
            var response = Http.Issuer_CreateV1(_jwt.JwtV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<IssuerModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool Issuer_DeleteV1(Guid issuerID)
        {
            var response = Http.Issuer_DeleteV1(_jwt.JwtV2.RawData, issuerID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public IEnumerable<ClientModel> Issuer_GetClientsV1(string issuerValue)
        {
            var response = Http.Issuer_GetClientsV1(_jwt.JwtV2.RawData, issuerValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<IEnumerable<ClientModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public Tuple<int, IEnumerable<IssuerModel>> Issuer_GetV1(CascadePager model)
        {
            var response = Http.Issuer_GetV1(_jwt.JwtV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
            {
                var ok = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<IssuerModel>>();
                var count = (int)ok["count"];

                return new Tuple<int, IEnumerable<IssuerModel>>(count, list);
            }

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public IssuerModel Issuer_GetV1(string issuerValue)
        {
            var response = Http.Issuer_GetV1(_jwt.JwtV2.RawData, issuerValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<IssuerModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public IssuerModel Issuer_UpdateV1(IssuerModel model)
        {
            var response = Http.Issuer_UpdateV1(_jwt.JwtV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<IssuerModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public LoginModel Login_CreateV1(LoginCreate model)
        {
            var response = Http.Login_CreateV1(_jwt.JwtV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<LoginModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool Login_DeleteV1(Guid loginID)
        {
            var response = Http.Login_DeleteV1(_jwt.JwtV2.RawData, loginID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public Tuple<int, IEnumerable<LoginModel>> Login_GetV1(CascadePager model)
        {
            var response = Http.Login_GetV1(_jwt.JwtV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
            {
                var ok = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<LoginModel>>();
                var count = (int)ok["count"];

                return new Tuple<int, IEnumerable<LoginModel>>(count, list);
            }

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public LoginModel Login_GetV1(string loginValue)
        {
            var response = Http.Login_GetV1(_jwt.JwtV2.RawData, loginValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<LoginModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public LoginModel Login_UpdateV1(LoginModel model)
        {
            var response = Http.Login_UpdateV1(_jwt.JwtV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<LoginModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool Role_AddUserV1(Guid roleID, Guid userID)
        {
            var response = Http.Role_AddUserV1(_jwt.JwtV2.RawData, roleID, userID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public RoleModel Role_CreateV1(RoleCreate model)
        {
            var response = Http.Role_CreateV1(_jwt.JwtV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<RoleModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool Role_DeleteV1(Guid roleID)
        {
            var response = Http.Role_DeleteV1(_jwt.JwtV2.RawData, roleID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public Tuple<int, IEnumerable<RoleModel>> Role_GetV1(CascadePager model)
        {
            var response = Http.Role_GetV1(_jwt.JwtV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
            {
                var ok = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<RoleModel>>();
                var count = (int)ok["count"];

                return new Tuple<int, IEnumerable<RoleModel>>(count, list);
            }

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public RoleModel Role_GetV1(string roleValue)
        {
            var response = Http.Role_GetV1(_jwt.JwtV2.RawData, roleValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<RoleModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool Role_RemoveUserV1(Guid roleID, Guid userID)
        {
            var response = Http.Role_RemoveUserV1(_jwt.JwtV2.RawData, roleID, userID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public RoleModel Role_UpdateV1(RoleModel model)
        {
            var response = Http.Role_UpdateV1(_jwt.JwtV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<RoleModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool User_AddClaimV1(Guid userID, Guid claimID)
        {
            var response = Http.User_AddClaimV1(_jwt.JwtV2.RawData, userID, claimID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool User_AddLoginV1(Guid userID, Guid loginID)
        {
            var response = Http.User_AddLoginV1(_jwt.JwtV2.RawData, userID, loginID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool User_AddPasswordV1(Guid userID, UserAddPassword model)
        {
            var response = Http.User_AddPasswordV1(_jwt.JwtV2.RawData, userID, model).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public UserModel User_CreateV1(UserCreate model)
        {
            var response = Http.User_CreateV1(_jwt.JwtV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<UserModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public UserModel User_CreateV1NoConfirm(UserCreate model)
        {
            var response = Http.User_CreateV1NoConfirm(_jwt.JwtV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<UserModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool User_DeleteV1(Guid userID)
        {
            var response = Http.User_DeleteV1(_jwt.JwtV2.RawData, userID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool User_DeleteRefreshesV1(Guid userID)
        {
            var response = Http.User_DeleteRefreshesV1(_jwt.JwtV2.RawData, userID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool User_DeleteRefreshV1(Guid userID, Guid refreshID)
        {
            var response = Http.User_DeleteRefreshV1(_jwt.JwtV2.RawData, userID, refreshID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public Tuple<int, IEnumerable<MotDType1Model>> User_GetMOTDsV1(CascadePager model)
        {
            var response = Http.User_GetMOTDsV1(_jwt.JwtV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
            {
                var ok = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<MotDType1Model>>();
                var count = (int)ok["count"];

                return new Tuple<int, IEnumerable<MotDType1Model>>(count, list);
            }

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public IEnumerable<ClaimModel> User_GetClaimsV1(string userValue)
        {
            var response = Http.User_GetClaimsV1(_jwt.JwtV2.RawData, userValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<IEnumerable<ClaimModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public IEnumerable<ClientModel> User_GetClientsV1(string userValue)
        {
            var response = Http.User_GetClientsV1(_jwt.JwtV2.RawData, userValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<IEnumerable<ClientModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public IEnumerable<LoginModel> User_GetLoginsV1(string userValue)
        {
            var response = Http.User_GetLoginsV1(_jwt.JwtV2.RawData, userValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<IEnumerable<LoginModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public IEnumerable<RefreshModel> User_GetRefreshesV1(string userValue)
        {
            var response = Http.User_GetRefreshesV1(_jwt.JwtV2.RawData, userValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<IEnumerable<RefreshModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public IEnumerable<RoleModel> User_GetRolesV1(string userValue)
        {
            var response = Http.User_GetRolesV1(_jwt.JwtV2.RawData, userValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<IEnumerable<RoleModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public Tuple<int, IEnumerable<UserModel>> User_GetV1(CascadePager model)
        {
            var response = Http.User_GetV1(_jwt.JwtV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
            {
                var ok = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<UserModel>>();
                var count = (int)ok["count"];

                return new Tuple<int, IEnumerable<UserModel>>(count, list);
            }

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public UserModel User_GetV1(string userValue)
        {
            var response = Http.User_GetV1(_jwt.JwtV2.RawData, userValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<UserModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool User_RemoveLoginV1(Guid roleID, Guid userID)
        {
            var response = Http.User_RemoveLoginV1(_jwt.JwtV2.RawData, roleID, userID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool User_RemovePasswordV1(Guid userID)
        {
            var response = Http.User_RemovePasswordV1(_jwt.JwtV2.RawData, userID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool User_SetPasswordV1(Guid userID, UserAddPassword model)
        {
            var response = Http.User_SetPasswordV1(_jwt.JwtV2.RawData, userID, model).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public UserModel User_UpdateV1(UserModel model)
        {
            var response = Http.User_UpdateV1(_jwt.JwtV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<UserModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }
    }
}
