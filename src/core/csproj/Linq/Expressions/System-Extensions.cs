using System.Collections.Generic;
using System.Reflection;
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
        public static string GetMemberName<TInstance, TMember>(this Expression<Func<TInstance, TMember>> expression)
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
        public class ParameterRebinder : ExpressionVisitor
        {
            private readonly Dictionary<ParameterExpression, ParameterExpression> map;

            public ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
            {
                this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
            }

            public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
            {
                return new ParameterRebinder(map).Visit(exp);
            }

            protected override Expression VisitParameter(ParameterExpression p)
            {
                ParameterExpression replacement;
                if (map.TryGetValue(p, out replacement))
                {
                    p = replacement;
                }
                return base.VisitParameter(p);
            }
        }
        public static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
        {
            // build parameter map (from parameters of second to parameters of first)
            var map = first.Parameters.Select((f, i) => new { f, s = second.Parameters[i] }).ToDictionary(p => p.s, p => p.f);

            // replace parameters in the second lambda expression with parameters from the first
            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);

            // apply composition of lambda expression bodies to parameters from the first expression 
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second) { return first.Compose(second, Expression.And); }
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second) { return first.Compose(second, Expression.Or); }
    }
}
