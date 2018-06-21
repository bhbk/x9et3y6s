using System;

namespace Bhbk.Lib.Identity.Interfaces
{
    public interface IActivityEntry
    {
        Guid Id { get; set; }
        Guid ActorId { get; set; }
        string ActivityType { get; set; }
        string TableName { get; set; }
        string KeyValues { get; set; }
        string OriginalValues { get; set; }
        string CurrentValues { get; set; }
        DateTime Created { get; set; }
        bool Immutable { get; set; }
    }
}
