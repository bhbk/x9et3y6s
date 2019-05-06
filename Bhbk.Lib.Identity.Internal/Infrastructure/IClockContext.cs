using System;

namespace Bhbk.Lib.Identity.Internal.Infrastructure
{
    public interface IClockContext
    {
        DateTimeOffset UtcNow { get; set; }
    }
}
