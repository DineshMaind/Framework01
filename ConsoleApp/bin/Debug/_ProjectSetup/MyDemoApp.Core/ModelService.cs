using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace MyDemoApp.Core
{
    public class ModelService<TDatabaseEntity, TModelEntity, TKey>
        where TDatabaseEntity : class
        where TModelEntity : class
    {
        private readonly ModelServiceBase<TDatabaseEntity, TModelEntity, TKey> _service = null;

        public ModelService(ModelServiceBase<TDatabaseEntity, TModelEntity, TKey> service)
        {
            this._service = service;
        }

        public ModelServiceBase<TDatabaseEntity, TModelEntity, TKey> ServiceBase { get { return this._service; } }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Add(TModelEntity model)
        {
            this._service.Add(model);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Delete(TModelEntity model)
        {
            this._service.Delete(model);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IEnumerable<TModelEntity> GetObjectList(Func<TDatabaseEntity, bool> databasePredicate = null, Predicate<TModelEntity> modelPredicate = null)
        {
            return this._service.GetObjectList(databasePredicate, modelPredicate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TModelEntity GetObject(Func<TDatabaseEntity, bool> databasePredicate)
        {
            return this._service.GetObject(databasePredicate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IEnumerable<TModelEntity> GetObjectRange(int pageNo = 1, int pageSize = 10)
        {
            return this._service.GetObjectRange(pageNo, pageSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IPagedList<TViewModelEntity> GetPagedList<TViewModelEntity>(Func<TModelEntity, TViewModelEntity> toViewModelExpression, int pageNo = 1, int pageSize = 10)
            where TViewModelEntity : class
        {
            return this._service.GetPagedList(toViewModelExpression, pageNo, pageSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Update(TModelEntity model)
        {
            this._service.Update(model);
        }
    }
}