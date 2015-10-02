using System.Collections.Generic;

namespace MyDemoApp.Core
{
    public interface IPagedList<T> : IList<T>
    {
        bool HasNextPage { get; }
        bool HasPreviousPage { get; }
        bool IsFirstPage { get; }
        bool IsLastPage { get; }
        int PageCount { get; }
        int PageIndex { get; }
        int PageNumber { get; }
        int PageSize { get; }
        int TotalItemCount { get; }
    }
}