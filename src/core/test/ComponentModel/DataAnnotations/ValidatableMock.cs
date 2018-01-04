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
    public class ValidatableMock : Notifier<ValidatableMock>
    {
        // The validation attributes for 'Id' are in a metadata type
        public int Id
        {
            get => GetValue(() => 1);
            set => SetValue(value);
        }
        [Required(ErrorMessage = "Name is required")]
        [StringLength(10, ErrorMessage = "Name length must be at maximum 10")]
        [CustomValidation(typeof(ValidatableMock), nameof(ValidatableMock.ValidateIfNameContainsOscar))]
        public string Name
        {
            get => GetValue(() => "Valid");
            set => SetValue(value);
        }
        public static ValidationResult ValidateIfNameContainsOscar(string value)
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
        [RecursiveValidation]
        [EnsureMinimumElements(1, ErrorMessage = "At least one element in collection")]
        public ObservableCollection<RecursiveValidatableMock> RecursiveValidatableCollection
        {
            get => GetValue(() => new ObservableCollection<RecursiveValidatableMock>(new[] { new RecursiveValidatableMock() }));
            set => SetValue(value);
        }

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
        [Range(1, int.MaxValue, ErrorMessage = "Id must be greater than 0")]
        public int Id
        {
            get => GetValue(() => 1);
            set => SetValue(value);
        }
        [Required(ErrorMessage = "Name is required")]
        [StringLength(10, ErrorMessage = "Name length must be at maximum 10")]
        public string Name
        {
            get => GetValue(() => "Valid");
            set => SetValue(value);
        }

        public override string ToString() => $"RecursiveValidatableMock({Id.ToString()})";
    }
}
