using System.Diagnostics.CodeAnalysis;

namespace Fuxion.Licensing;

public abstract class License
{
	public DateTime SignatureUtcTime { get; set; }
	protected internal virtual bool Validate(out string validationMessage)
	{
		validationMessage = "Success";
		return true;
	}
}