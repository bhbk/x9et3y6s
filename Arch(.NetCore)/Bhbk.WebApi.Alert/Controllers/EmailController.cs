using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Domain.Providers.Alert;
using Bhbk.Lib.Identity.Models.Alert;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace Bhbk.WebApi.Alert.Controllers
{
    [Route("email")]
    public class EmailController : BaseController
    {
        private EmailProvider _provider;

        public EmailController(IConfiguration conf, IContextService instance)
        {
            _provider = new EmailProvider(conf, instance);
        }

        [Route("v1"), HttpPost]
        public IActionResult SendEmailV1([FromBody] EmailV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = null;
            model.FromId = null;

            var email = Mapper.Map<tbl_QueueEmail>(model);

            UoW.QueueEmails.Create(email);
            UoW.Commit();

            return NoContent();
        }
    }
}
