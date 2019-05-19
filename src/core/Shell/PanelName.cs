using System;
using System.Linq;

namespace Fuxion.Shell
{
	public struct PanelName
	{
		public PanelName(string name, string? key)
		{
			if (name.Contains(KEY_SEPARATOR)) throw new ArgumentException($"{nameof(Name)} cannot contains key separator '{KEY_SEPARATOR}'.", nameof(name));
			if (key?.Contains(KEY_SEPARATOR) ?? false) throw new ArgumentException($"{nameof(Key)} cannot contains key separator '{KEY_SEPARATOR}'.", nameof(name));
			Name = name;
			Key = key;
		}
		public static PanelName Parse(string panelName)
		{
			var index = panelName.IndexOf(KEY_SEPARATOR);
			if (index == -1)
				return new PanelName(panelName, null);
			else
				return new PanelName(panelName.Substring(0, index), panelName.Substring(index + 1));
		}

		const string KEY_SEPARATOR = "@";
		const string ARGUMENT_SEPARATOR = "#";

		public string Name { get; private set; }
		public string? Key { get; private set; }

		public override int GetHashCode() => Name.GetHashCode();
		public override bool Equals(object obj) => obj is PanelName v && v.Name == Name && v.Key == Key;
		public static bool operator ==(PanelName v1, PanelName v2) => v1.Name == v2.Name && v1.Key == v2.Key;
		public static bool operator !=(PanelName v1, PanelName v2) => v1.Name != v2.Name && v1.Key == v2.Key;

		public override string ToString() => Name + (Key != null ? KEY_SEPARATOR + Key.ToString() : "");
	}
}
