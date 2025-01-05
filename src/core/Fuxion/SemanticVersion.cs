using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Fuxion;

// SPEC: https://semver.org/#semantic-versioning-specification-semver
public partial class SemanticVersion : IComparable, IComparable<SemanticVersion>, IEquatable<SemanticVersion>
{
	public SemanticVersion(string semanticVersion)
	{
		this.semanticVersion = semanticVersion;
		var m = SemanticVersionRegex().Match(semanticVersion);
		if (!m.Success) throw new SemanticVersionException($"String '{semanticVersion}' isn't a valid semantic version pattern");
		var major = m.Groups["major"];
		var minor = m.Groups["minor"];
		var patch = m.Groups["patch"];
		var preRelease = m.Groups["prerelease"];
		var buildMetadata = m.Groups["buildmetadata"];
		Major = uint.Parse(major.Value);
		Minor = uint.Parse(minor.Value);
		Patch = uint.Parse(patch.Value);
		PreRelease = string.IsNullOrWhiteSpace(preRelease.Value)
			? new([])
			: new(preRelease.Value.Split('.')
				.Select(i => new SemanticVersionIdentifier(i))
				.ToArray());
		BuildMetadata = string.IsNullOrWhiteSpace(buildMetadata.Value)
			? new([])
			: new(buildMetadata.Value.Split('.')
				.Select(i => new SemanticVersionIdentifier(i))
				.ToArray());
	}
	readonly string semanticVersion;
	public uint Major { get; } // Must be non negative integer
	public uint Minor { get; } // Must be non negative integer
	public uint Patch { get; } // Must be non negative integer
	public SemanticVersionIdentifierCollection PreRelease { get; } // A series of dot separated identifiers. Identifiers MUST comprise only ASCII alphanumerics and hyphens [0-9A-Za-z-]
	public SemanticVersionIdentifierCollection BuildMetadata { get; } // A series of dot separated identifiers. Identifiers MUST comprise only ASCII alphanumerics and hyphens [0-9A-Za-z-]
	public int CompareTo(object? obj)
	{
		if (obj is null) return 1;
		var other = obj as SemanticVersion ?? throw new ArgumentException($"Type must be '{nameof(SemanticVersion)}'", "obj");
		return CompareTo(other);
	}
	public int CompareTo(SemanticVersion? other)
	{
		if (ReferenceEquals(this, other)) return 0;
		if (other is null) return 1;
		if (Major > other.Major) return 1;
		if (other.Major > Major) return -1;
		if (Minor > other.Minor) return 1;
		if (other.Minor > Minor) return -1;
		if (Patch > other.Patch) return 1;
		if (other.Patch > Patch) return -1;
		return (PreRelease.Count, other.PreRelease.Count) switch
		{
			(0, > 0) => 1,
			(> 0, 0) => -1,
			var _ => PreRelease.CompareTo(other.PreRelease)
		};
	}
	public bool Equals(SemanticVersion? other) => other is not null && Major.Equals(other.Major) && Minor.Equals(other.Minor) && Patch.Equals(other.Patch) && PreRelease.Equals(other.PreRelease);
	// Official regex from https://semver.org/#is-there-a-suggested-regular-expression-regex-to-check-a-semver-string
	// Official regex must be adapted to work in c# by removing 'P' before each group name
	// You can try with this regex on https://regex101.com/r/P3smVG/1
	const string RegexPattern = @"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$";
#if NETSTANDARD2_0 || NET462
	internal static Regex SemanticVersionRegex() => new(RegexPattern);
#else
	[GeneratedRegex(RegexPattern)]
	internal static partial Regex SemanticVersionRegex();
#endif
	public override string ToString() => semanticVersion;
	public static bool TryParse(string semanticVersion, [MaybeNullWhen(returnValue: false)] out SemanticVersion result)
	{
		if (SemanticVersionRegex().Match(semanticVersion)
			.Success)
		{
			result = new(semanticVersion);
			return true;
		}
		result = null;
		return false;
	}
	public static bool operator ==(SemanticVersion? version1, SemanticVersion? version2)
	{
		if (version1 is null) return version2 is null;
		return version1.Equals(version2);
	}
	public static bool operator !=(SemanticVersion version1, SemanticVersion version2) => !(version1 == version2);
	public static bool operator <(SemanticVersion version1, SemanticVersion version2)
	{
#if NETSTANDARD2_0 || NET462
		if (version1 is null) throw new ArgumentException(nameof(version1));
#else
		ArgumentNullException.ThrowIfNull(version1);
#endif
		return version1.CompareTo(version2) < 0;
	}
	public static bool operator <=(SemanticVersion version1, SemanticVersion version2) => version1 == version2 || version1 < version2;
	public static bool operator >(SemanticVersion version1, SemanticVersion version2)
	{
#if NETSTANDARD2_0 || NET462
		if (version1 is null) throw new ArgumentException(nameof(version1));
#else
		ArgumentNullException.ThrowIfNull(version1);
#endif
		return version2 < version1;
	}
	public static bool operator >=(SemanticVersion version1, SemanticVersion version2) => version1 == version2 || version1 > version2;
	public override bool Equals(object? obj)
	{
		var collection = obj as SemanticVersion;
		return collection is not null && Equals(collection);
	}
	public override int GetHashCode() => HashCode.Combine(Major, Minor, Patch, PreRelease);
}