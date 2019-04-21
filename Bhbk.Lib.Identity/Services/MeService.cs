using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;

namespace Bhbk.Lib.Identity.Services
{
    public class MeService : IMeService
    {
        private readonly JwtHelper _jwt;
        private readonly MeRepository _repo;

        public MeService(IConfigurationRoot conf, InstanceContext instance, HttpClient client)
        {
            _jwt = new JwtHelper(conf, instance, client);
            _repo = new MeRepository(conf, instance, client);
        }

        public JwtSecurityToken Jwt
        {
            get { return _jwt.ResourceOwnerV2; }
            set { _jwt.ResourceOwnerV2 = value; }
        }

        public MeRepository Repo
        {
            get { return _repo; }
        }

        public UserModel DetailGetV1(string userValue)
        {
            throw new NotImplementedException();
        }
    }
}
