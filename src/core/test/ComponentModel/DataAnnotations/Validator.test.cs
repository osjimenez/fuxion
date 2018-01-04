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
        private void PrintValidatorResults(IEnumerable<ValidatorEntry2> res)
        {
            Output.WriteLine("Printing validator entries:");
            foreach (var r in res)
                Output.WriteLine($"   - {r.Path} - {r.PropertyName} - {r.Message}");
        }
        [Fact(DisplayName = "Validator - Default values are valid")]
        public void DefaultIsValid()
        {
            var obj = new ValidatableMock();
            var val = new Validator2();
            var res = val.Validate(obj);
            PrintValidatorResults(res);
            Assert.Empty(res);
        }
        [Fact(DisplayName = "Validator - Manual validation")]
        public void ManualValidation()
        {
            var obj = new ValidatableMock();
            var val = new Validator2();
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
            var val = new Validator2();
            obj.RecursiveValidatable.Id = 0; // Make invalid because must be greater than 0

            var res = val.Validate(obj);
            PrintValidatorResults(res);
            Assert.Equal(1, res.Count(r => r.Path == $"{nameof(obj.RecursiveValidatable)}" && r.PropertyName == nameof(obj.Id)));
        }
        [Fact(DisplayName = "Validator - Manual recursive validatable collection")]
        public void ManualRecursiveValidatableCollection()
        {
            var obj = new ValidatableMock();
            var val = new Validator2();
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
            var val = new Validator2();
            var counter = 0;
            ((INotifyCollectionChanged)val.Entries).CollectionChanged += (s, e) => counter++;

            obj.Id = 0; // Make invalid because must be greater than 0

            val.RegisterNotifier(obj);
            PrintValidatorResults(val.Entries);

            Assert.Equal(1, counter);
            Assert.Single(val.Entries);
            Assert.Equal(1, val.Entries.Count(r => string.IsNullOrEmpty(r.Path) && r.PropertyName == nameof(obj.Id)));

            obj.Name = "Fuxion789.12"; // Make doubly invalid because contains 'Oscar' and has more than 10 character length
            PrintValidatorResults(val.Entries);
            Assert.Equal(3, counter);
            Assert.Equal(3, val.Entries.Count);
            Assert.Equal(2, val.Entries.Count(r => string.IsNullOrEmpty(r.Path) && r.PropertyName == nameof(obj.Name)));

            obj.Id = 1; // Make valid because is greater than 0
            PrintValidatorResults(val.Entries);
            Assert.DoesNotContain(val.Entries, r => string.IsNullOrEmpty(r.Path) && r.PropertyName == nameof(obj.Id));
            Assert.Equal(4, counter);
            Assert.Equal(2, val.Entries.Count);

            val.UnregisterNotifier(obj);
            Assert.Empty(val.Entries);
        }
        [Fact(DisplayName = "Validator - Automatic recursive validatable")]
        public void AutomaticRecursiveValidatable()
        {
            var obj = new ValidatableMock();
            var val = new Validator2();
            var counter = 0;
            ((INotifyCollectionChanged)val.Entries).CollectionChanged += (s, e) => counter++;

            obj.RecursiveValidatable.Id = 0; // Make invalid because must be greater than 0

            val.RegisterNotifier(obj);
            PrintValidatorResults(val.Entries);
            Assert.Equal(1, counter);
            Assert.Single(val.Entries);
            Assert.Equal(1, val.Entries.Count(r => r.Path == $"{nameof(obj.RecursiveValidatable)}" && r.PropertyName == nameof(obj.Id)));

            obj.RecursiveValidatable.Name = "Fuxion678901"; // Make invalid because has more than 10 character length
            PrintValidatorResults(val.Entries);
            Assert.Equal(2, counter);
            Assert.Equal(2, val.Entries.Count);
            Assert.Equal(1, val.Entries.Count(r => r.Path == $"{nameof(obj.RecursiveValidatable)}" && r.PropertyName == nameof(obj.Name)));

            obj.RecursiveValidatable.Id = 1;
            Assert.Equal(0, val.Entries.Count(r => r.Path == $"{nameof(obj.RecursiveValidatable)}" && r.PropertyName == nameof(obj.Id)));

            val.UnregisterNotifier(obj.RecursiveValidatable);
            Assert.Empty(val.Entries);
        }
        [Fact(DisplayName = "Validator - Automatic recursive validatable collection")]
        public void AutomaticRecusiveValidatableCollection()
        {
            var obj = new ValidatableMock();
            var val = new Validator2();
            val.RegisterNotifier(obj);
            var counter = 0;
            ((INotifyCollectionChanged)val.Entries).CollectionChanged += (s, e) => counter++;
            var first = obj.RecursiveValidatableCollection.First();

            first.Id = 0; // Make invalid because must be greater than 0
            PrintValidatorResults(val.Entries);
            var tt = $"{nameof(obj.RecursiveValidatableCollection)}.{first}.{nameof(obj.Id)}";
            Assert.Equal(1, counter);
            Assert.Single(val.Entries);
            Assert.Equal(1, val.Entries.Count(r => r.Path == $"{nameof(obj.RecursiveValidatableCollection)}[{first}]" && r.PropertyName == nameof(obj.Id)));

            first.Name = "Fuxion678901"; // Make invalid because has more than 10 character length
            PrintValidatorResults(val.Entries);
            Assert.Equal(2, counter);
            Assert.Equal(2, val.Entries.Count);
            Assert.Equal(1, val.Entries.Count(r => r.Path == $"{nameof(obj.RecursiveValidatableCollection)}[{first}]" && r.PropertyName == nameof(obj.Name)));

            first.Id = 1; // Make valid
            Assert.Equal(3, counter);
            Assert.Single(val.Entries);
            Assert.Equal(0, val.Entries.Count(r => r.Path == $"{nameof(obj.RecursiveValidatableCollection)}[{first}]" && r.PropertyName == nameof(obj.Id)));
            // When Id is valid again, and change to 0, the entry for name must change its Path for new first.ToString result
            Assert.Equal(1, val.Entries.Count(r => r.Path == $"{nameof(obj.RecursiveValidatableCollection)}[{first}]" && r.PropertyName == nameof(obj.Name)));

            var added = new RecursiveValidatableMock
            {
                Id = 1,
                Name = "Valid"
            };
            obj.RecursiveValidatableCollection.Add(added);

            added.Id = -1; // Make invalid because must be greater than 0
            PrintValidatorResults(val.Entries);
            Assert.Equal(4, counter);
            Assert.Equal(2, val.Entries.Count);
            Assert.Equal(1, val.Entries.Count(r => r.Path == $"{nameof(obj.RecursiveValidatableCollection)}[{added}]" && r.PropertyName == nameof(obj.Id)));

            obj.RecursiveValidatableCollection.Remove(added);
            PrintValidatorResults(val.Entries);
            Assert.Equal(0, val.Entries.Count(r => r.Path == $"{nameof(obj.RecursiveValidatableCollection)}[{added}]" && r.PropertyName == nameof(obj.Id)));

            added.Name = "Fuxion678901"; // Make invalid because has more than 10 character length
            PrintValidatorResults(val.Entries);
            Assert.Equal(5, counter);
            Assert.Single(val.Entries);
            Assert.Equal(0, val.Entries.Count(r => r.Path == $"{nameof(obj.RecursiveValidatableCollection)}[{added}]" && r.PropertyName == nameof(obj.Name)));

            val.UnregisterNotifier(first);
            Assert.Empty(val.Entries);
        }
        [Fact(DisplayName = "Validator - Automatic recursive validatable collection ensure elements")]
        public void AutomaticRecusiveValidatableCollection2()
        {
            var obj = new ValidatableMock();
            var val = new Validator2();
            val.RegisterNotifier(obj);
            var counter = 0;
            ((INotifyCollectionChanged)val.Entries).CollectionChanged += (s, e) => counter++;
            var first = obj.RecursiveValidatableCollection.First();

            obj.RecursiveValidatableCollection.Remove(first);

            Assert.Equal(1, counter);
            Assert.Single(val.Entries);
            Assert.Equal(1, val.Entries.Count(r => r.Path == $"" && r.PropertyName == nameof(obj.RecursiveValidatableCollection)));

            var added = new RecursiveValidatableMock
            {
                Id = 1,
                Name = "Valid"
            };
            obj.RecursiveValidatableCollection.Add(added);

            Assert.Equal(2, counter);
            Assert.Empty(val.Entries);
            Assert.Equal(0, val.Entries.Count(r => r.Path == $"" && r.PropertyName == nameof(obj.RecursiveValidatableCollection)));
        }
    }
}