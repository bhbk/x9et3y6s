using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace Bhbk.Lib.Identity.Internal.InfrastructureTest
{
    public static class OrderHelperV2
    {
        public static IQueryable<T> BuildOrdersV2<T>(this IEnumerable<T> source, params OrdersV2[] properties)
        {
            if (properties == null || properties.Length == 0)
                return null;

            var typeOfT = typeof(T);

            Type t = typeOfT;

            IOrderedEnumerable<T> result = null;
            var thenBy = false;

            foreach (var item in properties.Select(prop => new {
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
                    var prevExpr = Expression.Parameter(typeof(IOrderedEnumerable<T>), "prevExpr");
                    var expr = Expression.Lambda<Func<IOrderedEnumerable<T>, IOrderedEnumerable<T>>>(
                        Expression.Call((isAscending ? thenByMethod : thenByDescendingMethod).MakeGenericMethod(typeOfT, propertyType), prevExpr,
                            Expression.Lambda(typeof(Func<,>).MakeGenericType(typeOfT, propertyType),
                                Expression.MakeMemberAccess(origExpr, propertyInfo), origExpr)
                            ),
                        prevExpr).Compile();

                    result = expr(result);
                }
                else
                {
                    var prevExpr = Expression.Parameter(typeof(IEnumerable<T>), "prevExpr");
                    var expr = Expression.Lambda<Func<IEnumerable<T>, IOrderedEnumerable<T>>>(
                        Expression.Call((isAscending ? orderByMethod : orderByDescendingMethod).MakeGenericMethod(typeOfT, propertyType), prevExpr,
                            Expression.Lambda(typeof(Func<,>).MakeGenericType(typeOfT, propertyType),
                                Expression.MakeMemberAccess(origExpr, propertyInfo), origExpr)
                            ),
                        prevExpr).Compile();

                    result = expr(source);
                    thenBy = true;
                }
            }

            return result.AsQueryable();
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

    public static class SampleOrdersV2
    {
        private static void Main()
        {
            var data = new List<CustomerV2>
            {
              new CustomerV2 {ID = 3, Name = "a"},
              new CustomerV2 {ID = 3, Name = "c"},
              new CustomerV2 {ID = 4},
              new CustomerV2 {ID = 3, Name = "b"},
              new CustomerV2 {ID = 2}
            };

            var result = data.BuildOrdersV2(
              new OrdersV2("ID", ListSortDirection.Ascending),
              new OrdersV2("Name", ListSortDirection.Ascending));
        }
    }

    public class OrdersV2
    {
        public string PropertyName;
        public ListSortDirection Direction;

        public OrdersV2(string property, ListSortDirection ascending)
        {
            PropertyName = property;
            Direction = ascending;
        }
    }

    public class CustomerV2
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
}
