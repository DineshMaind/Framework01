using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace MyDemoApp.Core
{
    public class ModelServiceBase<TDatabaseEntity, TModelEntity, TKey>
        where TDatabaseEntity : class
        where TModelEntity : class
    {
        private readonly ModelRepository _db = null;
        private readonly Func<TDatabaseEntity, TModelEntity> _toBusinessModelExpression = null;
        private readonly Func<TModelEntity, TDatabaseEntity> _toDatabaseModelExpression = null;
        private readonly Expression<Func<TDatabaseEntity, TKey>> _keySelector = null;

        public ModelServiceBase(ModelRepository db, Func<TDatabaseEntity, TModelEntity> toBusinessModelExpression, Func<TModelEntity, TDatabaseEntity> toDataModelExpression, Expression<Func<TDatabaseEntity, TKey>> keySelector)
        {
            this._db = db;
            this._toBusinessModelExpression = toBusinessModelExpression;
            this._toDatabaseModelExpression = toDataModelExpression;
            this._keySelector = keySelector;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual TDatabaseEntity Add(TModelEntity entity)
        {
            return this._db.Add(entity, this._toDatabaseModelExpression);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual TDatabaseEntity Delete(TModelEntity entity)
        {
            return this._db.Delete(entity, this._toDatabaseModelExpression);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TModelEntity GetObject(Func<TDatabaseEntity, bool> databasePredicate)
        {
            return this._db.GetObject(this._toBusinessModelExpression, databasePredicate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IEnumerable<TModelEntity> GetObjectList(Func<TDatabaseEntity, bool> databasePredicate = null, Predicate<TModelEntity> modelPredicate = null)
        {
            return this._db.GetObjectList(this._toBusinessModelExpression, databasePredicate, modelPredicate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IEnumerable<TModelEntity> GetObjectRange(int skipCount = 0, int takeCount = 10)
        {
            return this._db.GetObjectRange(this._toBusinessModelExpression, this._keySelector, skipCount, takeCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IPagedList<TViewModelEntity> GetPagedList<TViewModelEntity>(Func<TModelEntity, TViewModelEntity> toViewModelExpression, int pageNo = 1, int pageSize = 10)
            where TViewModelEntity : class
        {
            return this._db.GetPagedList(this._toBusinessModelExpression, toViewModelExpression, this._keySelector, pageNo, pageSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual TDatabaseEntity Update(TModelEntity entity, IEnumerable<string> columnNames = null)
        {
            return this._db.Update(entity, this._toDatabaseModelExpression, columnNames);
        }
    }
}