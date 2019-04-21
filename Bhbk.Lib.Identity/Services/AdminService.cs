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
        private readonly JwtHelper _jwt;
        private readonly AdminRepository _repo;

        public AdminService(IConfigurationRoot conf, InstanceContext instance, HttpClient client)
        {
            _jwt = new JwtHelper(conf, instance, client);
            _repo = new AdminRepository(conf, instance, client);
        }

        public JwtSecurityToken Jwt
        {
            get { return _jwt.ResourceOwnerV2; }
            set { _jwt.ResourceOwnerV2 = value; }
        }

        public AdminRepository Repo
        {
            get { return _repo; }
        }

        public Tuple<int, IEnumerable<ActivityModel>> ActivityGetV1(CascadePager model)
        {
            var response = _repo.Activity_GetV1(_jwt.ResourceOwnerV2.RawData, model).Result;

            var ok = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<ActivityModel>>();
            var count = (int)ok["count"];

            return new Tuple<int, IEnumerable<ActivityModel>>(count, list);
        }

        public ClaimModel ClaimCreateV1(ClaimCreate model)
        {
            var response = _repo.Claim_CreateV1(_jwt.ResourceOwnerV2.RawData, model).Result;

            return response.Content.ReadAsJsonAsync<ClaimModel>().Result;
        }

        public void ClaimDeleteV1(Guid claimID)
        {
            var response = _repo.Claim_DeleteV1(_jwt.ResourceOwnerV2.RawData, claimID).Result;
        }

        public Tuple<int, IEnumerable<ClaimModel>> ClaimGetV1(CascadePager model)
        {
            var response = _repo.Claim_GetV1(_jwt.ResourceOwnerV2.RawData, model).Result;

            var ok = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<ClaimModel>>();
            var count = (int)ok["count"];

            return new Tuple<int, IEnumerable<ClaimModel>>(count, list);
        }

        public ClaimModel ClaimGetV1(string claimValue)
        {
            var response = _repo.Claim_GetV1(_jwt.ResourceOwnerV2.RawData, claimValue).Result;

            return response.Content.ReadAsJsonAsync<ClaimModel>().Result;
        }

        public ClaimModel ClaimUpdateV1(ClaimModel model)
        {
            var response = _repo.Claim_UpdateV1(_jwt.ResourceOwnerV2.RawData, model).Result;

            return response.Content.ReadAsJsonAsync<ClaimModel>().Result;
        }

        public ClientModel ClientCreateV1(ClientCreate model)
        {
            var response = _repo.Client_CreateV1(_jwt.ResourceOwnerV2.RawData, model).Result;

            return response.Content.ReadAsJsonAsync<ClientModel>().Result;
        }

        public void ClientDeleteV1(Guid clientID)
        {
            var response = _repo.Client_DeleteV1(_jwt.ResourceOwnerV2.RawData, clientID).Result;
        }

        public Tuple<int, IEnumerable<ClientModel>> ClientGetV1(CascadePager model)
        {
            var response = _repo.Client_GetV1(_jwt.ResourceOwnerV2.RawData, model).Result;

            var ok = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<ClientModel>>();
            var count = (int)ok["count"];

            return new Tuple<int, IEnumerable<ClientModel>>(count, list);
        }

        public ClientModel ClientGetV1(string clientValue)
        {
            var response = _repo.Client_GetV1(_jwt.ResourceOwnerV2.RawData, clientValue).Result;

            return response.Content.ReadAsJsonAsync<ClientModel>().Result;
        }

        public ClientModel ClientUpdateV1(ClientModel model)
        {
            var response = _repo.Client_UpdateV1(_jwt.ResourceOwnerV2.RawData, model).Result;

            return response.Content.ReadAsJsonAsync<ClientModel>().Result;
        }

        public IssuerModel IssuerCreateV1(IssuerCreate model)
        {
            var response = _repo.Issuer_CreateV1(_jwt.ResourceOwnerV2.RawData, model).Result;

            return response.Content.ReadAsJsonAsync<IssuerModel>().Result;
        }

        public void IssuerDeleteV1(Guid issuerID)
        {
            var response = _repo.Issuer_DeleteV1(_jwt.ResourceOwnerV2.RawData, issuerID).Result;
        }

        public IEnumerable<ClientModel> IssuerGetClientsV1(string issuerValue)
        {
            var response = _repo.Issuer_GetClientsV1(_jwt.ResourceOwnerV2.RawData, issuerValue).Result;

            return response.Content.ReadAsJsonAsync<IEnumerable<ClientModel>>().Result;
        }

        public Tuple<int, IEnumerable<IssuerModel>> IssuerGetV1(CascadePager model)
        {
            var response = _repo.Issuer_GetV1(_jwt.ResourceOwnerV2.RawData, model).Result;

            var ok = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<IssuerModel>>();
            var count = (int)ok["count"];

            return new Tuple<int, IEnumerable<IssuerModel>>(count, list);
        }

        public IssuerModel IssuerGetV1(string issuerValue)
        {
            var response = _repo.Issuer_GetV1(_jwt.ResourceOwnerV2.RawData, issuerValue).Result;

            return response.Content.ReadAsJsonAsync<IssuerModel>().Result;
        }

        public IssuerModel IssuerUpdateV1(IssuerModel model)
        {
            var response = _repo.Issuer_UpdateV1(_jwt.ResourceOwnerV2.RawData, model).Result;

            return response.Content.ReadAsJsonAsync<IssuerModel>().Result;
        }

        public LoginModel LoginCreateV1(LoginCreate model)
        {
            var response = _repo.Login_CreateV1(_jwt.ResourceOwnerV2.RawData, model).Result;

            return response.Content.ReadAsJsonAsync<LoginModel>().Result;
        }

        public void LoginDeleteV1(Guid loginID)
        {
            var response = _repo.Login_DeleteV1(_jwt.ResourceOwnerV2.RawData, loginID).Result;
        }

        public Tuple<int, IEnumerable<LoginModel>> LoginGetV1(CascadePager model)
        {
            var response = _repo.Login_GetV1(_jwt.ResourceOwnerV2.RawData, model).Result;

            var ok = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<LoginModel>>();
            var count = (int)ok["count"];

            return new Tuple<int, IEnumerable<LoginModel>>(count, list);
        }

        public LoginModel LoginGetV1(string loginValue)
        {
            var response = _repo.Login_GetV1(_jwt.ResourceOwnerV2.RawData, loginValue).Result;

            return response.Content.ReadAsJsonAsync<LoginModel>().Result;
        }

        public LoginModel LoginUpdateV1(LoginModel model)
        {
            var response = _repo.Login_UpdateV1(_jwt.ResourceOwnerV2.RawData, model).Result;

            return response.Content.ReadAsJsonAsync<LoginModel>().Result;
        }

        public void RoleAddToUserV1(Guid roleID, Guid userID)
        {
            var response = _repo.Role_AddToUserV1(_jwt.ResourceOwnerV2.RawData, roleID, userID).Result;
        }

        public RoleModel RoleCreateV1(RoleCreate model)
        {
            var response = _repo.Role_CreateV1(_jwt.ResourceOwnerV2.RawData, model).Result;

            return response.Content.ReadAsJsonAsync<RoleModel>().Result;
        }

        public void RoleDeleteV1(Guid roleID)
        {
            var response = _repo.Role_DeleteV1(_jwt.ResourceOwnerV2.RawData, roleID).Result;
        }

        public Tuple<int, IEnumerable<RoleModel>> RoleGetV1(CascadePager model)
        {
            var response = _repo.Role_GetV1(_jwt.ResourceOwnerV2.RawData, model).Result;

            var ok = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<RoleModel>>();
            var count = (int)ok["count"];

            return new Tuple<int, IEnumerable<RoleModel>>(count, list);
        }

        public RoleModel RoleGetV1(string roleValue)
        {
            var response = _repo.Role_GetV1(_jwt.ResourceOwnerV2.RawData, roleValue).Result;

            return response.Content.ReadAsJsonAsync<RoleModel>().Result;
        }

        public void RoleRemoveFromUserV1(Guid roleID, Guid userID)
        {
            var response = _repo.Role_RemoveFromUserV1(_jwt.ResourceOwnerV2.RawData, roleID, userID).Result;
        }

        public RoleModel RoleUpdateV1(RoleModel model)
        {
            var response = _repo.Role_UpdateV1(_jwt.ResourceOwnerV2.RawData, model).Result;

            return response.Content.ReadAsJsonAsync<RoleModel>().Result;
        }

        public void UserAddToLoginV1(Guid roleID, Guid userID)
        {
            var response = _repo.User_AddToLoginV1(_jwt.ResourceOwnerV2.RawData, roleID, userID).Result;
        }

        public void UserAddToPasswordV1(Guid userID, UserAddPassword model)
        {
            var response = _repo.User_AddPasswordV1(_jwt.ResourceOwnerV2.RawData, userID, model).Result;
        }

        public UserModel UserCreateV1(UserCreate model)
        {
            var response = _repo.User_CreateV1(_jwt.ResourceOwnerV2.RawData, model).Result;

            return response.Content.ReadAsJsonAsync<UserModel>().Result;
        }

        public UserModel UserCreateV1NoConfirm(UserCreate model)
        {
            var response = _repo.User_CreateV1NoConfirm(_jwt.ResourceOwnerV2.RawData, model).Result;

            return response.Content.ReadAsJsonAsync<UserModel>().Result;
        }

        public void UserDeleteV1(Guid userID)
        {
            var response = _repo.User_DeleteV1(_jwt.ResourceOwnerV2.RawData, userID).Result;
        }

        public IEnumerable<ClientModel> UserGetClientsV1(string userValue)
        {
            var response = _repo.User_GetClientsV1(_jwt.ResourceOwnerV2.RawData, userValue).Result;

            return response.Content.ReadAsJsonAsync<IEnumerable<ClientModel>>().Result;
        }

        public IEnumerable<LoginModel> UserGetLoginsV1(string userValue)
        {
            var response = _repo.User_GetLoginsV1(_jwt.ResourceOwnerV2.RawData, userValue).Result;

            return response.Content.ReadAsJsonAsync<IEnumerable<LoginModel>>().Result;
        }

        public Tuple<int, IEnumerable<UserModel>> UserGetV1(CascadePager model)
        {
            var response = _repo.User_GetV1(_jwt.ResourceOwnerV2.RawData, model).Result;

            var ok = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<UserModel>>();
            var count = (int)ok["count"];

            return new Tuple<int, IEnumerable<UserModel>>(count, list);
        }

        public IEnumerable<RoleModel> UserGetRolesV1(string userValue)
        {
            var response = _repo.User_GetRolesV1(_jwt.ResourceOwnerV2.RawData, userValue).Result;

            return response.Content.ReadAsJsonAsync<IEnumerable<RoleModel>>().Result;
        }

        public UserModel UserGetV1(string userValue)
        {
            var response = _repo.User_GetV1(_jwt.ResourceOwnerV2.RawData, userValue).Result;

            return response.Content.ReadAsJsonAsync<UserModel>().Result;
        }

        public void UserRemoveFromLoginV1(Guid roleID, Guid userID)
        {
            var response = _repo.User_RemoveFromLoginV1(_jwt.ResourceOwnerV2.RawData, roleID, userID).Result;
        }

        public void UserRemovePasswordV1(Guid userID)
        {
            var response = _repo.User_RemovePasswordV1(_jwt.ResourceOwnerV2.RawData, userID).Result;
        }

        public void UserSetPasswordV1(Guid userID, UserAddPassword model)
        {
            var response = _repo.User_SetPasswordV1(_jwt.ResourceOwnerV2.RawData, userID, model).Result;
        }

        public UserModel UserUpdateV1(UserModel model)
        {
            var response = _repo.User_UpdateV1(_jwt.ResourceOwnerV2.RawData, model).Result;

            return response.Content.ReadAsJsonAsync<UserModel>().Result;
        }
    }
}
