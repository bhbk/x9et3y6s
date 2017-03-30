using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controller
{
    [RoutePrefix("client")]
    public class ClientController : BaseController
    {
        public ClientController() { }

        public ClientController(IUnitOfWork uow)
            : base(uow) { }

        [Route("v1"), HttpGet]
        public IHttpActionResult GetClients()
        {
            return Ok(UoW.ClientRepository.Get().Select(x => ModelFactory.Create(x)));
        }

        [Route("v1/{clientID}"), HttpGet]
        public async Task<IHttpActionResult> GetClient(Guid clientID)
        {
            var foundClient = await UoW.ClientRepository.FindAsync(clientID);

            if (foundClient == null)
                return BadRequest(BaseLib.Statics.MsgClientNotExist);

            else
                return Ok(ModelFactory.Create(foundClient));
        }

        [Route("v1/{clientID}/audiences"), HttpGet]
        public async Task<IHttpActionResult> GetAudiencesInClient(Guid clientID)
        {
            var foundClient = await UoW.ClientRepository.FindAsync(clientID);

            if (foundClient == null)
                return BadRequest(BaseLib.Statics.MsgClientNotExist);

            else
                return Ok(UoW.AudienceRepository.Get(x => x.ClientId == foundClient.Id).Select(x => ModelFactory.Create(x)));
        }

        [Route("v1"), HttpPost]
        //[Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> CreateClient(ClientModel.Binding.Create model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var foundClient = UoW.ClientRepository.Get(x => x.Name == model.Name).SingleOrDefault();

            if (foundClient == null)
            {
                var client = new AppClient()
                {
                    Id = Guid.NewGuid(),
                    Description = model.Description,
                    Name = model.Name,
                    Enabled = model.Enabled,
                    Immutable = false
                };

                UoW.ClientRepository.Create(client);
                await UoW.SaveAsync();

                return Ok(ModelFactory.Create(client));
            }
            else
                return BadRequest(BaseLib.Statics.MsgClientAlreadyExists);
        }

        [Route("v1/{clientID}"), HttpPut]
        //[Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> UpdateClient(Guid clientID, ClientModel.Binding.Update model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            else if (clientID != model.Id)
                return BadRequest(BaseLib.Statics.MsgClientInvalid);

            var foundClient = await UoW.ClientRepository.FindAsync(clientID);

            if (foundClient == null)
                return BadRequest(BaseLib.Statics.MsgClientNotExist);

            else if (foundClient.Immutable)
                return BadRequest(BaseLib.Statics.MsgClientImmutable);

            else
            {
                foundClient.Name = model.Name;
                foundClient.Description = model.Description;
                foundClient.Enabled = model.Enabled;
                foundClient.Immutable = false;

                UoW.ClientRepository.Update(foundClient);
                await UoW.SaveAsync();

                return Ok(ModelFactory.Create(foundClient));
            }
        }

        [Route("v1/{clientID}"), HttpDelete]
        //[Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> DeleteClient(Guid clientID)
        {
            var foundClient = await UoW.ClientRepository.FindAsync(clientID);

            if (foundClient == null)
                return BadRequest(BaseLib.Statics.MsgClientNotExist);

            else if (foundClient.Immutable)
                return BadRequest(BaseLib.Statics.MsgClientImmutable);

            else
            {
                UoW.ClientRepository.Delete(foundClient);
                await UoW.SaveAsync();

                return Ok();
            }
        }
    }
}