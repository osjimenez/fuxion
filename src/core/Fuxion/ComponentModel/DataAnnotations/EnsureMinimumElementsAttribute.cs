namespace System.ComponentModel.DataAnnotations;

using System.Collections;

public class EnsureMinimumElementsAttribute : ValidationAttribute
{
	private readonly int _minElements;
	public EnsureMinimumElementsAttribute(int minElements) => _minElements = minElements;
	public override string FormatErrorMessage(string name) => string.Format(ErrorMessageString, name, _minElements);
	public override bool IsValid(object? value)
	{
		if (value is IList list)
		{
			return list.Count >= _minElements;
		}
		return false;
	}
}