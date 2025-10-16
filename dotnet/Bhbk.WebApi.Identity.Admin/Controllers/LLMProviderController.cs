using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Constants;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("llm-providers")]
    public class LLMProviderController : BaseController
    {
        [Route("v1"), HttpGet]
        public IActionResult GetV1()
        {
            var expr = QueryExpressionFactory.GetQueryExpression<tbl_LLMProvider>()
                .ToLambda();

            var providers = uow.LLMProviders.Get(expr,
                new List<Expression<Func<tbl_LLMProvider, object>>>()
                {
                    x => x.tbl_LLMProviderSettings,
                })
                .OrderBy(x => x.FailoverOrder)
                .ToList();

            return Ok(map.Map<IEnumerable<LLMProviderV1>>(providers));
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = PolicyConstants.OAuth2ROPGrants)]
        [Authorize(Policy = PolicyConstants.IdentityAdminPolicy)]
        public IActionResult UpdateV1([FromBody] LLMProviderV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var provider = uow.LLMProviders.Get(x => x.Id == model.Id)
                .SingleOrDefault();

            if (provider == null)
            {
                ModelState.AddModelError(MessageType.ParametersInvalid.ToString(), $"LLMProvider:{model.Id}");
                return NotFound(ModelState);
            }

            provider.Name = model.Name;
            provider.IsEnabled = model.IsEnabled;
            provider.ModifiedUtc = DateTimeOffset.UtcNow;

            uow.LLMProviders.Put(provider);
            uow.Commit();

            return Ok(map.Map<LLMProviderV1>(provider));
        }

        [Route("v1/order"), HttpPut]
        [Authorize(Policy = PolicyConstants.OAuth2ROPGrants)]
        [Authorize(Policy = PolicyConstants.IdentityAdminPolicy)]
        public IActionResult UpdateOrderV1([FromBody] List<LLMProviderOrderV1> model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            foreach (var item in model)
            {
                var provider = uow.LLMProviders.Get(x => x.Id == item.Id)
                    .SingleOrDefault();

                if (provider == null)
                {
                    ModelState.AddModelError(MessageType.ParametersInvalid.ToString(), $"LLMProvider:{item.Id}");
                    return NotFound(ModelState);
                }

                provider.FailoverOrder = item.FailoverOrder;
                provider.ModifiedUtc = DateTimeOffset.UtcNow;

                uow.LLMProviders.Put(provider);
            }

            uow.Commit();

            return NoContent();
        }

        [Route("v1/{providerId:guid}/settings"), HttpPut]
        [Authorize(Policy = PolicyConstants.OAuth2ROPGrants)]
        [Authorize(Policy = PolicyConstants.IdentityAdminPolicy)]
        public IActionResult UpdateSettingsV1([FromRoute] Guid providerId, [FromBody] List<LLMProviderSettingV1> model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var provider = uow.LLMProviders.Get(x => x.Id == providerId)
                .SingleOrDefault();

            if (provider == null)
            {
                ModelState.AddModelError(MessageType.ParametersInvalid.ToString(), $"LLMProvider:{providerId}");
                return NotFound(ModelState);
            }

            foreach (var item in model)
            {
                /* skip unchanged secret values */
                if (item.ConfigValue == "********")
                    continue;

                var setting = uow.LLMProviderSettings.Get(x => x.Id == item.Id && x.ProviderId == providerId)
                    .SingleOrDefault();

                if (setting == null)
                {
                    ModelState.AddModelError(MessageType.ParametersInvalid.ToString(), $"LLMProviderSetting:{item.Id}");
                    return NotFound(ModelState);
                }

                setting.ConfigValue = item.ConfigValue;

                uow.LLMProviderSettings.Put(setting);
            }

            provider.ModifiedUtc = DateTimeOffset.UtcNow;
            uow.LLMProviders.Put(provider);
            uow.Commit();

            return NoContent();
        }
    }
}
