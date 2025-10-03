using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.MCP.Abstractions;
using Bhbk.Lib.Identity.MCP.Models;
using Bhbk.Lib.Identity.MCP.Services;
using Bhbk.Lib.Identity.MCP.Tools.User;
using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.MCP.Context
{
    public class UserMCPContext : IMCPToolContext
    {
        private readonly MCPToolRegistry _registry;

        public MCPScope Scope => MCPScope.User;
        public Guid? UserId { get; }
        public IUnitOfWork UnitOfWork { get; }
        public IReadOnlyList<IMCPTool> Tools => _registry.GetTools();

        public UserMCPContext(IUnitOfWork uow, Guid userId)
        {
            UnitOfWork = uow ?? throw new ArgumentNullException(nameof(uow));
            UserId = userId;
            _registry = new MCPToolRegistry();

            RegisterTools();
        }

        private void RegisterTools()
        {
            _registry.Register(new ProfileTool(UnitOfWork, UserId.Value));
            _registry.Register(new SessionTool(UnitOfWork, UserId.Value));
            _registry.Register(new UserQuoteTool(UnitOfWork));
        }

        public IMCPTool GetTool(string name)
        {
            return _registry.GetTool(name);
        }
    }
}
