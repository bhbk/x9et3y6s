using Bhbk.Lib.Core.Extensions;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Repositories;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;

namespace Bhbk.Lib.Identity.Services
{
    public class MeService : IMeService
    {
        private readonly ResourceOwnerHelper _jwt;
        private readonly MeRepository _repo;

        public MeService(IConfigurationRoot conf, InstanceContext instance, HttpClient client)
        {
            _jwt = new ResourceOwnerHelper(conf, instance, client);
            _repo = new MeRepository(conf, instance, client);
        }

        public JwtSecurityToken Jwt
        {
            get { return _jwt.JwtV2; }
            set { _jwt.JwtV2 = value; }
        }

        public MeRepository Repo
        {
            get { return _repo; }
        }

        public UserModel Detail_GetV1()
        {
            var response = _repo.Detail_GetV1(_jwt.JwtV2.RawData).Result;

            return response.Content.ReadAsJsonAsync<UserModel>().Result;
        }
    }
}
