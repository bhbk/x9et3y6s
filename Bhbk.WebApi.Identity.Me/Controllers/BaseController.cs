using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;

namespace Bhbk.WebApi.Identity.Me.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class BaseController : Controller
    {
        private readonly IConfigurationRoot _conf;
        private readonly IIdentityContext _ioc;
        private readonly IHostedService[] _tasks;
        private readonly string _client, _audience, _user, _pass;
        private JwtSecurityToken _access, _refresh;
        protected readonly S2SClients Connect;

        protected IConfigurationRoot Conf
        {
            get
            {
                return _conf ?? (IConfigurationRoot)ControllerContext.HttpContext.RequestServices.GetRequiredService<IConfigurationRoot>();
            }
        }

        protected IIdentityContext IoC
        {
            get
            {
                return _ioc ?? (IIdentityContext)ControllerContext.HttpContext.RequestServices.GetRequiredService<IIdentityContext>();
            }
        }

        protected IHostedService[] Tasks
        {
            get
            {
                return _tasks ?? (IHostedService[])ControllerContext.HttpContext.RequestServices.GetServices<IHostedService>();
            }
        }

        protected JwtSecurityToken Jwt
        {
            get
            {
                //check if access is valid...
                if (_access != null
                    && _access.ValidFrom < DateTime.UtcNow
                    && _access.ValidTo > DateTime.UtcNow)
                {
                    return _access;
                }
                //check if refresh is valid. update access with refresh if so.
                else if (_refresh != null
                    && _refresh.ValidFrom < DateTime.UtcNow
                    && _refresh.ValidTo > DateTime.UtcNow)
                {
                    var response = Connect.RefreshTokenV2(_client, new List<string> { _audience }, _refresh.RawData).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var json = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                        _access = new JwtSecurityToken((string)json["access_token"]);
                        _refresh = new JwtSecurityToken((string)json["refresh_token"]);
                    }

                    return _access;
                }

                else
                {
                    var response = Connect.AccessTokenV2(_client, new List<string> { _audience }, _user, _pass).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var json = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                        _access = new JwtSecurityToken((string)json["access_token"]);
                        _refresh = new JwtSecurityToken((string)json["refresh_token"]);
                    }

                    return _access;
                }
            }
        }

        public BaseController() { }

        public BaseController(IConfigurationRoot conf, IIdentityContext ioc, IHostedService[] tasks)
        {
            if (conf == null || ioc == null || tasks == null)
                throw new ArgumentNullException();

            _conf = conf;
            _ioc = ioc;
            _tasks = tasks;

            _client = _conf["IdentityLogin:ClientName"];
            _audience = _conf["IdentityLogin:AudienceName"];
            _user = _conf["IdentityLogin:UserName"];
            _pass = _conf["IdentityLogin:Password"];

            Connect = new S2SClients(conf, ioc, new HttpClientHandler());
        }

        [NonAction]
        protected IActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (IdentityError error in result.Errors)
                        ModelState.AddModelError(error.Code, error.Description);
                }

                if (ModelState.IsValid)
                    return BadRequest();

                return BadRequest(ModelState);
            }

            return null;
        }

        [NonAction]
        protected Guid GetUserGUID()
        {
            var claims = ControllerContext.HttpContext.User.Identity as ClaimsIdentity;
            return Guid.Parse(claims.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
        }

        [NonAction]
        public void SetUser(Guid userID)
        {
            var user = IoC.UserMgmt.Store.FindByIdAsync(userID.ToString()).Result;
            var identity = IoC.UserMgmt.ClaimProvider.CreateAsync(user).Result;

            ControllerContext.HttpContext = new DefaultHttpContext();
            ControllerContext.HttpContext.User = identity;
        }
    }
}
