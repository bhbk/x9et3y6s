using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace Bhbk.Lib.Identity.Internal.InfrastructureTest
{
    public static class OrderHelperV3
    {
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string ordering, params object[] values)
        {
            var resultExp = CreateMethodCallExpression(source, "OrderBy", ordering);
            return source.Provider.CreateQuery<T>(resultExp);
        }

        public static IQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string ordering, params object[] values)
        {
            var resultExp = CreateMethodCallExpression(source, "OrderByDescending", ordering);
            return source.Provider.CreateQuery<T>(resultExp);
        }

        public static IQueryable<T> ThenBy<T>(this IQueryable<T> source, string ordering, params object[] values)
        {
            var resultExp = CreateMethodCallExpression(source, "ThenBy", ordering);
            return source.Provider.CreateQuery<T>(resultExp);
        }

        public static IQueryable<T> ThenByDescending<T>(this IQueryable<T> source, string ordering, params object[] values)
        {
            var resultExp = CreateMethodCallExpression(source, "ThenByDescending", ordering);
            return source.Provider.CreateQuery<T>(resultExp);
        }

        private static MethodCallExpression CreateMethodCallExpression<T>(IQueryable<T> source, string methodName, string ordering)
        {
            var strings = ordering.Split('.');

            var types = new List<Type>();
            var properties = new List<PropertyInfo>();
            var propertyAccesses = new List<MemberExpression>();

            types.Add(typeof(T));

            for (int i = 0; i < strings.Length; i++)
            {
                if (i != 0)
                    types.Add(properties[i - 1].PropertyType);

                properties.Add(types[i].GetProperty(strings[i]));
            }

            var parameter = Expression.Parameter(types[0], "p");

            for (int i = 0; i < properties.Count; i++)
            {
                propertyAccesses.Add(i == 0
                    ? Expression.MakeMemberAccess(parameter, properties[i])
                    : Expression.MakeMemberAccess(propertyAccesses[i - 1], properties[i]));
            }

            var orderByExp = Expression.Lambda(propertyAccesses.Last(), parameter);

            return Expression.Call(typeof(Queryable), methodName,
                new Type[] { types.First(), properties.Last().PropertyType }, source.Expression, Expression.Quote(orderByExp));
        }
    }
}
