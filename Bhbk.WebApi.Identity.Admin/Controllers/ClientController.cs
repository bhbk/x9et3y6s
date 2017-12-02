using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("client")]
    public class ClientController : BaseController
    {
        public ClientController() { }

        public ClientController(IIdentityContext context)
            : base(context) { }

        [Route("v1"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateClient(ClientCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var client = await Context.ClientMgmt.FindByNameAsync(model.Name);

            if (client == null)
            {
                var create = Context.ClientMgmt.Store.Mf.Create.DoIt(model);
                var result = await Context.ClientMgmt.CreateAsync(create);

                return Ok(result);
            }
            else
                return BadRequest(BaseLib.Statics.MsgClientAlreadyExists);
        }

        [Route("v1/{clientID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteClient(Guid clientID)
        {
            var client = await Context.ClientMgmt.FindByIdAsync(clientID);

            if (client == null)
                return BadRequest(BaseLib.Statics.MsgClientNotExist);

            else if (client.Immutable)
                return BadRequest(BaseLib.Statics.MsgClientImmutable);

            if (!await Context.ClientMgmt.DeleteAsync(clientID))
                return StatusCode(StatusCodes.Status500InternalServerError);

            else
                return Ok();
        }

        [Route("v1/{clientID}"), HttpGet]
        public async Task<IActionResult> GetClient(Guid clientID)
        {
            var client = await Context.ClientMgmt.FindByIdAsync(clientID);

            if (client == null)
                return BadRequest(BaseLib.Statics.MsgClientNotExist);

            else
                return Ok(client);
        }

        [Route("v1"), HttpGet]
        public async Task<IActionResult> GetClients()
        {
            return Ok(await Context.ClientMgmt.GetListAsync());
        }

        [Route("v1/{clientID}/audiences"), HttpGet]
        public async Task<IActionResult> GetClientAudiences(Guid clientID)
        {
            var client = await Context.ClientMgmt.FindByIdAsync(clientID);

            if (client == null)
                return BadRequest(BaseLib.Statics.MsgClientNotExist);

            else
                return Ok(await Context.ClientMgmt.GetAudiencesAsync(clientID));
        }

        [Route("v1/{clientID}"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> UpdateClient(Guid clientID, ClientUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            else if (clientID != model.Id)
                return BadRequest(BaseLib.Statics.MsgClientInvalid);

            var client = await Context.ClientMgmt.FindByIdAsync(clientID);

            if (client == null)
                return BadRequest(BaseLib.Statics.MsgClientNotExist);

            else if (client.Immutable)
                return BadRequest(BaseLib.Statics.MsgClientImmutable);

            else
            {
                var update = Context.ClientMgmt.Store.Mf.Update.DoIt(model);
                var result = await Context.ClientMgmt.UpdateAsync(update);

                return Ok(result);
            }
        }
    }
}