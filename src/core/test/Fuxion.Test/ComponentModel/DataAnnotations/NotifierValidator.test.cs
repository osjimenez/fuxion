using Fuxion.ComponentModel.DataAnnotations;
using Fuxion.Testing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test.ComponentModel.DataAnnotations
{
	public class NotifierValidatorTest : BaseTest
	{
		public NotifierValidatorTest(ITestOutputHelper output) : base(output) { }
		private void PrintValidatorResults(IEnumerable<NotifierValidatorMessage> res)
		{
			Output.WriteLine("Printing validator entries:");
			foreach (NotifierValidatorMessage r in res)
			{
				Output.WriteLine($"   - {r.Path} - {r.PropertyName} - {r.Message}");
			}
		}
		[Fact(DisplayName = "Validator - Default values are valid")]
		public void DefaultIsValid()
		{
			ValidatableMock obj = new ValidatableMock();
			ICollection<NotifierValidatorMessage> res = NotifierValidator.Validate(obj);
			PrintValidatorResults(res);
			Assert.Empty(res);
		}
		[Fact(DisplayName = "Validator - Manual validation")]
		public void ManualValidation()
		{
			ValidatableMock obj = new ValidatableMock
			{
				Id = 0, // Make invalid because must be greater than 0
				Name = "Fuxion678901", // Make doubly invalid because contains 'Fuxion' and has more than 10 character length
				RecursiveValidatable = null!, // Make invalid because is required
			};

			obj.IgnoredName = null!; // Must be ignored
			obj.IgnoredRecursiveValidatable.Name = "Fuxion678901"; // Must be ignored
			obj.IgnoredRecursiveValidatableCollection = null!; // Must be ignored

			ICollection<NotifierValidatorMessage> res = NotifierValidator.Validate(obj);
			PrintValidatorResults(res);
			Assert.Equal(4, res.Count());
			Assert.Equal(1, res.Count(r => string.IsNullOrEmpty(r.Path) && r.PropertyName == nameof(obj.Id)));
			Assert.Equal(2, res.Count(r => string.IsNullOrEmpty(r.Path) && r.PropertyName == nameof(obj.Name)));
			Assert.Equal(1, res.Count(r => string.IsNullOrEmpty(r.Path) && r.PropertyName == nameof(obj.RecursiveValidatable)));

			res = NotifierValidator.Validate(obj, nameof(obj.Name));
			Assert.Equal(2, res.Count());
		}
		[Fact(DisplayName = "Validator - Manual recursive validatable")]
		public void ManualRecursiveValidatable()
		{
			ValidatableMock obj = new ValidatableMock();
			obj.RecursiveValidatable.Id = 0; // Make invalid because must be greater than 0

			obj.IgnoredName = null!; // Must be ignored
			obj.IgnoredRecursiveValidatable.Id = 0; // Must be ignored
			obj.IgnoredRecursiveValidatableCollection.First().Id = 0; // Must be ignored

			ICollection<NotifierValidatorMessage> res = NotifierValidator.Validate(obj);
			PrintValidatorResults(res);
			Assert.Equal(1, res.Count(r => r.Path == $"{nameof(obj.RecursiveValidatable)}" && r.PropertyName == nameof(obj.Id)));
		}
		[Fact(DisplayName = "Validator - Manual recursive validatable collection")]
		public void ManualRecursiveValidatableCollection()
		{
			ValidatableMock obj = new ValidatableMock();
			RecursiveValidatableMock first = obj.RecursiveValidatableCollection.First();
			first.Id = 0; // Make invalid because must be greater than 0

			obj.IgnoredName = null!; // Must be ignored
			obj.IgnoredRecursiveValidatable = null!; // Must be ignored
			obj.IgnoredRecursiveValidatableCollection.First().Id = 0; // Must be ignored

			ICollection<NotifierValidatorMessage> res = NotifierValidator.Validate(obj);
			PrintValidatorResults(res);
			Assert.Single(res);
			Assert.Equal(1, res.Count(r => r.Path == $"{nameof(obj.RecursiveValidatableCollection)}[{first}]" && r.PropertyName == nameof(obj.Id)));
		}

		[Fact(DisplayName = "Validator - Automatic validation")]
		public void AutomaticValidation()
		{
			ValidatableMock obj = new ValidatableMock();
			NotifierValidator val = new NotifierValidator();
			int counter = 0;
			((INotifyCollectionChanged)val.Messages).CollectionChanged += (s, e) => counter++;

			obj.Id = 0; // Make invalid because must be greater than 0

			val.RegisterNotifier(obj);
			PrintValidatorResults(val.Messages);

			Assert.Equal(1, counter);
			Assert.Single(val.Messages);
			Assert.Equal(1, val.Messages.Count(r => string.IsNullOrEmpty(r.Path) && r.PropertyName == nameof(obj.Id)));

			obj.Name = "Fuxion789.12"; // Make doubly invalid because contains 'Fuxion' and has more than 10 character length
			obj.IgnoredName = "Fuxion789.12"; // Must be ignored
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
			ValidatableMock obj = new ValidatableMock();
			NotifierValidator val = new NotifierValidator();
			int counter = 0;
			((INotifyCollectionChanged)val.Messages).CollectionChanged += (s, e) => counter++;

			obj.Id = 0; // Make invalid because must be greater than 0
			obj.Name = "Fuxion789.12"; // Make doubly invalid because contains 'Fuxion' and has more than 10 character length
			obj.IgnoredName = "Fuxion789.12"; // Must be ignored

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
			ValidatableMock obj = new ValidatableMock();
			NotifierValidator val = new NotifierValidator();
			int counter = 0;
			((INotifyCollectionChanged)val.Messages).CollectionChanged += (s, e) => counter++;

			obj.RecursiveValidatable.Id = 0; // Make invalid because must be greater than 0
			obj.IgnoredRecursiveValidatable.Id = 0; // Must be ignored

			val.RegisterNotifier(obj);
			PrintValidatorResults(val.Messages);
			Assert.Equal(1, counter);
			Assert.Single(val.Messages);
			Assert.Equal(1, val.Messages.Count(r => r.Path == $"{nameof(obj.RecursiveValidatable)}" && r.PropertyName == nameof(obj.Id)));

			obj.RecursiveValidatable.Name = "Fuxion678901"; // Make invalid because has more than 10 character length
			obj.IgnoredRecursiveValidatable.Name = "Fuxion678901"; // Must be ignored
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
			ValidatableMock obj = new ValidatableMock();
			NotifierValidator val = new NotifierValidator();
			int counter = 0;
			((INotifyCollectionChanged)val.Messages).CollectionChanged += (s, e) => counter++;

			obj.RecursiveValidatable = null!; // Make invalid because is required
			obj.IgnoredRecursiveValidatable = null!; // Must be ignored

			val.RegisterNotifier(obj);
			PrintValidatorResults(val.Messages);
			Assert.Equal(1, counter);
			Assert.Single(val.Messages);
			Assert.Equal(1, val.Messages.Count(r => r.Path == $"" && r.PropertyName == nameof(obj.RecursiveValidatable)));

			obj.RecursiveValidatable = new RecursiveValidatableMock();
			obj.IgnoredRecursiveValidatable = new RecursiveValidatableMock();

			PrintValidatorResults(val.Messages);
			Assert.Equal(2, counter);
			Assert.Empty(val.Messages);
			Assert.Equal(0, val.Messages.Count(r => r.Path == $"" && r.PropertyName == nameof(obj.RecursiveValidatable)));

			obj.RecursiveValidatable.Id = -1;
			obj.IgnoredRecursiveValidatable.Id = -1; // Must be ignored
			Assert.Equal(3, counter);
			Assert.Single(val.Messages);
			Assert.Equal(1, val.Messages.Count(r => r.Path == $"{nameof(obj.RecursiveValidatable)}" && r.PropertyName == nameof(obj.Id)));

			val.UnregisterNotifier(obj.RecursiveValidatable);
			Assert.Empty(val.Messages);
		}
		[Fact(DisplayName = "Validator - Automatic recursive validatable collection")]
		public void AutomaticRecusiveValidatableCollection()
		{
			ValidatableMock obj = new ValidatableMock();
			NotifierValidator val = new NotifierValidator();
			val.RegisterNotifier(obj);
			int counter = 0;
			((INotifyCollectionChanged)val.Messages).CollectionChanged += (s, e) => counter++;
			var first = obj.RecursiveValidatableCollection.First();
			var ignoredFirst = obj.IgnoredRecursiveValidatableCollection.First();

			first.Id = 0; // Make invalid because must be greater than 0
			ignoredFirst.Id = 0; // Must be ignored
			PrintValidatorResults(val.Messages);
			Assert.Equal(1, counter);
			Assert.Single(val.Messages);
			Assert.Equal(1, val.Messages.Count(r => r.Path == $"{nameof(obj.RecursiveValidatableCollection)}[{first}]" && r.PropertyName == nameof(obj.Id)));

			first.Name = "Fuxion678901"; // Make invalid because has more than 10 character length
			ignoredFirst.Name = "Fuxion678901"; // Must be ignored
			PrintValidatorResults(val.Messages);
			Assert.Equal(2, counter);
			Assert.Equal(2, val.Messages.Count);
			Assert.Equal(1, val.Messages.Count(r => r.Path == $"{nameof(obj.RecursiveValidatableCollection)}[{first}]" && r.PropertyName == nameof(obj.Name)));

			first.Id = 1; // Make valid
			ignoredFirst.Id = 1; // Must be ignored
			PrintValidatorResults(val.Messages);
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
			var ignoredAdded = new RecursiveValidatableMock
			{
				Id = 1,
				Name = "Valid"
			};
			obj.RecursiveValidatableCollection.Add(added);
			obj.IgnoredRecursiveValidatableCollection.Add(ignoredAdded);

			added.Id = -1; // Make invalid because must be greater than 0
			ignoredAdded.Id = -1; // Must be ignored
			PrintValidatorResults(val.Messages);
			Assert.Equal(4, counter);
			Assert.Equal(2, val.Messages.Count);
			Assert.Equal(1, val.Messages.Count(r => r.Path == $"{nameof(obj.RecursiveValidatableCollection)}[{added}]" && r.PropertyName == nameof(obj.Id)));

			obj.RecursiveValidatableCollection.Remove(added);
			obj.IgnoredRecursiveValidatableCollection.Remove(ignoredAdded);
			PrintValidatorResults(val.Messages);
			Assert.Equal(0, val.Messages.Count(r => r.Path == $"{nameof(obj.RecursiveValidatableCollection)}[{added}]" && r.PropertyName == nameof(obj.Id)));

			added.Name = "Fuxion678901"; // Make invalid because has more than 10 character length
			ignoredAdded.Name = "Fuxion678901"; // Must be ignored
			PrintValidatorResults(val.Messages);
			Assert.Equal(5, counter);
			Assert.Single(val.Messages);
			Assert.Equal(0, val.Messages.Count(r => r.Path == $"{nameof(obj.RecursiveValidatableCollection)}[{added}]" && r.PropertyName == nameof(obj.Name)));

			val.UnregisterNotifier(first);
			val.UnregisterNotifier(ignoredFirst);
			Assert.Empty(val.Messages);
		}
		[Fact(DisplayName = "Validator - Automatic recursive validatable collection ensure elements")]
		public void AutomaticRecusiveValidatableCollectionEnsureElements()
		{
			ValidatableMock obj = new ValidatableMock();
			NotifierValidator val = new NotifierValidator();
			val.RegisterNotifier(obj);
			int counter = 0;
			((INotifyCollectionChanged)val.Messages).CollectionChanged += (s, e) => counter++;
			var first = obj.RecursiveValidatableCollection.First();
			var ignoredFirst = obj.IgnoredRecursiveValidatableCollection.First();

			obj.RecursiveValidatableCollection.Remove(first);
			obj.IgnoredRecursiveValidatableCollection.Remove(ignoredFirst);

			Assert.Equal(1, counter);
			Assert.Single(val.Messages);
			Assert.Equal(1, val.Messages.Count(r => r.Path == $"" && r.PropertyName == nameof(obj.RecursiveValidatableCollection)));

			var added = new RecursiveValidatableMock();
			var ignoredAdded = new RecursiveValidatableMock();
			obj.RecursiveValidatableCollection.Add(added);
			obj.IgnoredRecursiveValidatableCollection.Add(ignoredAdded);

			Assert.Equal(2, counter);
			Assert.Empty(val.Messages);
			Assert.Equal(0, val.Messages.Count(r => r.Path == $"" && r.PropertyName == nameof(obj.RecursiveValidatableCollection)));
		}
		[Fact(DisplayName = "Validator - Automatic recursive validatable collection null")]
		public void AutomaticRecusiveValidatableCollectionNull()
		{
			ValidatableMock obj = new ValidatableMock();
			NotifierValidator val = new NotifierValidator();
			val.RegisterNotifier(obj);
			int counter = 0;
			((INotifyCollectionChanged)val.Messages).CollectionChanged += (s, e) => counter++;

			obj.RecursiveValidatableCollection = null!;
			obj.IgnoredRecursiveValidatableCollection = null!;

			PrintValidatorResults(val.Messages);
			Assert.Equal(1, counter);
			Assert.Single(val.Messages);
			Assert.Equal(1, val.Messages.Count(r => r.Path == $"" && r.PropertyName == nameof(obj.RecursiveValidatableCollection)));

			obj.RecursiveValidatableCollection = new ObservableCollection<RecursiveValidatableMock>();
			obj.IgnoredRecursiveValidatableCollection = new ObservableCollection<RecursiveValidatableMock>();
			PrintValidatorResults(val.Messages);
			var added = new RecursiveValidatableMock();
			var ignoredAdded = new RecursiveValidatableMock();
			obj.RecursiveValidatableCollection.Add(added);
			obj.IgnoredRecursiveValidatableCollection.Add(ignoredAdded);

			PrintValidatorResults(val.Messages);
			Assert.Equal(2, counter);
			Assert.Empty(val.Messages);
			Assert.Equal(0, val.Messages.Count(r => r.Path == $"" && r.PropertyName == nameof(obj.RecursiveValidatableCollection)));

			added.Id = -1;
			ignoredAdded.Id = -1;

			PrintValidatorResults(val.Messages);
			Assert.Equal(3, counter);
			Assert.Single(val.Messages);
			Assert.Equal(1, val.Messages.Count(r => r.Path == $"{nameof(obj.RecursiveValidatableCollection)}[{added}]" && r.PropertyName == nameof(obj.Id)));
		}
		[Fact(DisplayName = "Validator - Automatic recursive validatable collection cleared")]
		public void AutomaticRecusiveValidatableCollectionCleared()
		{
			ValidatableMock obj = new ValidatableMock();
			NotifierValidator val = new NotifierValidator();
			val.RegisterNotifier(obj);
			int counter = 0;
			((INotifyCollectionChanged)val.Messages).CollectionChanged += (s, e) => counter++;

			var first = obj.RecursiveValidatableCollection.First();
			var ignoredFirst = obj.IgnoredRecursiveValidatableCollection.First();
			first.Name = null!;
			ignoredFirst.Name = null!; // Must be ignored

			PrintValidatorResults(val.Messages);
			Assert.Equal(1, counter);
			Assert.Single(val.Messages);
			Assert.Equal(1, val.Messages.Count(r => r.Path == $"{nameof(obj.RecursiveValidatableCollection)}[{first}]" && r.PropertyName == nameof(obj.Name)));

			obj.RecursiveValidatableCollection.Clear();
			obj.IgnoredRecursiveValidatableCollection.Clear();

			Assert.Equal(3, counter);
			Assert.Equal(1, val.Messages.Count(r => r.Path == "" && r.PropertyName == nameof(obj.RecursiveValidatableCollection)));
			Assert.Single(val.Messages);
		}

		[Fact(DisplayName = "Validator - Custom validation")]
		public void CustomValidation()
		{
			//var obj = new ValidatableMock();
			NotifierValidator val = new NotifierValidator();

			IDisposable dis = val.AddCustom("Test for custom validations", "Path", "PropertyName", "PropertyDisplayName");

			Assert.Single(val.Messages);
			Assert.Equal(1, val.Messages.Count(r => r.Path == "Path" && r.PropertyName == "PropertyName" && r.PropertyDisplayName == "PropertyDisplayName"));

			dis.Dispose();

			Assert.Empty(val.Messages);

			val.AddCustom("Test for custom validations", "Path", "PropertyName", "PropertyDisplayName");
			val.ClearCustoms();
			Assert.Empty(val.Messages);
		}

		[Fact(DisplayName = "Validator - Change GetHashCode")]
		public void ChangeGethashCode()
		{
			ValidatableMock obj = new ValidatableMock();
			NotifierValidator val = new NotifierValidator();
			int counter = 0;
			((INotifyCollectionChanged)val.Messages).CollectionChanged += (s, e) => counter++;
			val.RegisterNotifier(obj);

			obj.RecursiveValidatable.Key = Guid.NewGuid();
			obj.Id = 0; // Make invalid because must be greater than 0
			Assert.Equal(1, counter);
			Assert.Single(val.Messages);
			Assert.Equal(1, val.Messages.Count(r => string.IsNullOrEmpty(r.Path) && r.PropertyName == nameof(obj.Id)));

			obj.RecursiveValidatable.Key = Guid.NewGuid();
			obj.Name = "Fuxion789.12"; // Make doubly invalid because contains 'Fuxion' and has more than 10 character length
			obj.IgnoredName = "Fuxion789.12"; // Must be ignored
			PrintValidatorResults(val.Messages);
			Assert.Equal(3, counter);
			Assert.Equal(3, val.Messages.Count);
			Assert.Equal(2, val.Messages.Count(r => string.IsNullOrEmpty(r.Path) && r.PropertyName == nameof(obj.Name)));

			obj.RecursiveValidatable.Key = Guid.NewGuid();
			obj.Id = 1; // Make valid because is greater than 0
			PrintValidatorResults(val.Messages);
			Assert.DoesNotContain(val.Messages, r => string.IsNullOrEmpty(r.Path) && r.PropertyName == nameof(obj.Id));
			Assert.Equal(4, counter);
			Assert.Equal(2, val.Messages.Count);

			val.UnregisterNotifier(obj);
			Assert.Empty(val.Messages);
		}
	}
}