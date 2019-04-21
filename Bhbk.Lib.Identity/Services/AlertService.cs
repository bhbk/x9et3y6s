using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Repositories;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;

namespace Bhbk.Lib.Identity.Services
{
    public class AlertService : IAlertService
    {
        private readonly JwtHelper _jwt;
        private readonly AlertRepository _repo;

        public AlertService(IConfigurationRoot conf, InstanceContext instance, HttpClient client)
        {
            _jwt = new JwtHelper(conf, instance, client);
            _repo = new AlertRepository(conf, instance, client);
        }

        public JwtSecurityToken Jwt
        {
            get { return _jwt.ResourceOwnerV2; }
            set { _jwt.ResourceOwnerV2 = value; }
        }

        public AlertRepository Repo
        {
            get { return _repo; }
        }

        public void EmailEnqueueV1(EmailCreate model)
        {
            var response = _repo.Enqueue_EmailV1(_jwt.ResourceOwnerV2.RawData, model).Result;
        }

        public void TextEnqueueV1(TextCreate model)
        {
            var response = _repo.Enqueue_TextV1(_jwt.ResourceOwnerV2.RawData, model).Result;
        }
    }
}
