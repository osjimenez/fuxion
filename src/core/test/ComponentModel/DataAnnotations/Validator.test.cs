using Fuxion.ComponentModel;
using Fuxion.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        private void PrintValidatorResults(IEnumerable<NotifierValidatorMessage> res)
        {
            Output.WriteLine("Printing validator entries:");
            foreach (var r in res)
                Output.WriteLine($"   - {r.Path} - {r.PropertyName} - {r.Message}");
        }
        [Fact(DisplayName = "Validator - Default values are valid")]
        public void DefaultIsValid()
        {
            var obj = new ValidatableMock();
            var val = new NotifierValidator();
            var res = val.Validate(obj);
            PrintValidatorResults(res);
            Assert.Empty(res);
        }
        [Fact(DisplayName = "Validator - Manual validation")]
        public void ManualValidation()
        {
            var obj = new ValidatableMock();
            var val = new NotifierValidator();
            obj.Id = 0; // Make invalid because must be greater than 0
            obj.Name = "Fuxion678901"; // Make doubly invalid because contains 'Oscar' and has more than 10 character length
            obj.RecursiveValidatable = null; // Make invalid because is required

            var res = val.Validate(obj);
            PrintValidatorResults(res);
            Assert.Equal(4, res.Count());
            Assert.Equal(1, res.Count(r => string.IsNullOrEmpty(r.Path) && r.PropertyName == nameof(obj.Id)));
            Assert.Equal(2, res.Count(r => string.IsNullOrEmpty(r.Path) && r.PropertyName == nameof(obj.Name)));
            Assert.Equal(1, res.Count(r => string.IsNullOrEmpty(r.Path) && r.PropertyName == nameof(obj.RecursiveValidatable)));

            res = val.Validate(obj, nameof(obj.Name));
            Assert.Equal(2, res.Count());
        }
        [Fact(DisplayName = "Validator - Manual recursive validatable")]
        public void ManualRecursiveValidatable()
        {
            var obj = new ValidatableMock();
            var val = new NotifierValidator();
            obj.RecursiveValidatable.Id = 0; // Make invalid because must be greater than 0

            var res = val.Validate(obj);
            PrintValidatorResults(res);
            Assert.Equal(1, res.Count(r => r.Path == $"{nameof(obj.RecursiveValidatable)}" && r.PropertyName == nameof(obj.Id)));
        }
        [Fact(DisplayName = "Validator - Manual recursive validatable collection")]
        public void ManualRecursiveValidatableCollection()
        {
            var obj = new ValidatableMock();
            var val = new NotifierValidator();
            var first = obj.RecursiveValidatableCollection.First();
            first.Id = 0; // Make invalid because must be greater than 0

            var res = val.Validate(obj);
            PrintValidatorResults(res);
            Assert.Single(res);
            Assert.Equal(1, res.Count(r => r.Path == $"{nameof(obj.RecursiveValidatableCollection)}[{first}]" && r.PropertyName == nameof(obj.Id)));
        }

        [Fact(DisplayName = "Validator - Automatic validation")]
        public void AutomaticValidation()
        {
            var obj = new ValidatableMock();
            var val = new NotifierValidator();
            var counter = 0;
            ((INotifyCollectionChanged)val.Messages).CollectionChanged += (s, e) => counter++;

            obj.Id = 0; // Make invalid because must be greater than 0

            val.RegisterNotifier(obj);
            PrintValidatorResults(val.Messages);

            Assert.Equal(1, counter);
            Assert.Single(val.Messages);
            Assert.Equal(1, val.Messages.Count(r => string.IsNullOrEmpty(r.Path) && r.PropertyName == nameof(obj.Id)));

            obj.Name = "Fuxion789.12"; // Make doubly invalid because contains 'Fuxion' and has more than 10 character length
            PrintValidatorResults(val.Messages);
            Assert.Equal(3, counter);
            Assert.Equal(3, val.Messages.Count);
            Assert.Equal(2, val.Messages.Count(r => string.IsNullOrEmpty(r.Path) && r.PropertyName == nameof(obj.Name)));

            obj.Id = 1; // Make valid because is greater than 0
            PrintValidatorResults(val.Messages);
            Assert.DoesNotContain(val.Messages, r => string.IsNullOrEmpty(r.Path) && r.PropertyName == nameof(obj.Id));
            Assert.Equal(4, counter);
            Assert.Equal(2, val.Messages.Count);

            val.UnregisterNotifier(obj);
            Assert.Empty(val.Messages);
        }
        [Fact(DisplayName = "Validator - Automatic conditional validation")]
        public void AutomaticConditionalValidation()
        {
            var obj = new ValidatableMock();
            var val = new NotifierValidator();
            var counter = 0;
            ((INotifyCollectionChanged)val.Messages).CollectionChanged += (s, e) => counter++;

            obj.Id = 0; // Make invalid because must be greater than 0
            obj.Name = "Fuxion789.12"; // Make doubly invalid because contains 'Fuxion' and has more than 10 character length

            val.RegisterNotifier(obj);
            
            PrintValidatorResults(val.Messages);
            Assert.Equal(3, counter);
            Assert.Equal(3, val.Messages.Count);
            Assert.Equal(2, val.Messages.Count(r => string.IsNullOrEmpty(r.Path) && r.PropertyName == nameof(obj.Name)));

            obj.IsValid = false; // Disable all validations
            
            PrintValidatorResults(val.Messages);
            Assert.DoesNotContain(val.Messages, r => string.IsNullOrEmpty(r.Path) && r.PropertyName == nameof(obj.Id));
            Assert.DoesNotContain(val.Messages, r => string.IsNullOrEmpty(r.Path) && r.PropertyName == nameof(obj.Name));
            Assert.Equal(6, counter);
            Assert.Empty(val.Messages);
        }
        [Fact(DisplayName = "Validator - Automatic recursive validatable")]
        public void AutomaticRecursiveValidatable()
        {
            var obj = new ValidatableMock();
            var val = new NotifierValidator();
            var counter = 0;
            ((INotifyCollectionChanged)val.Messages).CollectionChanged += (s, e) => counter++;

            obj.RecursiveValidatable.Id = 0; // Make invalid because must be greater than 0

            val.RegisterNotifier(obj);
            PrintValidatorResults(val.Messages);
            Assert.Equal(1, counter);
            Assert.Single(val.Messages);
            Assert.Equal(1, val.Messages.Count(r => r.Path == $"{nameof(obj.RecursiveValidatable)}" && r.PropertyName == nameof(obj.Id)));

            obj.RecursiveValidatable.Name = "Fuxion678901"; // Make invalid because has more than 10 character length
            PrintValidatorResults(val.Messages);
            Assert.Equal(2, counter);
            Assert.Equal(2, val.Messages.Count);
            Assert.Equal(1, val.Messages.Count(r => r.Path == $"{nameof(obj.RecursiveValidatable)}" && r.PropertyName == nameof(obj.Name)));

            obj.RecursiveValidatable.Id = 1;
            PrintValidatorResults(val.Messages);
            Assert.Equal(3, counter);
            Assert.Single(val.Messages);
            Assert.Equal(0, val.Messages.Count(r => r.Path == $"{nameof(obj.RecursiveValidatable)}" && r.PropertyName == nameof(obj.Id)));

            obj.RecursiveValidatable.Name = "Valid";
            PrintValidatorResults(val.Messages);
            Assert.Equal(4, counter);
            Assert.Empty(val.Messages);
            Assert.Equal(0, val.Messages.Count(r => r.Path == $"{nameof(obj.RecursiveValidatable)}" && r.PropertyName == nameof(obj.Name)));

            val.UnregisterNotifier(obj.RecursiveValidatable);
            Assert.Empty(val.Messages);
        }
        [Fact(DisplayName = "Validator - Automatic recursive validatable null")]
        public void AutomaticRecursiveValidatableNull()
        {
            var obj = new ValidatableMock();
            var val = new NotifierValidator();
            var counter = 0;
            ((INotifyCollectionChanged)val.Messages).CollectionChanged += (s, e) => counter++;

            obj.RecursiveValidatable = null; // Make invalid because is required

            val.RegisterNotifier(obj);
            PrintValidatorResults(val.Messages);
            Assert.Equal(1, counter);
            Assert.Single(val.Messages);
            Assert.Equal(1, val.Messages.Count(r => r.Path == $"" && r.PropertyName == nameof(obj.RecursiveValidatable)));

            obj.RecursiveValidatable = new RecursiveValidatableMock();

            PrintValidatorResults(val.Messages);
            Assert.Equal(2, counter);
            Assert.Empty(val.Messages);
            Assert.Equal(0, val.Messages.Count(r => r.Path == $"" && r.PropertyName == nameof(obj.RecursiveValidatable)));

            obj.RecursiveValidatable.Id = -1;
            Assert.Equal(3, counter);
            Assert.Single(val.Messages);
            Assert.Equal(1, val.Messages.Count(r => r.Path == $"{nameof(obj.RecursiveValidatable)}" && r.PropertyName == nameof(obj.Id)));

            val.UnregisterNotifier(obj.RecursiveValidatable);
            Assert.Empty(val.Messages);
        }
        [Fact(DisplayName = "Validator - Automatic recursive validatable collection")]
        public void AutomaticRecusiveValidatableCollection()
        {
            var obj = new ValidatableMock();
            var val = new NotifierValidator();
            val.RegisterNotifier(obj);
            var counter = 0;
            ((INotifyCollectionChanged)val.Messages).CollectionChanged += (s, e) => counter++;
            var first = obj.RecursiveValidatableCollection.First();

            first.Id = 0; // Make invalid because must be greater than 0
            PrintValidatorResults(val.Messages);
            Assert.Equal(1, counter);
            Assert.Single(val.Messages);
            Assert.Equal(1, val.Messages.Count(r => r.Path == $"{nameof(obj.RecursiveValidatableCollection)}[{first}]" && r.PropertyName == nameof(obj.Id)));

            first.Name = "Fuxion678901"; // Make invalid because has more than 10 character length
            PrintValidatorResults(val.Messages);
            Assert.Equal(2, counter);
            Assert.Equal(2, val.Messages.Count);
            Assert.Equal(1, val.Messages.Count(r => r.Path == $"{nameof(obj.RecursiveValidatableCollection)}[{first}]" && r.PropertyName == nameof(obj.Name)));

            first.Id = 1; // Make valid
            Assert.Equal(3, counter);
            Assert.Single(val.Messages);
            Assert.Equal(0, val.Messages.Count(r => r.Path == $"{nameof(obj.RecursiveValidatableCollection)}[{first}]" && r.PropertyName == nameof(obj.Id)));
            // When Id is valid again, and change to 0, the entry for name must change its Path for new first.ToString result
            Assert.Equal(1, val.Messages.Count(r => r.Path == $"{nameof(obj.RecursiveValidatableCollection)}[{first}]" && r.PropertyName == nameof(obj.Name)));

            var added = new RecursiveValidatableMock
            {
                Id = 1,
                Name = "Valid"
            };
            obj.RecursiveValidatableCollection.Add(added);

            added.Id = -1; // Make invalid because must be greater than 0
            PrintValidatorResults(val.Messages);
            Assert.Equal(4, counter);
            Assert.Equal(2, val.Messages.Count);
            Assert.Equal(1, val.Messages.Count(r => r.Path == $"{nameof(obj.RecursiveValidatableCollection)}[{added}]" && r.PropertyName == nameof(obj.Id)));

            obj.RecursiveValidatableCollection.Remove(added);
            PrintValidatorResults(val.Messages);
            Assert.Equal(0, val.Messages.Count(r => r.Path == $"{nameof(obj.RecursiveValidatableCollection)}[{added}]" && r.PropertyName == nameof(obj.Id)));

            added.Name = "Fuxion678901"; // Make invalid because has more than 10 character length
            PrintValidatorResults(val.Messages);
            Assert.Equal(5, counter);
            Assert.Single(val.Messages);
            Assert.Equal(0, val.Messages.Count(r => r.Path == $"{nameof(obj.RecursiveValidatableCollection)}[{added}]" && r.PropertyName == nameof(obj.Name)));

            val.UnregisterNotifier(first);
            Assert.Empty(val.Messages);
        }
        [Fact(DisplayName = "Validator - Automatic recursive validatable collection ensure elements")]
        public void AutomaticRecusiveValidatableCollectionEnsureElements()
        {
            var obj = new ValidatableMock();
            var val = new NotifierValidator();
            val.RegisterNotifier(obj);
            var counter = 0;
            ((INotifyCollectionChanged)val.Messages).CollectionChanged += (s, e) => counter++;
            var first = obj.RecursiveValidatableCollection.First();

            obj.RecursiveValidatableCollection.Remove(first);

            Assert.Equal(1, counter);
            Assert.Single(val.Messages);
            Assert.Equal(1, val.Messages.Count(r => r.Path == $"" && r.PropertyName == nameof(obj.RecursiveValidatableCollection)));

            var added = new RecursiveValidatableMock();
            obj.RecursiveValidatableCollection.Add(added);

            Assert.Equal(2, counter);
            Assert.Empty(val.Messages);
            Assert.Equal(0, val.Messages.Count(r => r.Path == $"" && r.PropertyName == nameof(obj.RecursiveValidatableCollection)));
        }
        [Fact(DisplayName = "Validator - Automatic recursive validatable collection null")]
        public void AutomaticRecusiveValidatableCollectionNull()
        {
            var obj = new ValidatableMock();
            var val = new NotifierValidator();
            val.RegisterNotifier(obj);
            var counter = 0;
            ((INotifyCollectionChanged)val.Messages).CollectionChanged += (s, e) => counter++;

            obj.RecursiveValidatableCollection = null;

            PrintValidatorResults(val.Messages);
            Assert.Equal(1, counter);
            Assert.Single(val.Messages);
            Assert.Equal(1, val.Messages.Count(r => r.Path == $"" && r.PropertyName == nameof(obj.RecursiveValidatableCollection)));

            obj.RecursiveValidatableCollection = new ObservableCollection<RecursiveValidatableMock>();
            PrintValidatorResults(val.Messages);
            var added = new RecursiveValidatableMock();
            obj.RecursiveValidatableCollection.Add(added);

            PrintValidatorResults(val.Messages);
            Assert.Equal(2, counter);
            Assert.Empty(val.Messages);
            Assert.Equal(0, val.Messages.Count(r => r.Path == $"" && r.PropertyName == nameof(obj.RecursiveValidatableCollection)));

            added.Id = -1;

            PrintValidatorResults(val.Messages);
            Assert.Equal(3, counter);
            Assert.Single(val.Messages);
            Assert.Equal(1, val.Messages.Count(r => r.Path == $"{nameof(obj.RecursiveValidatableCollection)}[{added}]" && r.PropertyName == nameof(obj.Id)));
        }

        [Fact(DisplayName = "Validator - Custom validation")]
        public void CustomValidation()
        {
            //var obj = new ValidatableMock();
            var val = new NotifierValidator();

            var dis = val.AddCustom("Test for custom validations", "Path", "PropertyName", "PropertyDisplayName");

            Assert.Single(val.Messages);
            Assert.Equal(1, val.Messages.Count(r => r.Path == "Path" && r.PropertyName == "PropertyName" && r.PropertyDisplayName == "PropertyDisplayName"));

            dis.Dispose();

            Assert.Empty(val.Messages);


            //obj.Id = 0; // Make invalid because must be greater than 0
            //obj.Name = "Fuxion678901"; // Make doubly invalid because contains 'Oscar' and has more than 10 character length
            //obj.RecursiveValidatable = null; // Make invalid because is required

            //var res = val.Validate(obj);
            //PrintValidatorResults(res);
            //Assert.Equal(4, res.Count());
            //Assert.Equal(1, res.Count(r => string.IsNullOrEmpty(r.Path) && r.PropertyName == nameof(obj.Id)));
            //Assert.Equal(2, res.Count(r => string.IsNullOrEmpty(r.Path) && r.PropertyName == nameof(obj.Name)));
            //Assert.Equal(1, res.Count(r => string.IsNullOrEmpty(r.Path) && r.PropertyName == nameof(obj.RecursiveValidatable)));

            //res = val.Validate(obj, nameof(obj.Name));
            //Assert.Equal(2, res.Count());
        }
    }
}