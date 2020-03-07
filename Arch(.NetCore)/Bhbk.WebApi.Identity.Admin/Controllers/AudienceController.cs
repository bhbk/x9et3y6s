﻿using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataState.Extensions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Bhbk.Lib.Identity.Domain.Providers.Admin;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Exceptions;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("audience")]
    [Authorize(Policy = Constants.PolicyForUsers)]
    public class AudienceController : BaseController
    {
        private AudienceProvider _provider;

        public AudienceController(IConfiguration conf, IContextService instance)
        {
            _provider = new AudienceProvider(conf, instance);
        }

        [Route("v1"), HttpPost]
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult CreateV1([FromBody] AudienceV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (UoW.Audiences.Get(x => x.IssuerId == model.IssuerId
                && x.Name == model.Name).Any())
            {
                ModelState.AddModelError(MessageType.AudienceAlreadyExists.ToString(), $"Issuer:{model.IssuerId} Audience:{model.Name}");
                return BadRequest(ModelState);
            }

            AudienceType audienceType;

            if (!Enum.TryParse<AudienceType>(model.AudienceType, true, out audienceType))
            {
                ModelState.AddModelError(MessageType.AudienceInvalid.ToString(), $"Issuer:{model.IssuerId} Audience:{model.Name}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetIdentityGUID();

            var result = UoW.Audiences.Create(Mapper.Map<tbl_Audiences>(model));

            UoW.Commit();

            return Ok(Mapper.Map<AudienceV1>(result));
        }

        [Route("v1/{audienceID:guid}"), HttpDelete]
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult DeleteV1([FromRoute] Guid audienceID)
        {
            var audience = UoW.Audiences.Get(x => x.Id == audienceID)
                .SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{audienceID}");
                return NotFound(ModelState);
            }
            else if (audience.Immutable)
            {
                ModelState.AddModelError(MessageType.AudienceImmutable.ToString(), $"Audience:{audienceID}");
                return BadRequest(ModelState);
            }

            audience.ActorId = GetIdentityGUID();

            UoW.Audiences.Delete(audience);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{audienceID:guid}/refresh"), HttpDelete]
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult DeleteRefreshesV1([FromRoute] Guid audienceID)
        {
            var audience = UoW.Audiences.Get(x => x.Id == audienceID)
                .SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{audienceID}");
                return NotFound(ModelState);
            }

            UoW.Refreshes.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Refreshes>()
                .Where(x => x.AudienceId == audienceID).ToLambda());

            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{audienceID:guid}/refresh/{refreshID}"), HttpDelete]
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult DeleteRefreshV1([FromRoute] Guid audienceID, [FromRoute] Guid refreshID)
        {
            var expr = QueryExpressionFactory.GetQueryExpression<tbl_Refreshes>()
                .Where(x => x.AudienceId == audienceID && x.Id == refreshID).ToLambda();

            if (!UoW.Refreshes.Exists(expr))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{refreshID}");
                return NotFound(ModelState);
            }

            UoW.Refreshes.Delete(expr);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{audienceValue}"), HttpGet]
        public IActionResult GetV1([FromRoute] string audienceValue)
        {
            Guid audienceID;
            tbl_Audiences audience = null;

            if (Guid.TryParse(audienceValue, out audienceID))
                audience = UoW.Audiences.Get(x => x.Id == audienceID)
                    .SingleOrDefault();
            else
                audience = UoW.Audiences.Get(x => x.Name == audienceValue)
                    .SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{audienceValue}");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<AudienceV1>(audience));
        }

        [Route("v1/page"), HttpPost]
        public IActionResult GetV1([FromBody] DataStateV1 state)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new DataStateV1Result<AudienceV1>
                {
                    Data = Mapper.Map<IEnumerable<AudienceV1>>(
                        UoW.Audiences.Get(
                            Mapper.MapExpression<Expression<Func<IQueryable<tbl_Audiences>, IQueryable<tbl_Audiences>>>>(
                                QueryExpressionFactory.GetQueryExpression<tbl_Audiences>().ApplyState(state)),
                            new List<Expression<Func<tbl_Audiences, object>>>() { x => x.tbl_Roles })),

                    Total = UoW.Audiences.Count(
                        Mapper.MapExpression<Expression<Func<IQueryable<tbl_Audiences>, IQueryable<tbl_Audiences>>>>(
                            QueryExpressionFactory.GetQueryExpression<tbl_Audiences>().ApplyPredicate(state)))
                };

                return Ok(result);
            }
            catch (QueryExpressionException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());
                return BadRequest(ModelState);
            }
        }

        [Route("v1/{audienceID:guid}/refreshes"), HttpGet]
        public IActionResult GetRefreshesV1([FromRoute] Guid audienceID)
        {
            var audience = UoW.Audiences.Get(x => x.Id == audienceID)
                .SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{audienceID}");
                return NotFound(ModelState);
            }

            var refreshes = UoW.Refreshes.Get(QueryExpressionFactory.GetQueryExpression<tbl_Refreshes>()
                .Where(x => x.AudienceId == audience.Id).ToLambda());

            return Ok(Mapper.Map<IEnumerable<RefreshV1>>(refreshes));
        }

        [Route("v1/{audienceID:guid}/roles"), HttpGet]
        public IActionResult GetRolesV1([FromRoute] Guid audienceID)
        {
            var audience = UoW.Audiences.Get(x => x.Id == audienceID)
                .SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{audienceID}");
                return NotFound(ModelState);
            }

            var roles = UoW.Roles.Get(QueryExpressionFactory.GetQueryExpression<tbl_Roles>()
                .Where(x => x.AudienceId == audience.Id).ToLambda());

            return Ok(Mapper.Map<IEnumerable<RoleV1>>(roles));
        }

        [Route("v1/{audienceID:guid}/urls"), HttpGet]
        public IActionResult GetUrlsV1([FromRoute] Guid audienceID)
        {
            var audience = UoW.Audiences.Get(x => x.Id == audienceID)
                .SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{audienceID}");
                return NotFound(ModelState);
            }

            var urls = UoW.Urls.Get(QueryExpressionFactory.GetQueryExpression<tbl_Urls>()
                .Where(x => x.AudienceId == audience.Id).ToLambda());

            return Ok(Mapper.Map<IEnumerable<UrlV1>>(urls));
        }

        [Route("v1/{audienceID:guid}/set-password"), HttpPut]
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult SetPasswordV1([FromRoute] Guid audienceID, [FromBody] PasswordAddV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var audience = UoW.Audiences.Get(x => x.Id == audienceID)
                .SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{audienceID}");
                return NotFound(ModelState);
            }

            audience.ActorId = GetIdentityGUID();

            if (model.NewPassword != model.NewPasswordConfirm
                || !new ValidationHelper().ValidatePassword(model.NewPassword).Succeeded)
            {
                ModelState.AddModelError(MessageType.AudienceInvalid.ToString(), $"Bad password for audience:{audience.Id}");
                return BadRequest(ModelState);
            }

            UoW.Audiences.SetPasswordHash(audience, model.NewPassword);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult UpdateV1([FromBody] AudienceV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var audience = UoW.Audiences.Get(x => x.Id == model.Id)
                .SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{model.Id}");
                return NotFound(ModelState);
            }
            else if (audience.Immutable
                && audience.Immutable != model.Immutable)
            {
                ModelState.AddModelError(MessageType.AudienceImmutable.ToString(), $"Audience:{audience.Id}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetIdentityGUID();

            var result = UoW.Audiences.Update(Mapper.Map<tbl_Audiences>(model));

            UoW.Commit();

            return Ok(Mapper.Map<AudienceV1>(result));
        }
    }
}