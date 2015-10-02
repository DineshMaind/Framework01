using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDemoApp.Core
{
    internal class PagedList<TSource> : IPagedList<TSource>
    {
        private IList<TSource> _pagedList = new List<TSource>();

        public int Count
        {
            get { return this._pagedList.Count; }
        }

        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool IsFirstPage { get; set; }
        public bool IsLastPage { get; set; }

        public bool IsReadOnly
        {
            get { return this._pagedList.IsReadOnly; }
        }

        public int PageCount { get; set; }

        public int PageIndex { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItemCount { get; set; }

        public TSource this[int index]
        {
            get
            {
                return this._pagedList[index];
            }
            set
            {
                this._pagedList[index] = value;
            }
        }

        public void Add(TSource item)
        {
            this._pagedList.Add(item);
        }

        public void Clear()
        {
            this._pagedList.Clear();
        }

        public bool Contains(TSource item)
        {
            return this._pagedList.Contains(item);
        }

        public void CopyTo(TSource[] array, int arrayIndex)
        {
            this._pagedList.CopyTo(array, arrayIndex);
        }

        public IEnumerator<TSource> GetEnumerator()
        {
            return this._pagedList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._pagedList.GetEnumerator();
        }

        public int IndexOf(TSource item)
        {
            return this._pagedList.IndexOf(item);
        }

        public void Insert(int index, TSource item)
        {
            this._pagedList.Insert(index, item);
        }

        public bool Remove(TSource item)
        {
            return this._pagedList.Remove(item);
        }

        public void RemoveAt(int index)
        {
            this._pagedList.RemoveAt(index);
        }
    }
}
