using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.MCP.Abstractions;
using Bhbk.Lib.Identity.MCP.Models;
using Bhbk.Lib.Identity.MCP.Services;
using Bhbk.Lib.Identity.MCP.Tools.Admin;
using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.MCP.Context
{
    public class AdminMCPContext : IMCPToolContext
    {
        private readonly MCPToolRegistry _registry;

        public MCPScope Scope => MCPScope.Admin;
        public Guid? UserId => null;
        public IUnitOfWork UnitOfWork { get; }
        public IReadOnlyList<IMCPTool> Tools => _registry.GetTools();

        public AdminMCPContext(IUnitOfWork uow)
        {
            UnitOfWork = uow ?? throw new ArgumentNullException(nameof(uow));
            _registry = new MCPToolRegistry();

            RegisterTools();
        }

        private void RegisterTools()
        {
            _registry.Register(new UserTool(UnitOfWork));
            _registry.Register(new AudienceTool(UnitOfWork));
            _registry.Register(new IssuerTool(UnitOfWork));
            _registry.Register(new RoleTool(UnitOfWork));
            _registry.Register(new ClaimTool(UnitOfWork));
            _registry.Register(new LoginProviderTool(UnitOfWork));
            _registry.Register(new UserAuthActivityTool(UnitOfWork));
            _registry.Register(new SettingTool(UnitOfWork));
            _registry.Register(new QuoteTool(UnitOfWork));
            _registry.Register(new LLMProviderTool(UnitOfWork));
            _registry.Register(new JobTool(UnitOfWork));
            _registry.Register(new EmailQueueTool(UnitOfWork));
            _registry.Register(new TextQueueTool(UnitOfWork));
            _registry.Register(new ExportTool(UnitOfWork));
            _registry.Register(new SchemaTool(UnitOfWork));
        }

        public IMCPTool GetTool(string name)
        {
            return _registry.GetTool(name);
        }
    }
}
