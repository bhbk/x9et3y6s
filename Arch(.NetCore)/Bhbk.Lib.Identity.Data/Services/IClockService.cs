using System;

namespace Bhbk.Lib.Identity.Data.Services
{
    public interface IClockService
    {
        DateTimeOffset UtcNow { get; set; }
    }
}
