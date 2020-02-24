using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.EFCore.Models;
using Bhbk.Lib.Identity.Domain.Providers.Admin;
using Bhbk.Lib.Identity.Models.Me;
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
    [Route("motd")]
    [Authorize(Policy = Constants.PolicyForUsers)]
    public class MOTDController : BaseController
    {
        private MOTDProvider _provider;

        public MOTDController(IConfiguration conf, IContextService instance)
        {
            _provider = new MOTDProvider(conf, instance);
        }

        [Route("v1"), HttpPost]
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult CreateV1([FromBody] MOTDCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (UoW.MOTDs.Get(x => x.Quote == model.quote).Any())
            {
                ModelState.AddModelError(MessageType.MOTDAlreadyExists.ToString(), $"Author:\"{model.author}\" Quote:\"{model.quote}\"");
                return BadRequest(ModelState);
            }

            var result = UoW.MOTDs.Create(Mapper.Map<tbl_MOTDs>(model));

            UoW.Commit();

            return Ok(Mapper.Map<MOTDModel>(result));
        }

        [Route("v1/{motdID:guid}"), HttpDelete]
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult DeleteV1([FromRoute] string motdValue)
        {
            var motd = UoW.MOTDs.Get(x => x.Id == motdValue)
                .SingleOrDefault();

            if (motd == null)
            {
                ModelState.AddModelError(MessageType.MOTDNotFound.ToString(), $"MOTD:{motdValue}");
                return NotFound(ModelState);
            }

            UoW.MOTDs.Delete(motd);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{motdValue}"), HttpGet]
        public IActionResult GetV1([FromRoute] string motdValue)
        {
            var motd = UoW.MOTDs.Get(new QueryExpression<tbl_MOTDs>()
                    .Where(x => x.Id == motdValue).ToLambda())
                    .SingleOrDefault();

            if (motd == null)
            {
                ModelState.AddModelError(MessageType.MOTDNotFound.ToString(), $"MOTD:{motdValue}");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<MOTDModel>(motd));
        }

        [Route("v1/page"), HttpPost]
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult GetV1([FromBody] PageStateTypeC model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new PageStateTypeCResult<MOTDModel>
                {
                    Data = Mapper.Map<IEnumerable<MOTDModel>>(
                        UoW.MOTDs.Get(
                            Mapper.MapExpression<Expression<Func<IQueryable<tbl_MOTDs>, IQueryable<tbl_MOTDs>>>>(
                                model.ToExpression<tbl_MOTDs>()))),

                    Total = UoW.MOTDs.Count(
                        Mapper.MapExpression<Expression<Func<IQueryable<tbl_MOTDs>, IQueryable<tbl_MOTDs>>>>(
                            model.ToPredicateExpression<tbl_MOTDs>()))
                };

                return Ok(result);
            }
            catch (QueryExpressionException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());
                return BadRequest(ModelState);
            }
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult UpdateV1([FromBody] MOTDModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var motd = UoW.MOTDs.Get(x => x.Id == model.id).SingleOrDefault();

            if (motd == null)
            {
                ModelState.AddModelError(MessageType.MOTDNotFound.ToString(), $"MOTD:{model.id}");
                return NotFound(ModelState);
            }

            var result = UoW.MOTDs.Update(Mapper.Map<tbl_MOTDs>(model));

            UoW.Commit();

            return Ok(Mapper.Map<MOTDModel>(result));
        }
    }
}
