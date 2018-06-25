using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Helpers
{
    public class UrlFilter
    {
        private ushort _pageSizeMax = 100;

        [Required]
        public uint PageNum { get; private set; }

        [Required]
        public ushort PageSize { get; private set; }

        [Required]
        public string OrderBy { get; private set; }

        [Required]
        public string Sort { get; private set; }

        public List<string> Extras { get; private set; }

        public UrlFilter(ushort pageSize, uint pageNum, string orderBy, string sort)
        {
            PageNum = pageNum;

            if (PageSize > _pageSizeMax)
                PageSize = 0;
            else
                PageSize = pageSize;

            OrderBy = orderBy;

            if (Sort != "ascending" && Sort != "descending")
                Sort = "ascending";
            else
                Sort = sort;
        }
    }
}
