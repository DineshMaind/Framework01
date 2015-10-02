using System;
using System.Linq;
using System.Linq.Expressions;
namespace MyDemoApp.Core
{
    public static class QueryableExtension
    {
        public static IPagedList<TTarget> ToPagedList<TSource, TKey, TTarget>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, int pageNo, int pageSize, Func<TSource, TTarget> toTargetObject)
            where TSource : class
            where TTarget : class
        {
            var skipIndex = (pageNo - 1) * pageSize;

            var totalRecords = source.Count();

            if (skipIndex < 0 || skipIndex >= totalRecords)
            {
                return null;
            }

            var sourceList = source.OrderBy(keySelector).Skip(skipIndex).Take(pageSize);

            PagedList<TTarget> pagedList = new PagedList<TTarget>();

            foreach (var item in sourceList)
            {
                pagedList.Add(toTargetObject(item));
            }

            var pageCount = (totalRecords / pageSize) + ((totalRecords % pageSize) > 0 ? 1 : 0);

            pagedList.HasNextPage = (pageNo < pageCount);
            pagedList.HasPreviousPage = (pageNo > 1);
            pagedList.IsFirstPage = (pageNo == 1);
            pagedList.IsLastPage = (pageNo == pageSize);
            pagedList.PageCount = pageCount;
            pagedList.PageIndex = (pageNo - 1);
            pagedList.PageNumber = pageNo;
            pagedList.PageSize = pageSize;
            pagedList.TotalItemCount = totalRecords;

            return pagedList;
        }
    }
}
