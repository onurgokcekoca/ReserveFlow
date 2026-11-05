using System;
using System.Collections.Generic;
using System.Text;

namespace ReserveFlow.Shared.Application.Pagination
{
    public class PagedResult<T>
    {
        public PagedResult(IReadOnlyCollection<T> items, int page, int pageSize, int totalCount, int totalPages)
        {
            Items = items;
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = totalPages;
        }

        public IReadOnlyCollection<T> Items { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public int TotalPages { get; }
    }
}
