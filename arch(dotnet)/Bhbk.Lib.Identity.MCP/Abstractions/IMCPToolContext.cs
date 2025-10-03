using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.MCP.Models;
using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.MCP.Abstractions
{
    public interface IMCPToolContext
    {
        MCPScope Scope { get; }
        Guid? UserId { get; }
        IUnitOfWork UnitOfWork { get; }
        IReadOnlyList<IMCPTool> Tools { get; }
        IMCPTool GetTool(string name);
    }
}
