using Fuxion.Identity.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fuxion.Identity
{
	public static class PermissionExtensions
	{
		public static bool IsValid(this IPermission me) => me.Function != null && (me.Scopes?.All(s => s.Discriminator != null) ?? true) && me.Scopes?.Select(s => s.Discriminator.TypeId).Distinct().Count() == me.Scopes?.Count();
		internal static bool Match(this IPermission me, bool forFilter, IFunction function, TypeDiscriminator typeDiscriminator, params IDiscriminator[] discriminators)
		{
			using (var res = Printer.CallResult<bool>())
			{
				using (Printer.Indent2("Input parameters"))
				{
					Printer.WriteLine($"Permission:");
					new[] { me }.Print(PrintMode.Table);
					Printer.WriteLine($"'{nameof(forFilter)}': " + forFilter);
					Printer.WriteLine($"Function: {function?.Name ?? "<null>"}");

					if (typeDiscriminator != null)
					{
						Printer.WriteLine($"'{nameof(typeDiscriminator)}':");
						new[] { typeDiscriminator }.Print(PrintMode.Table);
					}
					else
						Printer.WriteLine($"'{nameof(typeDiscriminator)}': null");
					Printer.WriteLine($"Discriminators:");
					discriminators.Print(PrintMode.Table);
				}
				bool Compute()
				{
					if (function == null || !me.MatchByFunction(function))
					{
						Printer.WriteLine($"Matching failed on check the function");
						return false;
					}
					if (!me.MatchByDiscriminatorsInclusionsAndExclusions(forFilter, typeDiscriminator, discriminators))
					{
						Printer.WriteLine($"Matching failed on check the inclusions/exclusions of discriminator");
						return false;
					}
					return true;
				}
				return res.Value = Compute();
			}
		}
		internal static bool MatchByFunction(this IPermission me, IFunction function)
		{
			using (var res = Printer.CallResult<bool>())
			{
				using (Printer.Indent2("Input parameters"))
				{
					Printer.WriteLine($"Permission:");
					new[] { me }.Print(PrintMode.Table);
					Printer.WriteLine($"Function: {function.Name}");
				}
				Printer.WriteLine($"Inclusiones: {me.Function.GetAllInclusions().Aggregate("", (a, s) => a + " - " + s.Id, a => a.Trim(' ', '-'))}");
				Printer.WriteLine($"Exclusiones: {me.Function.GetAllExclusions().Aggregate("", (a, s) => a + " - " + s.Id, a => a.Trim(' ', '-'))}");
				var comparer = new FunctionEqualityComparer();
				if (comparer.Equals(me.Function, function))
				{
					Printer.WriteLine("Match with same function");
					return res.Value = true;
				}
				else if (me.Value && me.Function.GetAllInclusions().Contains(function, comparer))
				{
					Printer.WriteLine("Match by included function");
					return res.Value = true;
				}
				else if (!me.Value && me.Function.GetAllExclusions().Contains(function, comparer))
				{
					Printer.WriteLine("Match by excluded function");
					return res.Value = true;
				}
				else
					return res.Value = false;
			}
		}
		internal static bool MatchByDiscriminatorsInclusionsAndExclusions(this IPermission me, bool forFilter, TypeDiscriminator typeDiscriminator, params IDiscriminator[] discriminators)
		{
			var res = false;
			using (Printer.Indent2($"CALL {nameof(MatchByDiscriminatorsInclusionsAndExclusions)}:", '│'))
			{
				using (Printer.Indent2("Input parameters"))
				{
					Printer.WriteLine($"Permission:");
					new[] { me }.Print(PrintMode.Table);
					Printer.WriteLine($"'{nameof(forFilter)}': " + forFilter);
					if (typeDiscriminator == null)
						Printer.WriteLine($"'{nameof(typeDiscriminator)}': null");
					else
					{
						Printer.WriteLine($"'{nameof(typeDiscriminator)}':");
						new[] { typeDiscriminator }.Print(PrintMode.Table);
					}
					Printer.WriteLine($"'{nameof(discriminators)}':");
					discriminators.Print(PrintMode.Table);
				}
				if (discriminators.Any(d => Comparer.AreEquals(d.TypeId, TypeDiscriminator.TypeDiscriminatorId)))
					throw new ArgumentException($"'{nameof(discriminators)}' cannot contains a '{nameof(TypeDiscriminator)}'");
				if (typeDiscriminator == null)
					throw new ArgumentException($"'{nameof(typeDiscriminator)}' cannot be null");
				bool Compute()
				{
					// Si no hay discriminador de tipo, TRUE
					if (typeDiscriminator == null)
					{
						Printer.WriteLine($"'{nameof(typeDiscriminator)}' is null");
						//return true;
					}
					// Si el permiso no define scopes, TRUE
					if (!me.Scopes.Any())
					{
						Printer.WriteLine("Permission hasn't scopes");
						return true;
					}
					var typeDiscriminatorRelatedWithAnyPermissionScope = false;
					// Compruebo el discriminador de tipo
					using (Printer.Indent2($"Checking type discriminator"))
					{
						var scopeOfTypeOfTypeDiscriminator = me.Scopes.FirstOrDefault(s => Comparer.AreEquals(s.Discriminator.TypeId, typeDiscriminator?.TypeId));
						if (scopeOfTypeOfTypeDiscriminator != null)
						{
							Printer.WriteLine($"The {nameof(typeDiscriminator)} '{typeDiscriminator}' and permission scope '{scopeOfTypeOfTypeDiscriminator}' have same type '{typeDiscriminator.TypeId}', continue");
							var scopeDiscriminatorRelatedWithTargetDiscriminator = scopeOfTypeOfTypeDiscriminator?.Discriminator
								.GetAllRelated(scopeOfTypeOfTypeDiscriminator.Propagation)
								.FirstOrDefault(rel => Comparer.AreEquals(typeDiscriminator.TypeId, rel.TypeId) && Comparer.AreEquals(typeDiscriminator.Id, rel.Id));
							if (scopeDiscriminatorRelatedWithTargetDiscriminator != null)
							{
								Printer.WriteLine($"The {nameof(typeDiscriminator)} '{typeDiscriminator}' is related to permission scope '{scopeOfTypeOfTypeDiscriminator}' on discriminator '{scopeDiscriminatorRelatedWithTargetDiscriminator}', check discriminators");
								typeDiscriminatorRelatedWithAnyPermissionScope = true;
							}
							else if (typeDiscriminator != TypeDiscriminator.Empty)
							{
								Printer.WriteLine($"The {nameof(typeDiscriminator)} '{typeDiscriminator}' isn't related to permission scope '{scopeOfTypeOfTypeDiscriminator}', FALSE");
								return false;
							}
						}
						else
						{
							Printer.WriteLine($"The {nameof(typeDiscriminator)} '{typeDiscriminator}' hasn't any scope with discriminator of its type");
							if (discriminators.IsNullOrEmpty())
							{
								Printer.WriteLine($"Haven't discriminators, VALUE ({me.Value})");
								return me.Value;
							}
							else
								Printer.WriteLine($"Have some discriminators, check discriminators");
						}
					}
					using (Printer.Indent2($"Checking discriminators:"))
					{
						// Compruebo el resto de discriminadores
						return discriminators.All(dis =>
						{
							var scopeOfTypeOfDiscriminator = me.Scopes.FirstOrDefault(s => Comparer.AreEquals(s.Discriminator.TypeId, dis.TypeId));
							var scopeDiscriminatorRelatedWithDiscriminator = scopeOfTypeOfDiscriminator?.Discriminator
								.GetAllRelated(scopeOfTypeOfDiscriminator.Propagation)
								.FirstOrDefault(rel => Comparer.AreEquals(dis.TypeId, rel.TypeId) && Comparer.AreEquals(dis.Id, rel.Id));

							if (scopeOfTypeOfDiscriminator != null)
							{
								Printer.WriteLine($"The discriminator '{dis}' and permission scope '{scopeOfTypeOfDiscriminator}' have same type '{dis.TypeId}'");
								if (scopeDiscriminatorRelatedWithDiscriminator != null)
								{
									Printer.WriteLine($"The discriminator '{dis}' is related to permission scope '{scopeOfTypeOfDiscriminator}' on discriminator '{scopeDiscriminatorRelatedWithDiscriminator}'");
									return true;
								}
								else
								{
									Printer.WriteLine($"The discriminator '{dis}' isn't related to permission scopes, continue");
									if (forFilter)
									{
										Printer.WriteLine($"This search is 'forFilter', TRUE");
										return true;
									}
								}
							}
							else
							{
								Printer.WriteLine($"The permission hasn't any discriminator of type '{dis}', check typeDiscriminator");
								if (typeDiscriminatorRelatedWithAnyPermissionScope)
								{
									Printer.WriteLine($"The {nameof(typeDiscriminator)} '{typeDiscriminator}' is related to any permission scope, TRUE");
									return true;
								}
								else
								{
									Printer.WriteLine($"The {nameof(typeDiscriminator)} '{typeDiscriminator}' isn't related to any permission scope, check if discriminator '{dis}' id is null");
									if (dis.Id.IsNullOrDefault())
									{
										Printer.WriteLine($"Discriminator '{dis}' id is null, VALUE ({me.Value})");
										return me.Value;
									}
									else
									{
										Printer.WriteLine($"Discriminator '{dis}' id isn't null, VALUE ({me.Value})");
										return !me.Value;
									}
								}
								//return dis.Id.IsNullOrDefault() ? me.Value : !me.Value;
							}
							return false;
						});
					}
				}
				res = Compute();
			}
			Printer.WriteLine($"● RESULT {nameof(MatchByDiscriminatorsInclusionsAndExclusions)}: {res}");
			return res;
		}
		public static string ToOneLineString(this IPermission me)
			=> $"{me.Value} - {me.Function.Name} - {me.Scopes.Count()}";
		public static void Print(this IEnumerable<IPermission> me, PrintMode mode)
		{
			switch (mode)
			{
				case PrintMode.OneLine:
					foreach (var per in me)
					{
						Printer.WriteLine(per.Function.Name.PadRight(8, ' ') + " , v:" +
							per.Value + "".PadRight(per.Value ? 1 : 0, ' ') + " , ss:[" +
							per.Scopes.Aggregate("", (str, actual) => str + actual + ",", str => str.Trim(',')) +
							"]");
					}
					break;
				case PrintMode.PropertyList:
					break;
				case PrintMode.Table:
					string GetValue(IPermission permission) => permission?.Value.ToString();
					string GetFunction(IPermission permission) => permission?.Function?.ToString() ?? "null";
					string GetDiscriminatorTypeId(IScope scope) => scope.Discriminator?.TypeId?.ToString() ?? "null";
					string GetDiscriminatorTypeName(IScope scope) => scope.Discriminator?.TypeName?.ToString() ?? "null";
					string GetDiscriminatorId(IScope scope) => scope.Discriminator?.Id?.ToString() ?? "null";
					string GetDiscriminatorName(IScope scope) => scope.Discriminator?.Name?.ToString() ?? "null";
					string GetScopePropagation(IScope scope) => scope?.Propagation.ToString() ?? "null";

					var maxValue = me.Select(p => GetValue(p).Length).Union(new[] { "VALUE".Length }).Max();
					var maxFunction = me.Select(p => GetFunction(p).Length).Union(new[] { "FUNCTION".Length }).Max();
					var maxDiscriminatorTypeId = me.SelectMany(p => p.Scopes.Select(s => GetDiscriminatorTypeId(s).Length)).Union(new[] { "TYPE_ID".Length }).Max();
					var maxDiscriminatorTypeName = me.SelectMany(p => p.Scopes.Select(s => GetDiscriminatorTypeName(s).Length)).Union(new[] { "TYPE_NAME".Length }).Max();
					var maxDiscriminatorId = me.SelectMany(p => p.Scopes.Select(s => GetDiscriminatorId(s).Length)).Union(new[] { "ID".Length }).Max();
					var maxDiscriminatorName = me.SelectMany(p => p.Scopes.Select(s => GetDiscriminatorName(s).Length)).Union(new[] { "NAME".Length }).Max();
					var maxScopePropagation = me.SelectMany(p => p.Scopes.Select(s => GetScopePropagation(s).Length)).Union(new[] { "PROPAGATION".Length }).Max();

					// Headers
					Printer.WriteLine("┌" + ("".PadRight(maxValue, '─')) + "┬" + ("".PadRight(maxFunction, '─')) + "╥" + ("".PadRight(maxDiscriminatorTypeId, '─')) + "┬" + ("".PadRight(maxDiscriminatorTypeName, '─')) + "┬" + ("".PadRight(maxDiscriminatorId, '─')) + "┬" + ("".PadRight(maxDiscriminatorName, '─')) + "┬" + ("".PadRight(maxScopePropagation, '─')) + "┐");
					if (me.Any())
					{
						Printer.WriteLine("│" + ("VALUE".PadRight(maxValue, ' ')) + "│" + ("FUNCTION".PadRight(maxFunction, ' ')) + "║" + ("TYPE_ID".PadRight(maxDiscriminatorTypeId, ' ')) + "│" + ("TYPE_NAME".PadRight(maxDiscriminatorTypeName, ' ')) + "│" + ("ID".PadRight(maxDiscriminatorId, ' ')) + "│" + ("NAME".PadRight(maxDiscriminatorName, ' ')) + "│" + ("PROPAGATION".PadRight(maxScopePropagation, ' ')) + "│");
						Printer.WriteLine("├" + ("".PadRight(maxValue, '─')) + "┼" + ("".PadRight(maxFunction, '─')) + "╫" + ("".PadRight(maxDiscriminatorTypeId, '─')) + "┼" + ("".PadRight(maxDiscriminatorTypeName, '─')) + "┼" + ("".PadRight(maxDiscriminatorId, '─')) + "┼" + ("".PadRight(maxDiscriminatorName, '─')) + "┼" + ("".PadRight(maxScopePropagation, '─')) + "┤");
					}

					// Body
					foreach (var per in me)
					{
						var list = per.Scopes.ToList();
						if (list.Count == 0)
						{
							Printer.WriteLine("│" +
									GetValue(per).PadRight(maxValue, ' ') + "│" +
									GetFunction(per).PadRight(maxFunction, ' ') + "║" +
									("".PadRight(maxDiscriminatorTypeId, ' ')) + "│" +
									("".PadRight(maxDiscriminatorTypeName, ' ')) + "│" +
									("".PadRight(maxDiscriminatorId, ' ')) + "│" +
									("".PadRight(maxDiscriminatorName, ' ')) + "│" +
									("".PadRight(maxScopePropagation, ' ')) + "│");
						}
						else
						{
							for (var i = 0; i < list.Count; i++)
							{
								Printer.WriteLine("│" +
									((i == 0 ? GetValue(per) : "").PadRight(maxValue, ' ')) + "│" +
									((i == 0 ? GetFunction(per) : "").PadRight(maxFunction, ' ')) + "║" +
									(GetDiscriminatorTypeId(list[i]).PadRight(maxDiscriminatorTypeId, ' ')) + "│" +
									(GetDiscriminatorTypeName(list[i]).PadRight(maxDiscriminatorTypeName, ' ')) + "│" +
									(GetDiscriminatorId(list[i]).PadRight(maxDiscriminatorId, ' ')) + "│" +
									(GetDiscriminatorName(list[i]).PadRight(maxDiscriminatorName, ' ')) + "│" +
									(GetScopePropagation(list[i]).PadRight(maxScopePropagation, ' ')) + "│");
							}
						}
					}

					// Footer
					Printer.WriteLine("└" + ("".PadRight(maxValue, '─')) + "┴" + ("".PadRight(maxFunction, '─')) + "╨" + ("".PadRight(maxDiscriminatorTypeId, '─')) + "┴" + ("".PadRight(maxDiscriminatorTypeName, '─')) + "┴" + ("".PadRight(maxDiscriminatorId, '─')) + "┴" + ("".PadRight(maxDiscriminatorName, '─')) + "┴" + ("".PadRight(maxScopePropagation, '─')) + "┘");
					break;
			}
		}
	}
}
