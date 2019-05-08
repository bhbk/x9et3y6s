using Bhbk.Lib.Core.Primitives.Enums;
using System;

namespace Bhbk.Lib.Identity.Data.Services
{
    public interface IContextService
    {
        InstanceContext InstanceType { get; }
    }
}
