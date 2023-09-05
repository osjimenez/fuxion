using System.Collections;

namespace System.ComponentModel.DataAnnotations;

public class EnsureMinimumElementsAttribute : ValidationAttribute
{
	public EnsureMinimumElementsAttribute(int minElements) => _minElements = minElements;
	readonly int _minElements;
	public override string FormatErrorMessage(string name) => string.Format(ErrorMessageString, name, _minElements);
	public override bool IsValid(object? value)
	{
		if (value is IList list) return list.Count >= _minElements;
		return false;
	}
}