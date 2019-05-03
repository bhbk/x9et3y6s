using System;

namespace Bhbk.Lib.Identity.Internal.Infrastructure
{
    public interface IClockService
    {
        DateTime GetCurrentTime();
    }
}
