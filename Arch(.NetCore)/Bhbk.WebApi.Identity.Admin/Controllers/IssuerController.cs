using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.DataState.Extensions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Models_TBL;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Exceptions;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("issuer")]
    public class IssuerController : BaseController
    {
        [Route("v1"), HttpPost]
        [Authorize(Policy = Constants.DefaultPolicyForHumans)]
        [Authorize(Roles = Constants.DefaultRoleForAdmin_Identity)]
        public IActionResult CreateV1([FromBody] IssuerV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (UoW.Issuers.Get(x => x.Name == model.Name).Any())
            {
                ModelState.AddModelError(MessageType.IssuerAlreadyExists.ToString(), $"Issuer:{model.Name}");
                return BadRequest(ModelState);
            }

            var issuer = UoW.Issuers.Create(Mapper.Map<tbl_Issuer>(model));

            UoW.Settings.Create(
                Mapper.Map<tbl_Setting>(new SettingV1()
                {
                    IssuerId = issuer.Id,
                    ConfigKey = Constants.SettingAccessExpire,
                    ConfigValue = 600.ToString(),
                    IsDeletable = true,
                }));

            UoW.Settings.Create(
                Mapper.Map<tbl_Setting>(new SettingV1()
                {
                    IssuerId = issuer.Id,
                    ConfigKey = Constants.SettingRefreshExpire,
                    ConfigValue = 86400.ToString(),
                    IsDeletable = true,
                }));

            UoW.Settings.Create(
                Mapper.Map<tbl_Setting>(new SettingV1()
                {
                    IssuerId = issuer.Id,
                    ConfigKey = Constants.SettingTotpExpire,
                    ConfigValue = 600.ToString(),
                    IsDeletable = true,
                }));

            UoW.Settings.Create(
                Mapper.Map<tbl_Setting>(new SettingV1()
                {
                    IssuerId = issuer.Id,
                    ConfigKey = Constants.SettingPollingMax,
                    ConfigValue = 10.ToString(),
                    IsDeletable = true,
                }));

            UoW.Commit();

            return Ok(Mapper.Map<IssuerV1>(issuer));
        }

        [Route("v1/{issuerID:guid}"), HttpDelete]
        [Authorize(Policy = Constants.DefaultPolicyForHumans)]
        [Authorize(Roles = Constants.DefaultRoleForAdmin_Identity)]
        public IActionResult DeleteV1([FromRoute] Guid issuerID)
        {
            var issuer = UoW.Issuers.Get(x => x.Id == issuerID)
                .SingleOrDefault();

            if (issuer == null)
            {
                ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{issuerID}");
                return NotFound(ModelState);
            }
            
            if (!issuer.IsDeletable)
            {
                ModelState.AddModelError(MessageType.IssuerImmutable.ToString(), $"Issuer:{issuerID}");
                return BadRequest(ModelState);
            }

            var claims = UoW.Claims.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                .Where(x => x.IssuerId == issuer.Id).ToLambda());

            var refreshes = UoW.Refreshes.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                .Where(x => x.IssuerId == issuer.Id).ToLambda());

            var settings = UoW.Settings.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Setting>()
                .Where(x => x.IssuerId == issuer.Id).ToLambda());

            var states = UoW.States.Delete(QueryExpressionFactory.GetQueryExpression<tbl_State>()
                .Where(x => x.IssuerId == issuer.Id).ToLambda());

            var roles = UoW.Roles.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Role>()
                .Where(x => x.Audience.IssuerId == issuer.Id).ToLambda());

            var audiences = UoW.Audiences.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                .Where(x => x.IssuerId == issuer.Id).ToLambda());

            UoW.Issuers.Delete(issuer);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/keys"), HttpPost]
        [Authorize(Policy = Constants.DefaultPolicyForServices)]
        public IActionResult GetKeysV1([FromBody] List<string> model)
        {
            var current = GetIdentityGUID();
            var audience = UoW.Audiences.Get(x => x.Id == current).SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{current}");
                return NotFound(ModelState);
            }
            
            if (audience.IsLockedOut)
            {
                ModelState.AddModelError(MessageType.AudienceInvalid.ToString(), $"Audience:{current}");
                return BadRequest(ModelState);
            }

            var issuers = UoW.Issuers.Get(QueryExpressionFactory.GetQueryExpression<tbl_Issuer>()
                .Where(x => x.IsEnabled == true && (model.Contains(x.Id.ToString()) || model.Contains(x.Name))).ToLambda());

            return Ok(issuers.ToDictionary(x => x.Id, x => x.IssuerKey));
        }

        [Route("v1/{issuerValue}"), HttpGet]
        public IActionResult GetV1([FromRoute] string issuerValue)
        {
            Guid issuerID;
            tbl_Issuer issuer = null;

            if (Guid.TryParse(issuerValue, out issuerID))
                issuer = UoW.Issuers.Get(x => x.Id == issuerID)
                    .SingleOrDefault();
            else
                issuer = UoW.Issuers.Get(x => x.Name == issuerValue)
                    .SingleOrDefault();

            if (issuer == null)
            {
                ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{issuerValue}");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<IssuerV1>(issuer));
        }

        [Route("v1/page"), HttpPost]
        public IActionResult GetV1([FromBody] DataStateV1 state)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new DataStateV1Result<IssuerV1>
                {
                    Data = Mapper.Map<IEnumerable<IssuerV1>>(
                        UoW.Issuers.Get(
                            Mapper.MapExpression<Expression<Func<IQueryable<tbl_Issuer>, IQueryable<tbl_Issuer>>>>(
                                QueryExpressionFactory.GetQueryExpression<tbl_Issuer>().ApplyState(state)),
                            new List<Expression<Func<tbl_Issuer, object>>>() { x => x.tbl_Audiences })),

                    Total = UoW.Issuers.Count(
                        Mapper.MapExpression<Expression<Func<IQueryable<tbl_Issuer>, IQueryable<tbl_Issuer>>>>(
                            QueryExpressionFactory.GetQueryExpression<tbl_Issuer>().ApplyPredicate(state)))
                };

                return Ok(result);
            }
            catch (QueryExpressionException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());
                return BadRequest(ModelState);
            }
        }

        [Route("v1/{issuerID:guid}/audiences"), HttpGet]
        public IActionResult GetAudiencesV1([FromRoute] Guid issuerID)
        {
            var issuer = UoW.Issuers.Get(x => x.Id == issuerID)
                .SingleOrDefault();

            if (issuer == null)
            {
                ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{issuerID}");
                return NotFound(ModelState);
            }

            var audiences = UoW.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                .Where(x => x.IssuerId == issuerID).ToLambda());

            return Ok(Mapper.Map<AudienceV1>(audiences));
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = Constants.DefaultPolicyForHumans)]
        [Authorize(Roles = Constants.DefaultRoleForAdmin_Identity)]
        public IActionResult UpdateV1([FromBody] IssuerV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var issuer = UoW.Issuers.GetAsNoTracking(QueryExpressionFactory.GetQueryExpression<tbl_Issuer>()
                .Where(x => x.Id == model.Id).ToLambda())
                .SingleOrDefault();

            if (issuer == null)
            {
                ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{model.Id}");
                return NotFound(ModelState);
            }
            
            if (issuer.IsDeletable
                && issuer.IsDeletable != model.IsDeletable)
            {
                ModelState.AddModelError(MessageType.IssuerImmutable.ToString(), $"Issuer:{issuer.Id}");
                return BadRequest(ModelState);
            }

            var result = UoW.Issuers.Update(Mapper.Map<tbl_Issuer>(model));

            UoW.Commit();

            return Ok(Mapper.Map<IssuerV1>(result));
        }
    }
}