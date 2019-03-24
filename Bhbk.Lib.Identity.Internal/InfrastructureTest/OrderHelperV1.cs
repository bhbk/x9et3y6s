using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace Bhbk.Lib.Identity.Internal.InfrastructureTest
{
    public static class OrderHelperV1
    {
        public static IQueryable<T> BuildOrdersV1<T>(this IQueryable<T> source, params OrdersV1[] properties)
        {
            if (properties == null || properties.Length == 0)
                return null;

            var typeOfT = typeof(T);

            Type t = typeOfT;

            IOrderedQueryable<T> result = null;
            var thenBy = false;

            foreach (var item in properties.Select(prop => new
            {
                PropertyInfo = t.GetProperty(prop.PropertyName),
                prop.Direction
            }))
            {
                var origExpr = Expression.Parameter(typeOfT, "o");
                var propertyInfo = item.PropertyInfo;
                var propertyType = propertyInfo.PropertyType;
                var isAscending = item.Direction == ListSortDirection.Ascending;

                if (thenBy)
                {
                    var prevExpr = Expression.Parameter(typeof(IOrderedQueryable<T>), "prevExpr");
                    var propAccess = (isAscending ? thenByMethod : thenByDescendingMethod).MakeGenericMethod(typeOfT, propertyType);
                    var expr = Expression.Lambda<Func<IOrderedQueryable<T>, IOrderedQueryable<T>>>(
                        Expression.Call(propAccess, prevExpr,
                            Expression.Lambda(typeof(Func<,>).MakeGenericType(typeOfT, propertyType),
                                Expression.MakeMemberAccess(origExpr, propertyInfo), origExpr)
                            ),
                        prevExpr).Compile();

                    result = expr(result);
                }
                else
                {
                    var prevExpr = Expression.Parameter(typeof(IQueryable<T>), "prevExpr");
                    var expr = Expression.Lambda<Func<IQueryable<T>, IOrderedQueryable<T>>>(
                        Expression.Call((isAscending ? orderByMethod : orderByDescendingMethod).MakeGenericMethod(typeOfT, propertyType), prevExpr,
                            Expression.Lambda(typeof(Func<,>).MakeGenericType(typeOfT, propertyType),
                                Expression.MakeMemberAccess(origExpr, propertyInfo), origExpr)
                            ),
                        prevExpr).Compile();

                    result = expr(source);
                    thenBy = true;
                }
            }

            return result;
        }

        private static MethodInfo orderByMethod =
            MethodOf(() => Enumerable.OrderBy(default(IEnumerable<object>), default(Func<object, object>)))
                .GetGenericMethodDefinition();

        private static MethodInfo orderByDescendingMethod =
            MethodOf(() => Enumerable.OrderByDescending(default(IEnumerable<object>), default(Func<object, object>)))
                .GetGenericMethodDefinition();

        private static MethodInfo thenByMethod =
            MethodOf(() => Enumerable.ThenBy(default(IOrderedEnumerable<object>), default(Func<object, object>)))
                .GetGenericMethodDefinition();

        private static MethodInfo thenByDescendingMethod =
            MethodOf(() => Enumerable.ThenByDescending(default(IOrderedEnumerable<object>), default(Func<object, object>)))
                .GetGenericMethodDefinition();

        public static MethodInfo MethodOf<T>(Expression<Func<T>> method)
        {
            MethodCallExpression mce = (MethodCallExpression)method.Body;
            MethodInfo mi = mce.Method;

            return mi;
        }
    }

    //public static class SampleOrdersV1
    //{
    //    private static void Main()
    //    {
    //        var data = new List<CustomerV1>
    //        {
    //          new CustomerV1 {ID = 3, Name = "a"},
    //          new CustomerV1 {ID = 3, Name = "c"},
    //          new CustomerV1 {ID = 4},
    //          new CustomerV1 {ID = 3, Name = "b"},
    //          new CustomerV1 {ID = 2}
    //        };

    //        var result = data.BuildOrdersV1(
    //          new OrdersV1("ID", ListSortDirection.Ascending),
    //          new OrdersV1("Name", ListSortDirection.Ascending));
    //    }
    //}

    public class OrdersV1
    {
        public string PropertyName;
        public ListSortDirection Direction;

        public OrdersV1(string property, ListSortDirection ascending)
        {
            PropertyName = property;
            Direction = ascending;
        }
    }

    public class CustomerV1
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
}
