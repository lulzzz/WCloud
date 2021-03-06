﻿using Lib.data;
using Lib.extension;
using Lib.helper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace WCloud.Framework.Database.EntityFrameworkCore
{
    public static class EFExtension
    {
        private static void Test<T>() where T : class
        {
            IEnumerable<T> list = null;
            IQueryable<T> query = null;
            DbSet<T> set = null;
            Microsoft.EntityFrameworkCore.Internal.InternalDbSet<T> set_ = null;

            //Enumerable extension
            list.Where(x => true);
            //Queryable extension
            query.Where(x => true);
            //Queryable extension。ef没有扩展自己的where
            set.Where(x => true);
            set_.Where(x => true);

            //ienumerable的扩展比如where接收匿名函数用于计算func<t,bool>

            //iqueryable的扩展比如where接收表达式用于"描述计算过程"expression<func<t,bool>>
            //其实ef只是把iqueryable生成的表达式转成了sql执行，
            //最终拿到数据后隐式实现了GetEnumerator
            //just like this:
            list.GetEnumerator();
        }

        /// <summary>
        /// 如果存在未提交的更改就抛出异常
        /// 为了防止不可预测的提交
        /// </summary>
        /// <param name="context"></param>
        public static void ThrowIfHasChanges(this DbContext context)
        {
            if (context.ChangeTracker.HasChanges())
                throw new UnSubmitChangesException($"{context.GetType().FullName}存在未提交的更改");
        }

        public static void RollbackEntityChanges(this DbContext context)
        {
            if (context.ChangeTracker.HasChanges())
            {
                var entries = context.ChangeTracker.Entries()
                       .Where(e => 
                       e.State == EntityState.Added || 
                       e.State == EntityState.Modified || 
                       e.State == EntityState.Deleted)
                       .ToArray();

                foreach (var m in entries)
                {
                    m.State = EntityState.Unchanged;
                }
            }
        }

        /// <summary>
        /// 创建表
        /// </summary>
        public static void TryCreateTable(this DbContext context)
            => context.Database.EnsureCreated();

        public static void AttachIfNot<T>(this DbContext db, T entity) where T : class, IDBTable
        {
            if (entity == null)
                throw new ArgumentNullException($"{nameof(AttachIfNot)}.{nameof(entity)}");

            if (db.ChangeTracker.Entries<T>().Any(ent => ent.Entity == entity))
                return;
            db.Set<T>().Attach(entity);
        }

        /// <summary>
        /// 获取不跟踪的IQueryable用于查询，效率更高
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="set"></param>
        /// <returns></returns>
        public static IQueryable<T> AsNoTrackingQueryable<T>(this IQueryable<T> set) where T : class
        {
            return set.AsQueryableTrackingOrNot(false);
        }

        /// <summary>
        /// 获取追踪或者不追踪的查询对象
        /// </summary>
        public static IQueryable<T> AsQueryableTrackingOrNot<T>(this IQueryable<T> set, bool tracking) where T : class
        {
            return tracking ? set.AsQueryable() : set.AsNoTracking().AsQueryable();
        }

        /// <summary>
        /// 把实体加载到EF上下文，不重复加载
        /// Nop中的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static T AttachEntityToContext<T>(this DbContext db, T entity) where T : class
        {
            //little hack here until Entity Framework really supports stored procedures
            //otherwise, navigation properties of loaded entities are not loaded until an entity is attached to the context
            var set = db.Set<T>();
            //运算符重载
            var alreadyAttached = set.Local.FirstOrDefault(x => x == entity);
            if (alreadyAttached == null)
            {
                //attach new entity
                set.Attach(entity);
                return entity;
            }

            //entity is already loaded
            return alreadyAttached;
        }

        public static async Task<T> FirstOrThrowAsync<T>(this IQueryable<T> query, string error_msg)
        {
            var model = await query.FirstOrDefaultAsync();
            if (model == null)
                throw new DataNotFoundException(error_msg);
            return model;
        }

        /// <summary>
        /// 获取记录总数和分页总数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="page_size"></param>
        /// <returns></returns>
        public static async Task<(int item_count, int page_count)> QueryRowCountAndPageCountAsync<T>(this IQueryable<T> query, int page_size)
        {
            var item_count = await query.CountAsync();
            var page_count = PagerHelper.GetPageCount(item_count, page_size);
            return (item_count, page_count);
        }

        /// <summary>
        /// 自动分页
        /// </summary>
        public static async Task<PagerData<T>> ToPagedListAsync<T, SortColumn>(this IQueryable<T> query,
            int page, int pagesize, Expression<Func<T, SortColumn>> orderby, bool desc = true)
        {
            var data = new PagerData<T>()
            {
                Page = page,
                PageSize = pagesize
            };

            data.ItemCount = await query.CountAsync();
            data.DataList = await query.OrderBy_(orderby, desc).QueryPage(page, pagesize).ToListAsync();

            return data;
        }
    }
}
