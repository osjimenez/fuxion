using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
namespace Fuxion.ComponentModel.DataAnnotations
{
    public class Validator2 : Notifier<Validator2>
    {
        public Validator2()
        {
            Entries = new ReadOnlyObservableCollection<Validator2Entry>(_entries);
        }
        public Validator2(INotifyPropertyChanged notifier) : this()
        {
            notifier.PropertyChanged += (s, e) =>
            {

            };
        }
        public Validator2(INotifyCollectionChanged collectionNotifier) : this() { }

        ObservableCollection<Validator2Entry> _entries = new ObservableCollection<Validator2Entry>();
        public ReadOnlyObservableCollection<Validator2Entry> Entries { get; private set; }
        //public ReadOnlyObservableCollection<string> AllErrors => Entries.SelectMany(e => e.Errors);
        //public ReadOnlyObservableCollection<string> AllWarnings => Entries.SelectMany(e => e.Warnings);

        public void AddError(string key, string errorMessage)
        {
            var ent = Entries.FirstOrDefault(e => e.Key == key);
            if (ent == null)
                _entries.Add(ent = new Validator2Entry
                {
                    Key = key
                });
            ent.AddError(errorMessage);
        }

        public IEnumerable<ValidatorResult> Validate(object instance)
        {
            if (instance == null) return Enumerable.Empty<ValidatorResult>();
            // Validate all properties of the instance
            var insRes = TypeDescriptor.GetProperties(instance.GetType())
                .Cast<PropertyDescriptor>()
                .Where(pro => pro.Attributes.OfType<ValidationAttribute>().Any())
                .SelectMany(pro => pro.Attributes.OfType<ValidationAttribute>()
                    .Where(att => !att.IsValid(pro.GetValue(instance)))
                    .Select(att => new ValidatorResult { Message = att.FormatErrorMessage(string.Empty) }));
            // Validate all properties of the metadata type
            var metaAtt = instance.GetType().GetCustomAttribute<MetadataTypeAttribute>(true, false, true);
            if(metaAtt != null)
            {
                var metaRes = TypeDescriptor.GetProperties(metaAtt.MetadataClassType)
                .Cast<PropertyDescriptor>()
                .Where(pro => pro.Attributes.OfType<ValidationAttribute>().Any())
                .SelectMany(pro => pro.Attributes.OfType<ValidationAttribute>()
                    .Where(att => !att.IsValid(TypeDescriptor.GetProperties(instance).Cast<PropertyDescriptor>().First(p => p.Name == pro.Name).GetValue(instance)))
                    .Select(att => new ValidatorResult { Message = att.FormatErrorMessage(string.Empty) }));
                insRes = insRes.Concat(metaRes);
            }
            // Validate all sub validatables
            var subIns = TypeDescriptor.GetProperties(instance.GetType())
                .Cast<PropertyDescriptor>()
                .Where(pro => pro.Attributes.OfType<SubValidatable>().Any())
                .SelectMany(pro => Validate(pro.GetValue(instance)));

            return insRes.Concat(subIns);
        }
    }
    public class ValidatorResult {
        public string Message { get; set; }
    }
    public class Validator2Entry
    {
        public Validator2Entry()
        {
            Errors = new ReadOnlyObservableCollection<string>(_errors);
            Warnings = new ReadOnlyObservableCollection<string>(_warnings);
        }
        public string Key { get; set; }

        ObservableCollection<string> _errors = new ObservableCollection<string>();
        public ReadOnlyObservableCollection<string> Errors { get; private set; }
        ObservableCollection<string> _warnings = new ObservableCollection<string>();
        public ReadOnlyObservableCollection<string> Warnings { get; private set; }

        public void AddError(string errorMessage) => _errors.Add(errorMessage);
    }

    public class SubValidatable : Attribute
    {
    }
}
