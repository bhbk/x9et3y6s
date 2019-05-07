using System;

namespace Bhbk.Lib.Identity.Data.Infrastructure
{
    public interface IClockContext
    {
        DateTimeOffset UtcNow { get; set; }
    }
}
