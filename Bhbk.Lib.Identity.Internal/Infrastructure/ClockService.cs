using System;

namespace Bhbk.Lib.Identity.Internal.Infrastructure
{
    public class ClockService : IClockService
    {
        public DateTime GetCurrentTime()
        {
            return DateTime.UtcNow;
        }
    }
}
