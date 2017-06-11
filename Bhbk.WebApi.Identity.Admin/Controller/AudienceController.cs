using Bhbk.Lib.Identity.Helper;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controller
{
    [RoutePrefix("audience")]
    [Authorize(Roles = "(Built-In) Administrators")]
    public class AudienceController : BaseController
    {
        public AudienceController() { }

        public AudienceController(IUnitOfWork uow)
            : base(uow) { }

        [Route("v1"), HttpPost]
        public async Task<IHttpActionResult> CreateAudience(AudienceModel.Binding.Create model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var foundAudience = UoW.AudienceRepository.Get(x => x.Name == model.Name).SingleOrDefault();

            if (foundAudience == null)
            {
                var audience = new AppAudience()
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    AudienceKey = EntrophyHelper.GenerateRandomBase64(32),
                    AudienceType = BaseLib.AudienceType.ThinClient.ToString(),
                    Enabled = model.Enabled,
                    Immutable = false
                };

                UoW.AudienceRepository.Create(audience);
                await UoW.SaveAsync();

                return Ok(ModelFactory.Create(audience));
            }
            else
                return BadRequest(BaseLib.Statics.MsgAudienceAlreadyExists);
        }

        [Route("v1/{audienceID}"), HttpDelete]
        public async Task<IHttpActionResult> DeleteAudience(Guid audienceID)
        {
            var foundAudience = await UoW.AudienceRepository.FindAsync(audienceID);

            if (foundAudience == null)
                return BadRequest(BaseLib.Statics.MsgAudienceNotExist);

            else if (foundAudience.Immutable)
                return BadRequest(BaseLib.Statics.MsgAudienceImmutable);

            else
            {
                UoW.AudienceRepository.Delete(foundAudience);
                await UoW.SaveAsync();

                return Ok();
            }
        }

        [Route("v1/{audienceID}/key"), HttpGet]
        [AllowAnonymous]
        public IHttpActionResult GetAudienceKey(string audienceID)
        {
            var foundAudience = UoW.AudienceRepository.Get(x => x.Id.ToString() == audienceID || x.Name == audienceID).SingleOrDefault();

            if (foundAudience == null)
                return BadRequest(BaseLib.Statics.MsgAudienceNotExist);

            else
                return Ok(foundAudience.AudienceKey);
        }

        [Route("v1/{audienceID}/roles"), HttpGet]
        [Authorize]
        public async Task<IHttpActionResult> GetAudienceRoles(Guid audienceID)
        {
            var foundAudience = await UoW.AudienceRepository.FindAsync(audienceID);

            if (foundAudience == null)
                return BadRequest(BaseLib.Statics.MsgAudienceNotExist);

            else
            {
                UoW.AudienceRepository.LoadCollection(foundAudience, "Roles");

                return Ok(foundAudience.Roles.Select(x => ModelFactory.Create(x)));
            }
        }

        [Route("v1")]
        [Authorize]
        public IHttpActionResult GetAudiences()
        {
            return Ok(UoW.AudienceRepository.Get().Select(x => ModelFactory.Create(x)));
        }

        [Route("v1/{audienceID}"), HttpGet]
        [Authorize]
        public async Task<IHttpActionResult> GetAudience(Guid audienceID)
        {
            var foundAudience = await UoW.AudienceRepository.FindAsync(audienceID);

            if (foundAudience == null)
                return BadRequest(BaseLib.Statics.MsgAudienceNotExist);

            else
                return Ok(ModelFactory.Create(foundAudience));
        }

        [Route("v1/{audienceID}"), HttpPut]
        public async Task<IHttpActionResult> UpdateAudience(Guid audienceID, AudienceModel.Binding.Update model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            else if (audienceID != model.Id)
                return BadRequest(BaseLib.Statics.MsgAudienceNotExist);

            var foundAudience = await UoW.AudienceRepository.FindAsync(audienceID);

            if (foundAudience == null)
                return BadRequest(BaseLib.Statics.MsgAudienceNotExist);

            else if (foundAudience.Immutable)
                return BadRequest(BaseLib.Statics.MsgAudienceImmutable);

            else
            {
                foundAudience.Name = model.Name;
                foundAudience.Description = model.Description;
                foundAudience.Enabled = model.Enabled;
                foundAudience.Immutable = false;

                UoW.AudienceRepository.Update(foundAudience);
                await UoW.SaveAsync();

                return Ok(ModelFactory.Create(foundAudience));
            }
        }
    }
}