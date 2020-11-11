using Fuxion.Identity.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fuxion.Identity
{
	public interface IDiscriminator : IInclusive<IDiscriminator>, IExclusive<IDiscriminator>
	{
		object TypeKey { get; }
		string TypeName { get; }
		object? Id { get; }
		string? Name { get; }
	}
	public interface IDiscriminator<TId, TTypeId> : IDiscriminator, IInclusive<IDiscriminator<TId, TTypeId>>, IExclusive<IDiscriminator<TId, TTypeId>>
	{
		new TTypeId TypeKey { get; }
		new TId Id { get; }
	}
	public class Discriminator : IDiscriminator
	{
		private Discriminator(object typeKey, string typeName)
		{
			TypeKey = typeKey;
			TypeName = typeName;
		}
		public object TypeKey { get; private set; }

		public string TypeName { get; private set; }

		public object? Id { get; private set; }

		public string? Name { get; private set; }

		public override string ToString() => this.ToOneLineString();

		public IEnumerable<IDiscriminator> Inclusions => throw new NotImplementedException();

		public IEnumerable<IDiscriminator> Exclusions => throw new NotImplementedException();

		public static IDiscriminator Empty<TDiscriminator>() => Empty(typeof(TDiscriminator));
		public static IDiscriminator Empty(Type type)
		{
			var att = type.GetTypeInfo().GetCustomAttribute<DiscriminatorAttribute>();
			if (att != null)
				return new Discriminator(att.TypeKey, type.Name);
			throw new ArgumentException($"The type '{type.Name}' isn't adorned with Discriminator attribute");
		}
		internal static IDiscriminator ForId(Type type, object id) => ((Discriminator)Empty(type)).Transform(d =>
			{
				d.Id = id;
				d.Name = id?.ToString();
			});
	}
	public static class DiscriminatorExtensions
	{
		public static string ToOneLineString(this IDiscriminator me) => $"{me.TypeName} ({(string.IsNullOrWhiteSpace(me.Name?.ToString()) ? "null" : me.Name)})";
		public static bool IsValid(this IDiscriminator me) => !Comparer.AreEquals(me.TypeKey, me.TypeKey?.GetType().GetDefaultValue())
				&& !string.IsNullOrWhiteSpace(me.TypeName);
		public static void Print(this IEnumerable<IDiscriminator> me, PrintMode mode)
		{
			switch (mode)
			{
				case PrintMode.OneLine:
					break;
				case PrintMode.PropertyList:
					break;
				case PrintMode.Table:
					var maxTypeId = me.Select(s => s.TypeKey.ToString()?.Length ?? 0).Union(new[] { "TYPE_ID".Length }).Max();
					var maxTypeName = me.Select(s => s.TypeName.Length).Union(new[] { "TYPE_NAME".Length }).Max();
					var maxId = me.Select(s => s.Id?.ToString()?.Length ?? 0).Cast<int>().Union(new[] { "ID".Length, "null".Length }).Max();
					var maxName = me.Select(s => s.Name?.Length).RemoveNulls().Cast<int>().Union(new[] { "NAME".Length, "null".Length }).Max();

					Printer.WriteLine("┌" + ("".PadRight(maxTypeId, '─')) + "┬" + ("".PadRight(maxTypeName, '─')) + "┬" + ("".PadRight(maxId, '─')) + "┬" + ("".PadRight(maxName, '─')) + "┐");
					Printer.WriteLine("│" + "TYPE_ID".PadRight(maxTypeId, ' ') + "│" + "TYPE_NAME".PadRight(maxTypeName, ' ') + "│" + "ID".PadRight(maxId, ' ') + "│" + "NAME".PadRight(maxName, ' ') + "│");
					Printer.WriteLine("├" + ("".PadRight(maxTypeId, '─')) + "┼" + ("".PadRight(maxTypeName, '─')) + "┼" + ("".PadRight(maxId, '─')) + "┼" + ("".PadRight(maxName, '─')) + "┤");
					foreach (var sco in me)
						Printer.WriteLine("│" + (sco.TypeKey.ToString() ?? "").PadRight(maxTypeId, ' ') + "│" + sco.TypeName.PadRight(maxTypeName, ' ') + "│" + (sco.Id?.ToString() ?? "null").PadRight(maxId, ' ') + "│" + (sco.Name ?? "null").PadRight(maxName, ' ') + "│");
					Printer.WriteLine("└" + ("".PadRight(maxTypeId, '─')) + "┴" + ("".PadRight(maxTypeName, '─')) + "┴" + ("".PadRight(maxId, '─')) + "┴" + ("".PadRight(maxName, '─')) + "┘");
					break;
			}
		}
	}
}