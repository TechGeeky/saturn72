#region

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Saturn72.Core.Services.Impl.Data;
using Saturn72.Core.Domain;

#endregion

namespace Saturn72.Module.EntityFramework
{
    public class EfUnitOfWork<TDomainModel, TEntity> : DbContext, IUnitOfWork<TDomainModel>
        where TDomainModel : DomainModelBase
        where TEntity : class
    {
        private readonly string _nameOrConnectionString;

        public EfUnitOfWork(string nameOrConnectionString)
        {
            _nameOrConnectionString = nameOrConnectionString;
        }

        public IEnumerable<TDomainModel> GetAll()
        {
            return QueryNewContext(ctx => GetSet(ctx).AsNoTracking().ToDomainModel<TEntity, TDomainModel>());
        }

        public TDomainModel GetById(long id)
        {
            return QueryNewContext(ctx => GetSet(ctx).Find(id)?.ToDomainModel<TEntity, TDomainModel>());
        }

        public TDomainModel Replace(TDomainModel model)

        {
            return QueryNewContext(ctx =>
            {
                var modelAsEntity = model.ToEntity<TDomainModel, TEntity>();

                var entity = GetSet(ctx).Find(model.Id);

                //TODO: maybe update bug here ==> 
                ctx.Entry(entity).CurrentValues.SetValues(modelAsEntity);
                return SaveChangesToContext(ctx) > 0 ? entity.MapToInstance(model) : null;
            });
        }

        public object Create(TDomainModel model)
        {
            return QueryNewContext(ctx =>
            {
                var entity = model.ToEntity<TDomainModel, TEntity>();
                GetSet(ctx).Add(entity);
                var res = SaveChangesToContext(ctx);
                entity.MapToInstance(model);
                return res as object;
            });
        }

        public TDomainModel Update(TDomainModel model)

        {
            return QueryNewContext(ctx =>
            {
                var entity = GetSet(ctx).Find(model.Id);
                if (entity == null)
                    throw new ArgumentException(string.Format("Failed to find entity of type {0} with Id {1}",
                        model.ToString(), model.Id));

                var modelAsEntity = model.ToEntity<TDomainModel, TEntity>();

                var notChangedProperties = GetUnchangedProperties(modelAsEntity, entity);
                model.MapToInstance(entity);

                var entry = ctx.Entry(entity);
                foreach (var ncp in notChangedProperties)
                    entry.Property(ncp).IsModified = false;

                return SaveChangesToContext(ctx) > 0 ? entity.MapToInstance(model) : null;
            });
        }

        public int Delete(long id)
        {
            return Delete(new[] {id});
        }

        public int Delete(IEnumerable<long> ids)
        {
            return QueryNewContext(ctx =>
            {
                var set = GetSet(ctx);
                foreach (var id in ids)
                {
                    var entity = set.Find(id);
                    if (entity == null)
                        throw new ArgumentException(string.Format("Cannot find entity of type {0} with id: {1}",
                            typeof(TEntity).FullName, id));

                    set.Remove(entity);
                }

                return ctx.SaveChanges();
            });
        }

        private IEnumerable<string> GetUnchangedProperties<TEntity>(TEntity source, TEntity destination)
        {
            if (source == null || destination == null)
                throw new ArgumentException("On or more instance are null");

            var allPropertyInfos = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var result = new List<string>();

            foreach (var pInfo in allPropertyInfos)
            {
                if (result.Contains(pInfo.Name))
                    continue;
                var srcValue = pInfo.GetValue(source, null);
                var destValue = pInfo.GetValue(destination, null);

                if (WasNotModified(srcValue, destValue))
                    result.Add(pInfo.Name);
            }
            return result;
        }

        private static bool WasNotModified(object srcValue, object destValue)
        {
            if (srcValue == destValue)
                return true;
            if ((srcValue == null && destValue != null) || (srcValue != null && destValue == null))
                return false;

            return srcValue.Equals(destValue);
        }

        protected virtual TReturnType QueryNewContext<TReturnType>(Func<DbContext, TReturnType> query)
        {
            using (var ctx = CreateNewContext())
            {
                return query(ctx);
            }
        }

        protected virtual async Task<TReturnType> QueryNewContextAsync<TReturnType>(
            Func<DbContext, Task<TReturnType>> query)
        {
            using (var ctx = CreateNewContext())
            {
                return await query(ctx);
            }
        }

        protected virtual DbContext CreateNewContext()
        {
            return new DbContext(_nameOrConnectionString);
        }

        #region Utilities

        private static int SaveChangesToContext(DbContext ctx)
        {
            try
            {
                return ctx.SaveChanges();
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, ex);
                throw ex;
            }
        }

        private static IDbSet<TEntity> GetSet(DbContext ctx)
        {
            return ctx.Set<TEntity>();
        }

        #endregion
    }
}