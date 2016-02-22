using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace System.Linq.Expressions
{
	public static class ExpressionExtensions
	{
        public static string GetMemberName<T>(this object me, Expression<Func<T>> expression)
        {
            return expression.GetMemberName();
        }
		public static string GetMemberName<T>(this Expression<Func<T>> expression)
		{
			if (expression.NodeType != ExpressionType.Lambda)
				throw new ArgumentException("La expresión debe ser una lambda", "expression");
			if (expression.Body is MemberExpression)
				return (expression.Body as MemberExpression).Member.Name;
			else if (expression.Body is UnaryExpression)
				return ((MemberExpression)((UnaryExpression)expression.Body).Operand).Member.Name;
			else
				throw new ArgumentException("La expresión lambda debe ser de tipo 'MemberExpression' o 'UnaryExpression'.");
		}
		public static PropertyInfo GetPropertyInfo<T>(this Expression<Func<T>> expression)
		{
			if (expression.NodeType != ExpressionType.Lambda)
				throw new ArgumentException("La expresión debe ser una lambda", "expression");
			if (expression.Body is MemberExpression)
			{
				var mem = (expression.Body as MemberExpression).Member;
				if (mem is PropertyInfo)
					return mem as PropertyInfo;
				throw new ArgumentException("La expresión lambda no hace referencia a un miembro de propiedad.");
			} else if (expression.Body is UnaryExpression)
			{
				var mem = ((MemberExpression)((UnaryExpression)expression.Body).Operand).Member;
				if (mem is PropertyInfo)
					return mem as PropertyInfo;
				throw new ArgumentException("La expresión lambda no hace referencia a un miembro de propiedad.");
			} else
				throw new ArgumentException("La expresión lambda debe ser de tipo 'MemberExpression' o 'UnaryExpression'.");
		}
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
                                                    Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
                                                             Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }
}
