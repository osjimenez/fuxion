namespace Fuxion.EntityFramework;

using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure.Interception;
using System.Reflection;

public class TrackingInterceptor : IDbCommandTreeInterceptor
{
	public TrackingInterceptor(ITimeProvider timeProvider, Func<TimeZoneInfo>? getCurrentTimeZoneFunction = null, Func<string>? getCurrentIdentityFunction = null)
	{
		this.timeProvider = timeProvider;
		this.getCurrentTimeZoneFunction = getCurrentTimeZoneFunction ?? (() => TimeZoneInfo.Local);
		this.getCurrentIdentityFunction = getCurrentIdentityFunction ?? (() => "<undefined>");//Thread.CurrentPrincipal.Identity.Name
	}

	private readonly ITimeProvider timeProvider;
	private readonly Func<TimeZoneInfo> getCurrentTimeZoneFunction;
	private readonly Func<string> getCurrentIdentityFunction;
	private static TimeSpan _dateTimeAdjustment = TimeSpan.Zero;
	public static void SetDateTimeAdjustment(TimeSpan dateTimeAdjustment) => _dateTimeAdjustment = dateTimeAdjustment;
	private void AddOrReplaceClause(ICollection<DbModificationClause> clauses, DbSetClause clause)
	{
		var res = clauses.FirstOrDefault(c => ((DbPropertyExpression)((DbSetClause)c).Property).Property.Name == ((DbPropertyExpression)clause.Property).Property.Name);
		if (res != null)
			clauses.Remove(res);
		clauses.Add(clause);
	}
	private bool ExistClause(IEnumerable<DbModificationClause> clauses, string propertyName)
	{
		var res = clauses.FirstOrDefault(c => ((DbPropertyExpression)((DbSetClause)c).Property).Property.Name == propertyName);
		return res != null;
	}
	private void RemoveClause(List<DbModificationClause> clauses, string? propertyName)
	{
		var res = clauses.FirstOrDefault(c => ((DbPropertyExpression)((DbSetClause)c).Property).Property.Name == propertyName);
		if (res != null)
			clauses.Remove(res);
	}
	public void TreeCreated(DbCommandTreeInterceptionContext interceptionContext)
	{
		if (interceptionContext.Result.DataSpace == DataSpace.SSpace &&
			interceptionContext.DbContexts.All(con => con.GetType().GetCustomAttribute<TrackDisabledAttribute>() == null))
		{
			#region Query
			var queryCommand = interceptionContext.Result as DbQueryCommandTree;
			if (queryCommand != null)
			{
				var newQuery = queryCommand.Query.Accept(new TrackingQueryVisitor());
				interceptionContext.Result = new DbQueryCommandTree(
					queryCommand.MetadataWorkspace,
					queryCommand.DataSpace,
					newQuery);
			}
			#endregion
			#region Delete
			var deleteCommand = interceptionContext.Result as DbDeleteCommandTree;
			if (deleteCommand != null)
			{
				var bitColumn = ColumnAnnotationAttribute.GetColumnName<TrackDeletedBitAttribute>(deleteCommand.Target.VariableType.EdmType);
				var dateColumn = ColumnAnnotationAttribute.GetColumnName<TrackDeletedDateAttribute>(deleteCommand.Target.VariableType.EdmType);
				var identityColumn = ColumnAnnotationAttribute.GetColumnName<TrackDeletedIdentityAttribute>(deleteCommand.Target.VariableType.EdmType);
				var clauses = new List<DbModificationClause>();
				if (bitColumn != null)
				{
					clauses.Add(DbExpressionBuilder.SetClause(deleteCommand.Target.Variable.Property(bitColumn), DbExpression.FromBoolean(true)));
					if (dateColumn != null)
						clauses.Add(DbExpressionBuilder.SetClause(deleteCommand.Target.Variable.Property(dateColumn), DbExpression.FromDateTimeOffset(TimeZoneInfo.ConvertTime(timeProvider.NowOffsetted().Add(_dateTimeAdjustment), getCurrentTimeZoneFunction()))));
					if (identityColumn != null)
						clauses.Add(DbExpressionBuilder.SetClause(deleteCommand.Target.Variable.Property(identityColumn), DbExpression.FromString(getCurrentIdentityFunction())));
				}
				if (clauses.Count > 0)
				{
					//Add this constant clause to trace deletions on update command
					clauses.Add(DbExpressionBuilder.SetClause(DbExpressionBuilder.Constant(137), DbExpressionBuilder.Constant(137)));
					interceptionContext.Result = new DbUpdateCommandTree(
						deleteCommand.MetadataWorkspace,
						deleteCommand.DataSpace,
						deleteCommand.Target,
						deleteCommand.Predicate,
						clauses.AsReadOnly(),
						null);
				}
			}
			#endregion
			#region Insert
			var insertCommand = interceptionContext.Result as DbInsertCommandTree;
			if (insertCommand != null)
			{
				var dateColumn = ColumnAnnotationAttribute.GetColumnName<TrackCreatedDateAttribute>(insertCommand.Target.VariableType.EdmType);
				var identityColumn = ColumnAnnotationAttribute.GetColumnName<TrackCreatedIdentityAttribute>(insertCommand.Target.VariableType.EdmType);
				var clauses = insertCommand.SetClauses.ToList();
				RemoveClause(clauses, ColumnAnnotationAttribute.GetColumnName<TrackDeletedBitAttribute>(insertCommand.Target.VariableType.EdmType));
				RemoveClause(clauses, ColumnAnnotationAttribute.GetColumnName<TrackDeletedDateAttribute>(insertCommand.Target.VariableType.EdmType));
				RemoveClause(clauses, ColumnAnnotationAttribute.GetColumnName<TrackDeletedIdentityAttribute>(insertCommand.Target.VariableType.EdmType));
				RemoveClause(clauses, ColumnAnnotationAttribute.GetColumnName<TrackModifiedDateAttribute>(insertCommand.Target.VariableType.EdmType));
				RemoveClause(clauses, ColumnAnnotationAttribute.GetColumnName<TrackModifiedIdentityAttribute>(insertCommand.Target.VariableType.EdmType));
				if (dateColumn != null)
					AddOrReplaceClause(clauses, DbExpressionBuilder.SetClause(insertCommand.Target.Variable.Property(dateColumn), DbExpression.FromDateTimeOffset(TimeZoneInfo.ConvertTime(timeProvider.NowOffsetted().Add(_dateTimeAdjustment), getCurrentTimeZoneFunction()))));
				if (identityColumn != null)
					AddOrReplaceClause(clauses, DbExpressionBuilder.SetClause(insertCommand.Target.Variable.Property(identityColumn), DbExpression.FromString(getCurrentIdentityFunction())));
				if (dateColumn != null || (identityColumn != null))
					interceptionContext.Result = new DbInsertCommandTree(
						insertCommand.MetadataWorkspace,
						insertCommand.DataSpace,
						insertCommand.Target,
						clauses.AsReadOnly(),
						null);
			}
			#endregion
			#region Update
			var updateCommand = interceptionContext.Result as DbUpdateCommandTree;
			if (updateCommand != null)
			{
				var dateColumn = ColumnAnnotationAttribute.GetColumnName<TrackModifiedDateAttribute>(updateCommand.Target.VariableType.EdmType);
				var identityColumn = ColumnAnnotationAttribute.GetColumnName<TrackModifiedIdentityAttribute>(updateCommand.Target.VariableType.EdmType);
				var clauses = updateCommand.SetClauses.ToList();
				var traceDeleteionsClause = clauses.FirstOrDefault(c =>
					((DbSetClause)c).Property is DbConstantExpression && ((DbConstantExpression)((DbSetClause)c).Property).Value is int && ((int)((DbConstantExpression)((DbSetClause)c).Property).Value) == 137 &&
					((DbSetClause)c).Value is DbConstantExpression && ((DbConstantExpression)((DbSetClause)c).Value).Value is int && ((int)((DbConstantExpression)((DbSetClause)c).Value).Value) == 137
					);
				if (traceDeleteionsClause != null)
				{
					//This command is derived from a deletion operation
					clauses.Remove(traceDeleteionsClause);
				}
				else
				{
					RemoveClause(clauses, ColumnAnnotationAttribute.GetColumnName<TrackDeletedBitAttribute>(updateCommand.Target.VariableType.EdmType));
					RemoveClause(clauses, ColumnAnnotationAttribute.GetColumnName<TrackDeletedDateAttribute>(updateCommand.Target.VariableType.EdmType));
					RemoveClause(clauses, ColumnAnnotationAttribute.GetColumnName<TrackDeletedIdentityAttribute>(updateCommand.Target.VariableType.EdmType));
					RemoveClause(clauses, ColumnAnnotationAttribute.GetColumnName<TrackCreatedDateAttribute>(updateCommand.Target.VariableType.EdmType));
					RemoveClause(clauses, ColumnAnnotationAttribute.GetColumnName<TrackCreatedIdentityAttribute>(updateCommand.Target.VariableType.EdmType));
					if (dateColumn != null)
						AddOrReplaceClause(clauses, DbExpressionBuilder.SetClause(updateCommand.Target.Variable.Property(dateColumn), DbExpression.FromDateTimeOffset(TimeZoneInfo.ConvertTime(timeProvider.NowOffsetted().Add(_dateTimeAdjustment), getCurrentTimeZoneFunction()))));
					if (identityColumn != null)
						AddOrReplaceClause(clauses, DbExpressionBuilder.SetClause(updateCommand.Target.Variable.Property(identityColumn), DbExpression.FromString(getCurrentIdentityFunction())));
				}
				if (dateColumn != null || (identityColumn != null))
					interceptionContext.Result = new DbUpdateCommandTree(
						updateCommand.MetadataWorkspace,
						updateCommand.DataSpace,
						updateCommand.Target,
						updateCommand.Predicate,
						clauses.AsReadOnly(),
						null);
			}
			#endregion
		}
	}
}