using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Domain.Providers.Admin;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives.Enums;
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
    public class AudienceController : BaseController
    {
        private AudienceProvider _provider;

        public AudienceController(IConfiguration conf, IContextService instance)
        {
            _provider = new AudienceProvider(conf, instance);
        }

        [Route("v1"), HttpPost]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult CreateV1([FromBody] AudienceCreate model)
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

            return Ok(Mapper.Map<AudienceModel>(result));
        }

        [Route("v1/{audienceID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult DeleteV1([FromRoute] Guid audienceID)
        {
            var audience = UoW.Audiences.Get(x => x.Id == audienceID).SingleOrDefault();

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
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult DeleteRefreshesV1([FromRoute] Guid audienceID)
        {
            var audience = UoW.Audiences.Get(x => x.Id == audienceID).SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{audienceID}");
                return NotFound(ModelState);
            }

            UoW.Refreshes.Delete(new QueryExpression<tbl_Refreshes>()
                .Where(x => x.AudienceId == audienceID).ToLambda());

            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{audienceID:guid}/refresh/{refreshID}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult DeleteRefreshV1([FromRoute] Guid audienceID, [FromRoute] Guid refreshID)
        {
            var expr = new QueryExpression<tbl_Refreshes>()
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
                audience = UoW.Audiences.Get(x => x.Id == audienceID).SingleOrDefault();
            else
                audience = UoW.Audiences.Get(x => x.Name == audienceValue).SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{audienceValue}");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<AudienceModel>(audience));
        }

        [Route("v1/page"), HttpPost]
        public IActionResult GetV1([FromBody] PageStateTypeC model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new PageStateTypeCResult<AudienceModel>
                {
                    Data = Mapper.Map<IEnumerable<AudienceModel>>(
                        UoW.Audiences.Get(
                            Mapper.MapExpression<Expression<Func<IQueryable<tbl_Audiences>, IQueryable<tbl_Audiences>>>>(
                                model.ToExpression<tbl_Audiences>()),
                            new List<Expression<Func<tbl_Audiences, object>>>() { x => x.tbl_Roles })),

                    Total = UoW.Audiences.Count(
                        Mapper.MapExpression<Expression<Func<IQueryable<tbl_Audiences>, IQueryable<tbl_Audiences>>>>(
                            model.ToPredicateExpression<tbl_Audiences>()))
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
            var audience = UoW.Audiences.Get(x => x.Id == audienceID).SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{audienceID}");
                return NotFound(ModelState);
            }

            var refreshes = UoW.Refreshes.Get(new QueryExpression<tbl_Refreshes>()
                .Where(x => x.AudienceId == audience.Id).ToLambda());

            return Ok(Mapper.Map<IEnumerable<RefreshModel>>(refreshes));
        }

        [Route("v1/{audienceID:guid}/roles"), HttpGet]
        public IActionResult GetRolesV1([FromRoute] Guid audienceID)
        {
            var audience = UoW.Audiences.Get(x => x.Id == audienceID).SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{audienceID}");
                return NotFound(ModelState);
            }

            var roles = UoW.Roles.Get(new QueryExpression<tbl_Roles>()
                .Where(x => x.AudienceId == audience.Id).ToLambda());

            return Ok(Mapper.Map<IEnumerable<RoleModel>>(roles));
        }

        [Route("v1/{audienceID:guid}/urls"), HttpGet]
        public IActionResult GetUrlsV1([FromRoute] Guid audienceID)
        {
            var audience = UoW.Audiences.Get(x => x.Id == audienceID).SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{audienceID}");
                return NotFound(ModelState);
            }

            var urls = UoW.Urls.Get(new QueryExpression<tbl_Urls>()
                .Where(x => x.AudienceId == audience.Id).ToLambda());

            return Ok(Mapper.Map<IEnumerable<UrlModel>>(urls));
        }

        [Route("v1/{audienceID:guid}/set-password"), HttpPut]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult SetPasswordV1([FromRoute] Guid audienceID, [FromBody] PasswordAdd model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var audience = UoW.Audiences.Get(x => x.Id == audienceID).SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{audienceID}");
                return NotFound(ModelState);
            }

            audience.ActorId = GetIdentityGUID();

            if (model.NewPassword != model.NewPasswordConfirm)
            {
                ModelState.AddModelError(MessageType.AudienceInvalid.ToString(), $"Bad password for audience:{audience.Id}");
                return BadRequest(ModelState);
            }

            UoW.Audiences.SetPassword(audience, model.NewPassword);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult UpdateV1([FromBody] AudienceModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var audience = UoW.Audiences.Get(x => x.Id == model.Id).SingleOrDefault();

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

            return Ok(Mapper.Map<AudienceModel>(result));
        }
    }
}