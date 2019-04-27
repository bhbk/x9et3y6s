using Bhbk.Lib.Core.Extensions;
using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Repositories;
using Microsoft.Extensions.Configuration;
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

        public AdminService(IConfigurationRoot conf, InstanceContext instance, HttpClient client)
        {
            _jwt = new ResourceOwnerHelper(conf, instance, client);
            HttpClient = new AdminRepository(conf, instance, client);
        }

        public JwtSecurityToken Jwt
        {
            get { return _jwt.JwtV2; }
            set { _jwt.JwtV2 = value; }
        }

        public AdminRepository HttpClient { get; }

        public Tuple<int, IEnumerable<ActivityModel>> Activity_GetV1(CascadePager model)
        {
            var response = HttpClient.Activity_GetV1(_jwt.JwtV2.RawData, model).Result;

            var ok = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<ActivityModel>>();
            var count = (int)ok["count"];

            return new Tuple<int, IEnumerable<ActivityModel>>(count, list);
        }

        public ClaimModel Claim_CreateV1(ClaimCreate model)
        {
            var response = HttpClient.Claim_CreateV1(_jwt.JwtV2.RawData, model).Result;

            return response.Content.ReadAsJsonAsync<ClaimModel>().Result;
        }

        public bool Claim_DeleteV1(Guid claimID)
        {
            var response = HttpClient.Claim_DeleteV1(_jwt.JwtV2.RawData, claimID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage
                + Environment.NewLine + response);
        }

        public Tuple<int, IEnumerable<ClaimModel>> Claim_GetV1(CascadePager model)
        {
            var response = HttpClient.Claim_GetV1(_jwt.JwtV2.RawData, model).Result;

            var ok = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<ClaimModel>>();
            var count = (int)ok["count"];

            return new Tuple<int, IEnumerable<ClaimModel>>(count, list);
        }

        public ClaimModel Claim_GetV1(string claimValue)
        {
            var response = HttpClient.Claim_GetV1(_jwt.JwtV2.RawData, claimValue).Result;

            return response.Content.ReadAsJsonAsync<ClaimModel>().Result;
        }

        public ClaimModel Claim_UpdateV1(ClaimModel model)
        {
            var response = HttpClient.Claim_UpdateV1(_jwt.JwtV2.RawData, model).Result;

            return response.Content.ReadAsJsonAsync<ClaimModel>().Result;
        }

        public ClientModel Client_CreateV1(ClientCreate model)
        {
            var response = HttpClient.Client_CreateV1(_jwt.JwtV2.RawData, model).Result;

            return response.Content.ReadAsJsonAsync<ClientModel>().Result;
        }

        public bool Client_DeleteV1(Guid clientID)
        {
            var response = HttpClient.Client_DeleteV1(_jwt.JwtV2.RawData, clientID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage
                + Environment.NewLine + response);
        }

        public bool Client_DeleteRefreshesV1(Guid clientID)
        {
            var response = HttpClient.Client_DeleteRefreshesV1(_jwt.JwtV2.RawData, clientID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage
                + Environment.NewLine + response);
        }

        public bool Client_DeleteRefreshV1(Guid clientID, Guid refreshID)
        {
            var response = HttpClient.Client_DeleteRefreshV1(_jwt.JwtV2.RawData, clientID, refreshID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage
                + Environment.NewLine + response);
        }

        public Tuple<int, IEnumerable<ClientModel>> Client_GetV1(CascadePager model)
        {
            var response = HttpClient.Client_GetV1(_jwt.JwtV2.RawData, model).Result;

            var ok = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<ClientModel>>();
            var count = (int)ok["count"];

            return new Tuple<int, IEnumerable<ClientModel>>(count, list);
        }

        public ClientModel Client_GetV1(string clientValue)
        {
            var response = HttpClient.Client_GetV1(_jwt.JwtV2.RawData, clientValue).Result;

            return response.Content.ReadAsJsonAsync<ClientModel>().Result;
        }

        public IEnumerable<RefreshModel> Client_GetRefreshesV1(string clientValue)
        {
            var response = HttpClient.Client_GetRefreshesV1(_jwt.JwtV2.RawData, clientValue).Result;

            return response.Content.ReadAsJsonAsync<IEnumerable<RefreshModel>>().Result;
        }

        public ClientModel Client_UpdateV1(ClientModel model)
        {
            var response = HttpClient.Client_UpdateV1(_jwt.JwtV2.RawData, model).Result;

            return response.Content.ReadAsJsonAsync<ClientModel>().Result;
        }

        public IssuerModel Issuer_CreateV1(IssuerCreate model)
        {
            var response = HttpClient.Issuer_CreateV1(_jwt.JwtV2.RawData, model).Result;

            return response.Content.ReadAsJsonAsync<IssuerModel>().Result;
        }

        public bool Issuer_DeleteV1(Guid issuerID)
        {
            var response = HttpClient.Issuer_DeleteV1(_jwt.JwtV2.RawData, issuerID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage
                + Environment.NewLine + response);
        }

        public IEnumerable<ClientModel> Issuer_GetClientsV1(string issuerValue)
        {
            var response = HttpClient.Issuer_GetClientsV1(_jwt.JwtV2.RawData, issuerValue).Result;

            return response.Content.ReadAsJsonAsync<IEnumerable<ClientModel>>().Result;
        }

        public Tuple<int, IEnumerable<IssuerModel>> Issuer_GetV1(CascadePager model)
        {
            var response = HttpClient.Issuer_GetV1(_jwt.JwtV2.RawData, model).Result;

            var ok = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<IssuerModel>>();
            var count = (int)ok["count"];

            return new Tuple<int, IEnumerable<IssuerModel>>(count, list);
        }

        public IssuerModel Issuer_GetV1(string issuerValue)
        {
            var response = HttpClient.Issuer_GetV1(_jwt.JwtV2.RawData, issuerValue).Result;

            return response.Content.ReadAsJsonAsync<IssuerModel>().Result;
        }

        public IssuerModel Issuer_UpdateV1(IssuerModel model)
        {
            var response = HttpClient.Issuer_UpdateV1(_jwt.JwtV2.RawData, model).Result;

            return response.Content.ReadAsJsonAsync<IssuerModel>().Result;
        }

        public LoginModel Login_CreateV1(LoginCreate model)
        {
            var response = HttpClient.Login_CreateV1(_jwt.JwtV2.RawData, model).Result;

            return response.Content.ReadAsJsonAsync<LoginModel>().Result;
        }

        public bool Login_DeleteV1(Guid loginID)
        {
            var response = HttpClient.Login_DeleteV1(_jwt.JwtV2.RawData, loginID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage
                + Environment.NewLine + response);
        }

        public Tuple<int, IEnumerable<LoginModel>> Login_GetV1(CascadePager model)
        {
            var response = HttpClient.Login_GetV1(_jwt.JwtV2.RawData, model).Result;

            var ok = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<LoginModel>>();
            var count = (int)ok["count"];

            return new Tuple<int, IEnumerable<LoginModel>>(count, list);
        }

        public LoginModel Login_GetV1(string loginValue)
        {
            var response = HttpClient.Login_GetV1(_jwt.JwtV2.RawData, loginValue).Result;

            return response.Content.ReadAsJsonAsync<LoginModel>().Result;
        }

        public LoginModel Login_UpdateV1(LoginModel model)
        {
            var response = HttpClient.Login_UpdateV1(_jwt.JwtV2.RawData, model).Result;

            return response.Content.ReadAsJsonAsync<LoginModel>().Result;
        }

        public bool Role_AddUserV1(Guid roleID, Guid userID)
        {
            var response = HttpClient.Role_AddUserV1(_jwt.JwtV2.RawData, roleID, userID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage
                + Environment.NewLine + response);
        }

        public RoleModel Role_CreateV1(RoleCreate model)
        {
            var response = HttpClient.Role_CreateV1(_jwt.JwtV2.RawData, model).Result;

            return response.Content.ReadAsJsonAsync<RoleModel>().Result;
        }

        public bool Role_DeleteV1(Guid roleID)
        {
            var response = HttpClient.Role_DeleteV1(_jwt.JwtV2.RawData, roleID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            return false;
        }

        public Tuple<int, IEnumerable<RoleModel>> Role_GetV1(CascadePager model)
        {
            var response = HttpClient.Role_GetV1(_jwt.JwtV2.RawData, model).Result;

            var ok = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<RoleModel>>();
            var count = (int)ok["count"];

            return new Tuple<int, IEnumerable<RoleModel>>(count, list);
        }

        public RoleModel Role_GetV1(string roleValue)
        {
            var response = HttpClient.Role_GetV1(_jwt.JwtV2.RawData, roleValue).Result;

            return response.Content.ReadAsJsonAsync<RoleModel>().Result;
        }

        public bool Role_RemoveUserV1(Guid roleID, Guid userID)
        {
            var response = HttpClient.Role_RemoveUserV1(_jwt.JwtV2.RawData, roleID, userID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage
                + Environment.NewLine + response);
        }

        public RoleModel Role_UpdateV1(RoleModel model)
        {
            var response = HttpClient.Role_UpdateV1(_jwt.JwtV2.RawData, model).Result;

            return response.Content.ReadAsJsonAsync<RoleModel>().Result;
        }

        public bool User_AddLoginV1(Guid userID, Guid loginID)
        {
            var response = HttpClient.User_AddLoginV1(_jwt.JwtV2.RawData, userID, loginID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage
                + Environment.NewLine + response);
        }

        public bool User_AddPasswordV1(Guid userID, UserAddPassword model)
        {
            var response = HttpClient.User_AddPasswordV1(_jwt.JwtV2.RawData, userID, model).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage
                + Environment.NewLine + response);
        }

        public UserModel User_CreateV1(UserCreate model)
        {
            var response = HttpClient.User_CreateV1(_jwt.JwtV2.RawData, model).Result;

            return response.Content.ReadAsJsonAsync<UserModel>().Result;
        }

        public UserModel User_CreateV1NoConfirm(UserCreate model)
        {
            var response = HttpClient.User_CreateV1NoConfirm(_jwt.JwtV2.RawData, model).Result;

            return response.Content.ReadAsJsonAsync<UserModel>().Result;
        }

        public bool User_DeleteV1(Guid userID)
        {
            var response = HttpClient.User_DeleteV1(_jwt.JwtV2.RawData, userID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage
                + Environment.NewLine + response);
        }

        public bool User_DeleteRefreshesV1(Guid userID)
        {
            var response = HttpClient.User_DeleteRefreshesV1(_jwt.JwtV2.RawData, userID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage
                + Environment.NewLine + response);
        }

        public bool User_DeleteRefreshV1(Guid userID, Guid refreshID)
        {
            var response = HttpClient.User_DeleteRefreshV1(_jwt.JwtV2.RawData, userID, refreshID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage
                + Environment.NewLine + response);
        }

        public IEnumerable<ClaimModel> User_GetClaimsV1(string userValue)
        {
            var response = HttpClient.User_GetClaimsV1(_jwt.JwtV2.RawData, userValue).Result;

            return response.Content.ReadAsJsonAsync<IEnumerable<ClaimModel>>().Result;
        }

        public IEnumerable<ClientModel> User_GetClientsV1(string userValue)
        {
            var response = HttpClient.User_GetClientsV1(_jwt.JwtV2.RawData, userValue).Result;

            return response.Content.ReadAsJsonAsync<IEnumerable<ClientModel>>().Result;
        }

        public IEnumerable<LoginModel> User_GetLoginsV1(string userValue)
        {
            var response = HttpClient.User_GetLoginsV1(_jwt.JwtV2.RawData, userValue).Result;

            return response.Content.ReadAsJsonAsync<IEnumerable<LoginModel>>().Result;
        }

        public IEnumerable<RefreshModel> User_GetRefreshesV1(string userValue)
        {
            var response = HttpClient.User_GetRefreshesV1(_jwt.JwtV2.RawData, userValue).Result;

            return response.Content.ReadAsJsonAsync<IEnumerable<RefreshModel>>().Result;
        }

        public IEnumerable<RoleModel> User_GetRolesV1(string userValue)
        {
            var response = HttpClient.User_GetRolesV1(_jwt.JwtV2.RawData, userValue).Result;

            return response.Content.ReadAsJsonAsync<IEnumerable<RoleModel>>().Result;
        }

        public Tuple<int, IEnumerable<UserModel>> User_GetV1(CascadePager model)
        {
            var response = HttpClient.User_GetV1(_jwt.JwtV2.RawData, model).Result;

            var ok = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<UserModel>>();
            var count = (int)ok["count"];

            return new Tuple<int, IEnumerable<UserModel>>(count, list);
        }

        public UserModel User_GetV1(string userValue)
        {
            var response = HttpClient.User_GetV1(_jwt.JwtV2.RawData, userValue).Result;

            return response.Content.ReadAsJsonAsync<UserModel>().Result;
        }

        public bool User_RemoveLoginV1(Guid roleID, Guid userID)
        {
            var response = HttpClient.User_RemoveLoginV1(_jwt.JwtV2.RawData, roleID, userID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage
                + Environment.NewLine + response);
        }

        public bool User_RemovePasswordV1(Guid userID)
        {
            var response = HttpClient.User_RemovePasswordV1(_jwt.JwtV2.RawData, userID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage
                + Environment.NewLine + response);
        }

        public bool User_SetPasswordV1(Guid userID, UserAddPassword model)
        {
            var response = HttpClient.User_SetPasswordV1(_jwt.JwtV2.RawData, userID, model).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage
                + Environment.NewLine + response);
        }

        public UserModel User_UpdateV1(UserModel model)
        {
            var response = HttpClient.User_UpdateV1(_jwt.JwtV2.RawData, model).Result;

            return response.Content.ReadAsJsonAsync<UserModel>().Result;
        }
    }
}
