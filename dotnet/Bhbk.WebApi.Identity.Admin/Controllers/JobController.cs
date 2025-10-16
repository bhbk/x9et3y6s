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
    [Route("jobs")]
    public class JobController : BaseController
    {
        [Route("v1"), HttpGet]
        public IActionResult GetV1()
        {
            var expr = QueryExpressionFactory.GetQueryExpression<tbl_Job>()
                .ToLambda();

            var jobs = uow.Jobs.Get(expr,
                new List<Expression<Func<tbl_Job, object>>>()
                {
                    x => x.tbl_JobSettings,
                })
                .OrderBy(x => x.Name)
                .ToList();

            return Ok(map.Map<IEnumerable<JobV1>>(jobs));
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = PolicyConstants.OAuth2ROPGrants)]
        [Authorize(Policy = PolicyConstants.IdentityAdminPolicy)]
        public IActionResult UpdateV1([FromBody] JobV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var job = uow.Jobs.Get(x => x.Id == model.Id)
                .SingleOrDefault();

            if (job == null)
            {
                ModelState.AddModelError(MessageType.ParametersInvalid.ToString(), $"Job:{model.Id}");
                return NotFound(ModelState);
            }

            job.IsEnabled = model.IsEnabled;
            job.ModifiedUtc = DateTimeOffset.UtcNow;

            uow.Jobs.Put(job);
            uow.Commit();

            return Ok(map.Map<JobV1>(job));
        }

        [Route("v1/{jobId:guid}/settings"), HttpPut]
        [Authorize(Policy = PolicyConstants.OAuth2ROPGrants)]
        [Authorize(Policy = PolicyConstants.IdentityAdminPolicy)]
        public IActionResult UpdateSettingsV1([FromRoute] Guid jobId, [FromBody] List<JobSettingV1> model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var job = uow.Jobs.Get(x => x.Id == jobId)
                .SingleOrDefault();

            if (job == null)
            {
                ModelState.AddModelError(MessageType.ParametersInvalid.ToString(), $"Job:{jobId}");
                return NotFound(ModelState);
            }

            foreach (var item in model)
            {
                /* skip unchanged secret values */
                if (item.ConfigValue == "********")
                    continue;

                var setting = uow.JobSettings.Get(x => x.Id == item.Id && x.JobId == jobId)
                    .SingleOrDefault();

                if (setting == null)
                {
                    ModelState.AddModelError(MessageType.ParametersInvalid.ToString(), $"JobSetting:{item.Id}");
                    return NotFound(ModelState);
                }

                setting.ConfigValue = item.ConfigValue;

                uow.JobSettings.Put(setting);
            }

            job.ModifiedUtc = DateTimeOffset.UtcNow;
            uow.Jobs.Put(job);
            uow.Commit();

            return NoContent();
        }
    }
}
