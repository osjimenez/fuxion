using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace System.ComponentModel.DataAnnotations
{
    public class NullableEmailAddress : DataTypeAttribute
    {
		public NullableEmailAddress() : base(DataType.EmailAddress) { }
		public override bool IsValid(object value)
		{
			if (value == null)
			{
				return true;
			}
			var input = value as string;
			var emailAddressAttribute = new EmailAddressAttribute();
			return (input != null) && (string.IsNullOrEmpty(input) || emailAddressAttribute.IsValid(input));
		}
	}
}
