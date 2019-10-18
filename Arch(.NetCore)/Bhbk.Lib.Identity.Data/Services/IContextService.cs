using Bhbk.Lib.Common.Primitives.Enums;
using System;

namespace Bhbk.Lib.Identity.Data.Services
{
    public interface IContextService
    {
        InstanceContext InstanceType { get; }
    }
}
