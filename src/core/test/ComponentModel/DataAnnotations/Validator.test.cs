using Fuxion.ComponentModel;
using Fuxion.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test.ComponentModel.DataAnnotations
{
    public class ValidatorTest : BaseTest
    {
        public ValidatorTest(ITestOutputHelper output) : base(output)
        {
        }
        [Fact(DisplayName = "Validator - Default values are valid")]
        public void DefaultIsValid()
        {
            var obj = new ValidatableMock();
            var val = new Validator2();
            var res = val.Validate(obj);
            foreach (var r in res)
                Output.WriteLine(r.Message);
            Assert.Empty(res);
        }
        [Fact(DisplayName = "Validator - Manual validation")]
        public void ManualValidation()
        {
            var obj = new ValidatableMock();
            var val = new Validator2();
            obj.Id = 0; // Make invalid for Id
            obj.Name = "Oscar678901"; // Make invalid for Name
            obj.Sub = null; // Make invalid for Sub

            var res = val.Validate(obj);
            foreach (var r in res)
                Output.WriteLine(r.Message);
            Assert.True(res.Count() == 4);
        }
        [Fact(DisplayName = "Validator - Manual sub-validation")]
        public void ManualSubValidation()
        {
            var obj = new ValidatableMock();
            var val = new Validator2();
            obj.Sub.Id = 0; // Make invalid for Id

            var res = val.Validate(obj);
            foreach (var r in res)
                Output.WriteLine(r.Message);
            Assert.True(res.Count() == 1);
        }
        [Fact(DisplayName = "Validator - Manual sub-validation collection")]
        public void ManualSubValidationCollection()
        {
            var obj = new ValidatableMock();
            var val = new Validator2();
            obj.SubCollection.First().Id = 0; // Make invalid for id

            var res = val.Validate(obj);
            foreach (var r in res)
                Output.WriteLine(r.Message);
            Assert.True(res.Count() == 1);
        }
    }
    //[MetadataType(typeof(ValidatableMockMetadata))]
    public class ValidatableMock : Notifier<ValidatableMock>
    {
        [Range(1, int.MaxValue, ErrorMessage = "Id must be greater than 0")]
        public int Id
        {
            get => GetValue(() => 1);
            set => SetValue(value);
        }
        [Required(ErrorMessage = "Name is required")]
        [StringLength(10,ErrorMessage = "Name length must be at maximum 10")]
        [CustomValidation(typeof(ValidatableMock), nameof(ValidatableMock.ValidateName))]
        public string Name
        {
            get => GetValue(() => "Ogcar");
            set => SetValue(value);
        }
        public static ValidationResult ValidateName(string value)
        {
            if (value.ToLower().Contains("oscar"))
                return new ValidationResult("Name cannot contains 'Oscar'");
            return ValidationResult.Success;
        }
        [Required(ErrorMessage = "Sub is mandatory")]
        [SubValidatable]
        public SubValidatableMock Sub
        {
            get => GetValue(() => new SubValidatableMock());
            set => SetValue(value);
        }
        [SubValidatable]
        public ObservableCollection<SubValidatableMock> SubCollection
        {
            get => GetValue(() => new ObservableCollection<SubValidatableMock>(new[] { new SubValidatableMock() }));
            set => SetValue(value);
        }
    }
    public class ValidatableMockMetadata
    {
        [Required(ErrorMessage = "Id is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Id must be greater than 0")]
        public int Id { get; set; }
    }
    public class SubValidatableMock : Notifier<SubValidatableMock>
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id must be greater than 0 - SUB")]
        public int Id
        {
            get => GetValue(() => 1);
            set => SetValue(value);
        }
        [Required(ErrorMessage = "Name is required - SUB")]
        [StringLength(10, ErrorMessage = "Name length must be at maximum 10 - SUB")]
        public string Name
        {
            get => GetValue(() => "Ogcar");
            set => SetValue(value);
        }
    }
}