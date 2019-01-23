using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.EntityFramework
{
    public class TrackingQueryVisitor : DefaultExpressionVisitor
    {
        public override DbExpression Visit(DbScanExpression expression)
        {
            var column = ColumnAnnotationAttribute.GetColumnName<TrackDeletedBitAttribute>(expression.Target.ElementType);
            //            var column = TrackDeletedBitAttribute.GetColumnName(expression.Target.ElementType);
            if (column != null)
            {
                var binding = expression.Bind();
                return binding
                    .Filter(binding.VariableType.Variable(binding.VariableName)
                        .Property(column)
                        .NotEqual(DbExpression.FromBoolean(true))
                        .Or(binding.VariableType.Variable(binding.VariableName)
                            .Property(column).IsNull()));

            }
            else
            {
                return base.Visit(expression);
            }
        }
    }
}