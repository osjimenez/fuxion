namespace Fuxion.Licensing;

using System.Diagnostics.CodeAnalysis;

public abstract class License
{
	protected internal virtual bool Validate([NotNull] out string validationMessage)
	{
		validationMessage = "Success";
		return true;
	}
	public DateTime SignatureUtcTime { get; set; }
}