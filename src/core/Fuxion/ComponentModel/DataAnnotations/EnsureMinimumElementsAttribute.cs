using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace System.ComponentModel.DataAnnotations
{
	public class EnsureMinimumElementsAttribute : ValidationAttribute
	{
		private readonly int _minElements;
		public EnsureMinimumElementsAttribute(int minElements)
		{
			_minElements = minElements;
		}
		public override string FormatErrorMessage(string name)
		{
			return string.Format(ErrorMessageString, name, _minElements);
		}
		public override bool IsValid(object? value)
		{
			if (value is IList list)
			{
				return list.Count >= _minElements;
			}
			return false;
		}
	}
}
