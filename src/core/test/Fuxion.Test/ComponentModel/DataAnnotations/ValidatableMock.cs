using Fuxion.ComponentModel;
using Fuxion.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Test.ComponentModel.DataAnnotations
{
	[MetadataType(typeof(ValidatableMockMetadata))]
	[ConditionalValidation(typeof(ValidatableMock), nameof(MustBeValidate))]
	public class ValidatableMock : Notifier<ValidatableMock>
	{
		[Required(ErrorMessage = "Id is required")]
		[Range(1, int.MaxValue, ErrorMessage = "Id must be greater than 0")]
		// The validation attributes for 'Id' are in a metadata type
		public int Id
		{
			get => GetValue(() => 1);
			set => SetValue(value);
		}
		[Display(Name = "Name")]
		[Required(ErrorMessage = "Name is required")]
		[StringLength(10, ErrorMessage = "Name length must be at maximum 10")]
		[CustomValidation(typeof(ValidatableMock), nameof(ValidatableMock.ValidateIfNameContainsFuxion))]
		public string Name
		{
			get => GetValue(() => "Valid");
			set => SetValue(value);
		}
		[Display(Name = "IgnoredName")]
		[Required(ErrorMessage = "IgnoredName is required")]
		[StringLength(10, ErrorMessage = "IgnoredName length must be at maximum 10")]
		[CustomValidation(typeof(ValidatableMock), nameof(ValidatableMock.ValidateIfNameContainsFuxion))]
		[IgnoreValidation]
		public string IgnoredName
		{
			get => GetValue(() => "Valid");
			set => SetValue(value);
		}
		public static ValidationResult? ValidateIfNameContainsFuxion(string value)
		{
			if (value.ToLower().Contains("fuxion"))
				return new ValidationResult("Name cannot contains 'fuxion'");
			return ValidationResult.Success;
		}
		[Required(ErrorMessage = "RecursiveValidatable is mandatory")]
		[RecursiveValidation]
		public RecursiveValidatableMock RecursiveValidatable
		{
			get => GetValue(() => new RecursiveValidatableMock());
			set => SetValue(value);
		}
		[Required(ErrorMessage = "IgnoredRecursiveValidatable is mandatory")]
		[RecursiveValidation]
		[IgnoreValidation]
		public RecursiveValidatableMock IgnoredRecursiveValidatable
		{
			get => GetValue(() => new RecursiveValidatableMock());
			set => SetValue(value);
		}
		[RecursiveValidation]
		[EnsureMinimumElements(1, ErrorMessage = "At least one element in collection")]
		public ObservableCollection<RecursiveValidatableMock> RecursiveValidatableCollection
		{
			get => GetValue(() => new ObservableCollection<RecursiveValidatableMock>(new[] { new RecursiveValidatableMock() }));
			set => SetValue(value);
		}
		[RecursiveValidation]
		[EnsureMinimumElements(1, ErrorMessage = "At least one element in collection")]
		[IgnoreValidation]
		public ObservableCollection<RecursiveValidatableMock> IgnoredRecursiveValidatableCollection
		{
			get => GetValue(() => new ObservableCollection<RecursiveValidatableMock>(new[] { new RecursiveValidatableMock() }));
			set => SetValue(value);
		}
		public bool IsValid
		{
			get => GetValue(() => true);
			set => SetValue(value);
		}
		public bool MustBeValidate() => IsValid;
		public override string ToString() => $"ValidatableMock({Id})";
	}
	public class ValidatableMockMetadata
    {
        [Required(ErrorMessage = "Id is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Id must be greater than 0")]
        public int Id { get; set; }
    }
	public class RecursiveValidatableMock : Notifier<RecursiveValidatableMock>
	{
		[Required]
		public Guid Key
		{
			get => GetValue(() => Guid.NewGuid());
			set => SetValue(value);
		}
		[Required]
		[Range(1, int.MaxValue, ErrorMessage = "Id must be greater than 0")]
		public int Id
		{
			get => GetValue(() => 1);
			set => SetValue(value);
		}
		[Display(Name = "Nombre")]
		[Required(ErrorMessage = "Name is required")]
		[StringLength(10, ErrorMessage = "Name length must be at maximum 10")]
		public string Name
		{
			get => GetValue(() => "Valid");
			set => SetValue(value);
		}

		public override string ToString() => $"RecursiveValidatableMock({Id.ToString()})";

		public override bool Equals(object? obj)
		{
			if (obj is RecursiveValidatableMock mock)
				return Compare(mock, this);
			return base.Equals(obj);
		}
		public override int GetHashCode() => Key.GetHashCode();
		static bool Compare(RecursiveValidatableMock item1, RecursiveValidatableMock item2)
		{
			//if (ReferenceEquals(item1, null) && ReferenceEquals(item2, null)) return true;
			//if (ReferenceEquals(item1, null) && !ReferenceEquals(item2, null)) return false;
			//if (!ReferenceEquals(item1, null) && ReferenceEquals(item2, null)) return false;
			if (item1 is null && item2 is null) return true;
			if (item1 is null && item2 is object) return false;
			if (item1 is object && item2 is null) return false;
			return item1?.Key == item2?.Key;
		}
		public static bool operator ==(RecursiveValidatableMock dep1, RecursiveValidatableMock dep2) => Compare(dep1, dep2);
		public static bool operator !=(RecursiveValidatableMock dep1, RecursiveValidatableMock dep2) => !Compare(dep1, dep2);
	}
}
