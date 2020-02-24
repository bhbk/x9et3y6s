﻿using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.EFCore.Models;
using Bhbk.Lib.Identity.Domain.Providers.Admin;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives;
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
    [Route("issuer")]
    public class IssuerController : BaseController
    {
        private IssuerProvider _provider;

        public IssuerController(IConfiguration conf, IContextService instance)
        {
            _provider = new IssuerProvider(conf, instance);
        }

        [Route("v1"), HttpPost]
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult CreateV1([FromBody] IssuerCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (UoW.Issuers.Get(x => x.Name == model.Name).Any())
            {
                ModelState.AddModelError(MessageType.IssuerAlreadyExists.ToString(), $"Issuer:{model.Name}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetIdentityGUID();

            var result = UoW.Issuers.Create(Mapper.Map<tbl_Issuers>(model));

            UoW.Commit();

            return Ok(Mapper.Map<IssuerModel>(result));
        }

        [Route("v1/{issuerID:guid}"), HttpDelete]
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult DeleteV1([FromRoute] Guid issuerID)
        {
            var issuer = UoW.Issuers.Get(x => x.Id == issuerID)
                .SingleOrDefault();

            if (issuer == null)
            {
                ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{issuerID}");
                return NotFound(ModelState);
            }
            else if (issuer.Immutable)
            {
                ModelState.AddModelError(MessageType.IssuerImmutable.ToString(), $"Issuer:{issuerID}");
                return BadRequest(ModelState);
            }

            issuer.ActorId = GetIdentityGUID();

            UoW.Issuers.Delete(issuer);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/keys"), HttpPost]
        [Authorize(Policy = Constants.PolicyForServices)]
        public IActionResult GetKeysV1([FromBody] List<string> model)
        {
            var current = GetIdentityGUID();
            var audience = UoW.Audiences.Get(x => x.Id == current).SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{current}");
                return NotFound(ModelState);
            }
            else if (!audience.Enabled)
            {
                ModelState.AddModelError(MessageType.AudienceInvalid.ToString(), $"Audience:{current}");
                return BadRequest(ModelState);
            }

            var issuers = UoW.Issuers.Get(new QueryExpression<tbl_Issuers>()
                .Where(x => x.Enabled == true && (model.Contains(x.Id.ToString()) || model.Contains(x.Name))).ToLambda());

            return Ok(issuers.ToDictionary(x => x.Id, x => x.IssuerKey));
        }

        [Route("v1/{issuerValue}"), HttpGet]
        [Authorize(Policy = Constants.PolicyForUsers)]
        public IActionResult GetV1([FromRoute] string issuerValue)
        {
            Guid issuerID;
            tbl_Issuers issuer = null;

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

            return Ok(Mapper.Map<IssuerModel>(issuer));
        }

        [Route("v1/page"), HttpPost]
        [Authorize(Policy = Constants.PolicyForUsers)]
        public IActionResult GetV1([FromBody] PageStateTypeC model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new PageStateTypeCResult<IssuerModel>
                {
                    Data = Mapper.Map<IEnumerable<IssuerModel>>(
                        UoW.Issuers.Get(
                            Mapper.MapExpression<Expression<Func<IQueryable<tbl_Issuers>, IQueryable<tbl_Issuers>>>>(
                                model.ToExpression<tbl_Issuers>()),
                            new List<Expression<Func<tbl_Issuers, object>>>() { x => x.tbl_Audiences })),

                    Total = UoW.Issuers.Count(
                        Mapper.MapExpression<Expression<Func<IQueryable<tbl_Issuers>, IQueryable<tbl_Issuers>>>>(
                            model.ToPredicateExpression<tbl_Issuers>()))
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
        [Authorize(Policy = Constants.PolicyForUsers)]
        public IActionResult GetAudiencesV1([FromRoute] Guid issuerID)
        {
            var issuer = UoW.Issuers.Get(x => x.Id == issuerID)
                .SingleOrDefault();

            if (issuer == null)
            {
                ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{issuerID}");
                return NotFound(ModelState);
            }

            var audiences = UoW.Audiences.Get(new QueryExpression<tbl_Audiences>()
                .Where(x => x.IssuerId == issuerID).ToLambda());

            return Ok(Mapper.Map<AudienceModel>(audiences));
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult UpdateV1([FromBody] IssuerModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var issuer = UoW.Issuers.Get(x => x.Id == model.Id)
                .SingleOrDefault();

            if (issuer == null)
            {
                ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{model.Id}");
                return NotFound(ModelState);
            }
            else if (issuer.Immutable
                && issuer.Immutable != model.Immutable)
            {
                ModelState.AddModelError(MessageType.IssuerImmutable.ToString(), $"Issuer:{issuer.Id}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetIdentityGUID();

            var result = UoW.Issuers.Update(Mapper.Map<tbl_Issuers>(model));

            UoW.Commit();

            return Ok(Mapper.Map<IssuerModel>(result));
        }
    }
}