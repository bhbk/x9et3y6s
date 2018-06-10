using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("client")]
    public class ClientController : BaseController
    {
        public ClientController() { }

        public ClientController(IIdentityContext ioc, IHostedService[] tasks)
            : base(ioc, tasks) { }

        [Route("v1"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateClient(ClientCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var client = await IoC.ClientMgmt.FindByNameAsync(model.Name);

            if (client != null)
                return BadRequest(BaseLib.Statics.MsgClientAlreadyExists);

            var create = new ClientFactory<ClientCreate>(model);
            var result = await IoC.ClientMgmt.CreateAsync(create.Devolve());

            return Ok(create.Evolve());
        }

        [Route("v1/{clientID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteClient(Guid clientID)
        {
            var client = await IoC.ClientMgmt.FindByIdAsync(clientID);

            if (client == null)
                return BadRequest(BaseLib.Statics.MsgClientInvalid);

            else if (client.Immutable)
                return BadRequest(BaseLib.Statics.MsgClientImmutable);

            if (!await IoC.ClientMgmt.DeleteAsync(clientID))
                return StatusCode(StatusCodes.Status500InternalServerError);

            else
                return NoContent();
        }

        [Route("v1/{clientID}"), HttpGet]
        public async Task<IActionResult> GetClient(Guid clientID)
        {
            var client = await IoC.ClientMgmt.FindByIdAsync(clientID);

            if (client == null)
                return BadRequest(BaseLib.Statics.MsgClientInvalid);

            var result = new ClientFactory<AppAudience>(client);

            return Ok(result.Evolve());
        }

        [Route("v1"), HttpGet]
        public async Task<IActionResult> GetClients()
        {
            var result = new List<ClientResult>();
            var users = await IoC.ClientMgmt.GetListAsync();

            foreach (AppClient entry in users)
                result.Add(new ClientFactory<AppClient>(entry).Evolve());

            return Ok(result);
        }

        [Route("v1/{clientID}/audiences"), HttpGet]
        public async Task<IActionResult> GetClientAudiences(Guid clientID)
        {
            var client = await IoC.ClientMgmt.FindByIdAsync(clientID);

            if (client == null)
                return BadRequest(BaseLib.Statics.MsgClientInvalid);

            var result = new List<AudienceResult>();
            var audiences = await IoC.ClientMgmt.GetAudiencesAsync(clientID);

            foreach (AppAudience entry in audiences)
                result.Add(new AudienceFactory<AppAudience>(entry).Evolve());

            return Ok(result);
        }

        [Route("v1"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> UpdateClient(ClientUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var client = await IoC.ClientMgmt.FindByIdAsync(model.Id);

            if (client == null)
                return BadRequest(BaseLib.Statics.MsgClientInvalid);

            else if (client.Immutable)
                return BadRequest(BaseLib.Statics.MsgClientImmutable);

            else
            {
                var result = new ClientFactory<AppClient>(client);
                result.Update(model);

                var update = await IoC.ClientMgmt.UpdateAsync(result.Devolve());

                return Ok(result.Evolve());
            }
        }
    }
}