using System;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EF.Models
{
    public partial class tbl_ChatPrompt
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string PromptType { get; set; }
        public string Content { get; set; }
        public bool IsEnabled { get; set; }
        public int SortOrder { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset? ModifiedUtc { get; set; }
    }
}
