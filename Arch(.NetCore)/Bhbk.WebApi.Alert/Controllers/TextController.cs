using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Domain.Providers.Alert;
using Bhbk.Lib.Identity.Models.Alert;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace Bhbk.WebApi.Alert.Controllers
{
    [Route("text")]
    public class TextController : BaseController
    {
        private TextProvider _provider;

        public TextController(IConfiguration conf, IContextService instance)
        {
            _provider = new TextProvider(conf, instance);
        }

        [Route("v1"), HttpPost]
        public IActionResult SendTextV1([FromBody] TextV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = null;
            model.FromId = null;

            var text = Mapper.Map<tbl_TextQueue>(model);

            UoW.TextQueue.Create(text);
            UoW.Commit();

            return NoContent();
        }
    }
}
