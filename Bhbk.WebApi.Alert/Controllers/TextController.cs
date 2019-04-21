﻿using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Providers.Alert;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.WebApi.Alert.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Alert.Controllers
{
    [ApiController]
    [Route("text")]
    public class TextController : BaseController
    {
        private TextProvider _provider;

        public TextController(IConfiguration conf, IContextService instance)
        {
            _provider = new TextProvider(conf, instance);
        }

        [Route("v1"), HttpPost]
        public async Task<IActionResult> SendTextV1([FromBody] TextCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!(await UoW.UserRepo.GetAsync(x => x.Id == model.FromId
                && x.PhoneNumber == model.FromPhoneNumber)).Any())
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"SenderID:{model.FromId} SenderPhone:{model.FromPhoneNumber}");
                return NotFound(ModelState);
            }

            var queue = (QueueTextTask)Tasks.Single(x => x.GetType() == typeof(QueueTextTask));

            var msg = Mapper.Map<tbl_QueueTexts>(model);

            if (!queue.TryEnqueueText(msg))
            {
                ModelState.AddModelError(MessageType.TextEnqueueError.ToString(), $"MessageID:{msg.Id} SenderPhone:{model.FromPhoneNumber}");
                return BadRequest(ModelState);
            }

            return NoContent();
        }
    }
}
