using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;

namespace Fuxion.EntityFramework;

public class TrackingQueryVisitor : DefaultExpressionVisitor
{
	public override DbExpression Visit(DbScanExpression expression)
	{
		var column = ColumnAnnotationAttribute.GetColumnName<TrackDeletedBitAttribute>(expression.Target.ElementType);
		//            var column = TrackDeletedBitAttribute.GetColumnName(expression.Target.ElementType);
		if (column != null)
		{
			var binding = expression.Bind();
			return binding.Filter(binding.VariableType.Variable(binding.VariableName).Property(column).NotEqual(DbExpression.FromBoolean(true))
				.Or(binding.VariableType.Variable(binding.VariableName).Property(column).IsNull()));
		}
		return base.Visit(expression);
	}
}