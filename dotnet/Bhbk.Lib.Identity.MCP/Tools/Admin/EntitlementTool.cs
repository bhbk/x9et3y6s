using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.MCP.Abstractions;
using Bhbk.Lib.Identity.MCP.Models;
using Bhbk.Lib.Identity.MCP.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.MCP.Tools.Admin
{
    public class EntitlementTool : IMCPTool
    {
        private readonly IUnitOfWork _uow;
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public MCPToolDefinition Definition => new MCPToolDefinition
        {
            Name = "entitlements",
            Description = "Query user entitlements, audience entitlements, entitlement types, and entitlement scopes.",
            Scope = MCPScope.Admin,
            InputSchema = JObject.Parse(@"{
                'type': 'object',
                'properties': {
                    'action': { 'type': 'string', 'enum': ['types', 'scopes', 'user_list', 'audience_list', 'byuser', 'byaudience', 'search'], 'description': 'The operation to perform. types returns all entitlement types. scopes returns all entitlement scopes. user_list returns a paginated list of user entitlements. audience_list returns a paginated list of audience entitlements. byuser returns entitlements for a specific user. byaudience returns entitlements for a specific audience. search finds entitlements by type or scope name.' },
                    'id': { 'type': 'string', 'description': 'User ID or Audience ID (GUID) required for byuser and byaudience actions.' },
                    'query': { 'type': 'string', 'description': 'Search text for the search action. Matches against entitlement type and scope names.' },
                    'skip': { 'type': 'integer', 'description': 'Number of records to skip for pagination (default: 0).' },
                    'take': { 'type': 'integer', 'description': 'Number of records to return (default: 50, max: 100).' }
                },
                'required': ['action']
            }")
        };

        public EntitlementTool(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public Task<MCPToolResult> ExecuteAsync(JObject parameters)
        {
            var action = parameters["action"]?.ToString()?.ToLower();
            var skip = parameters["skip"]?.Value<int>() ?? 0;
            var take = Math.Min(parameters["take"]?.Value<int>() ?? 50, 100);

            try
            {
                switch (action)
                {
                    case "types":
                        return Task.FromResult(ListTypes());

                    case "scopes":
                        return Task.FromResult(ListScopes());

                    case "user_list":
                        return Task.FromResult(ListUserEntitlements(skip, take));

                    case "audience_list":
                        return Task.FromResult(ListAudienceEntitlements(skip, take));

                    case "byuser":
                        var userId = parameters["id"]?.ToString();
                        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                            return Task.FromResult(MCPToolResult.Fail("Valid user ID is required"));
                        return Task.FromResult(GetByUser(userGuid, skip, take));

                    case "byaudience":
                        var audId = parameters["id"]?.ToString();
                        if (string.IsNullOrEmpty(audId) || !Guid.TryParse(audId, out var audGuid))
                            return Task.FromResult(MCPToolResult.Fail("Valid audience ID is required"));
                        return Task.FromResult(GetByAudience(audGuid, skip, take));

                    case "search":
                        var query = parameters["query"]?.ToString();
                        if (string.IsNullOrEmpty(query))
                            return Task.FromResult(MCPToolResult.Fail("Search query is required"));
                        return Task.FromResult(SearchEntitlements(query, skip, take));

                    default:
                        return Task.FromResult(MCPToolResult.Fail($"Unknown action: {action}"));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(MCPToolResult.Fail($"Error: {ex.Message}"));
            }
        }

        private MCPToolResult ListTypes()
        {
            var types = _uow.EntitlementTypes.Get(x => true)
                .OrderBy(x => x.SortOrder)
                .ToList();

            var result = new JObject
            {
                ["total"] = types.Count,
                ["types"] = JArray.FromObject(types, JsonSerializer.Create(_jsonSettings))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult ListScopes()
        {
            var scopes = _uow.EntitlementScopes.Get(x => true)
                .OrderBy(x => x.SortOrder)
                .ToList();

            var result = new JObject
            {
                ["total"] = scopes.Count,
                ["scopes"] = JArray.FromObject(scopes, JsonSerializer.Create(_jsonSettings))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult ListUserEntitlements(int skip, int take)
        {
            var entitlements = _uow.UserEntitlements.Get(x => true)
                .OrderBy(x => x.Created)
                .Skip(skip)
                .Take(take)
                .ToList();

            var total = _uow.UserEntitlements.Get(x => true).Count();
            var result = new JObject
            {
                ["total"] = total,
                ["skip"] = skip,
                ["take"] = take,
                ["entitlements"] = JArray.FromObject(entitlements, JsonSerializer.Create(_jsonSettings))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult ListAudienceEntitlements(int skip, int take)
        {
            var entitlements = _uow.AudienceEntitlements.Get(x => true)
                .OrderBy(x => x.Created)
                .Skip(skip)
                .Take(take)
                .ToList();

            var total = _uow.AudienceEntitlements.Get(x => true).Count();
            var result = new JObject
            {
                ["total"] = total,
                ["skip"] = skip,
                ["take"] = take,
                ["entitlements"] = JArray.FromObject(entitlements, JsonSerializer.Create(_jsonSettings))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetByUser(Guid userId, int skip, int take)
        {
            var user = _uow.Users.Get(x => x.Id == userId).FirstOrDefault();
            if (user == null)
                return MCPToolResult.Fail($"User not found: {userId}");

            var entitlements = _uow.UserEntitlements.Get(x => x.UserId == userId)
                .OrderBy(x => x.Created)
                .Skip(skip)
                .Take(take)
                .ToList();

            var result = new JObject
            {
                ["userId"] = userId.ToString(),
                ["userName"] = user.UserName,
                ["count"] = entitlements.Count,
                ["entitlements"] = JArray.FromObject(entitlements, JsonSerializer.Create(_jsonSettings))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult GetByAudience(Guid audienceId, int skip, int take)
        {
            var audience = _uow.Audiences.Get(x => x.Id == audienceId).FirstOrDefault();
            if (audience == null)
                return MCPToolResult.Fail($"Audience not found: {audienceId}");

            var entitlements = _uow.AudienceEntitlements.Get(x => x.AudienceId == audienceId)
                .OrderBy(x => x.Created)
                .Skip(skip)
                .Take(take)
                .ToList();

            var result = new JObject
            {
                ["audienceId"] = audienceId.ToString(),
                ["audienceName"] = audience.Name,
                ["count"] = entitlements.Count,
                ["entitlements"] = JArray.FromObject(entitlements, JsonSerializer.Create(_jsonSettings))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }

        private MCPToolResult SearchEntitlements(string query, int skip, int take)
        {
            var lowerQuery = query.ToLower();

            var matchingTypeIds = _uow.EntitlementTypes.Get(x =>
                    x.Name.ToLower().Contains(lowerQuery) ||
                    (x.Description != null && x.Description.ToLower().Contains(lowerQuery)))
                .Select(x => x.Id)
                .ToList();

            var matchingScopeIds = _uow.EntitlementScopes.Get(x =>
                    x.Name.ToLower().Contains(lowerQuery) ||
                    (x.Description != null && x.Description.ToLower().Contains(lowerQuery)))
                .Select(x => x.Id)
                .ToList();

            var userEntitlements = _uow.UserEntitlements.Get(x =>
                    matchingTypeIds.Contains(x.EntitlementTypeId) ||
                    matchingScopeIds.Contains(x.EntitlementScopeId))
                .OrderBy(x => x.Created)
                .Skip(skip)
                .Take(take)
                .ToList();

            var result = new JObject
            {
                ["query"] = query,
                ["count"] = userEntitlements.Count,
                ["userEntitlements"] = JArray.FromObject(userEntitlements, JsonSerializer.Create(_jsonSettings))
            };

            return MCPToolResult.Ok(SensitiveFieldFilter.Filter(result));
        }
    }
}
