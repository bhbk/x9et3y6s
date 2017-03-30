using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controller
{
    [RoutePrefix("realm")]
    public class RealmController : BaseController
    {
        public RealmController() { }

        public RealmController(IUnitOfWork uow)
            : base(uow) { }

        [Route("v1"), HttpGet]
        public IHttpActionResult GetRealms()
        {
            return Ok(UoW.RealmRepository.Get().Select(x => ModelFactory.Create(x)));
        }

        [Route("v1/{realmID}"), HttpGet]
        public async Task<IHttpActionResult> GetRealm(Guid realmID)
        {
            var foundRealm = await UoW.RealmRepository.FindAsync(realmID);

            if (foundRealm == null)
                return BadRequest(BaseLib.Statics.MsgRealmNotExist);

            else
                return Ok(ModelFactory.Create(foundRealm));
        }

        [Route("v1/{realmID}/users"), HttpGet]
        public async Task<IHttpActionResult> GetUsersInRealm(Guid realmID)
        {
            var foundRealm = await UoW.RealmRepository.FindAsync(realmID);

            if (foundRealm == null)
                return BadRequest(BaseLib.Statics.MsgRealmNotExist);

            else
                return Ok(UoW.UserRepository.Get(x => x.RealmId == foundRealm.Id).Select(x => ModelFactory.Create(x)));
        }

        [Route("v1"), HttpPost]
        //[Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> CreateRealm(RealmModel.Binding.Create model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var foundRealm = UoW.RealmRepository.Get(x => x.Name == model.Name).SingleOrDefault();

            if (foundRealm == null)
            {
                var realm = new AppRealm()
                {
                    Id = Guid.NewGuid(),
                    Description = model.Description,
                    Name = model.Name,
                    Enabled = model.Enabled,
                    Immutable = false
                };

                UoW.RealmRepository.Create(realm);
                await UoW.SaveAsync();

                return Ok(ModelFactory.Create(realm));
            }
            else
                return BadRequest(BaseLib.Statics.MsgRealmAlreadyExists);
        }

        [Route("v1/{realmID}"), HttpPut]
        //[Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> UpdateRealm(Guid realmID, RealmModel.Binding.Update model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            else if (realmID != model.Id)
                return BadRequest(BaseLib.Statics.MsgRealmInvalid);

            var foundRealm = await UoW.RealmRepository.FindAsync(realmID);

            if (foundRealm == null)
                return BadRequest(BaseLib.Statics.MsgRealmNotExist);

            else if (foundRealm.Immutable)
                return BadRequest(BaseLib.Statics.MsgRealmImmutable);

            else
            {
                foundRealm.Name = model.Name;
                foundRealm.Description = model.Description;
                foundRealm.Enabled = model.Enabled;
                foundRealm.Immutable = false;

                UoW.RealmRepository.Update(foundRealm);
                await UoW.SaveAsync();

                return Ok(ModelFactory.Create(foundRealm));
            }
        }

        [Route("v1/{realmID}"), HttpDelete]
        //[Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> DeleteRealm(Guid realmID)
        {
            var foundRealm = await UoW.RealmRepository.FindAsync(realmID);

            if (foundRealm == null)
                return BadRequest(BaseLib.Statics.MsgRealmNotExist);

            else if (foundRealm.Immutable)
                return BadRequest(BaseLib.Statics.MsgRealmImmutable);

            else
            {
                UoW.RealmRepository.Delete(foundRealm);
                await UoW.SaveAsync();

                return Ok();
            }
        }
    }
}