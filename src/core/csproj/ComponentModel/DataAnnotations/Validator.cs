using Fuxion.Collections.Generic;
using System;
using System.Collections;
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
            Entries = new ReadOnlyObservableCollection<ValidatorEntry2>(_entries);
            _entries.CollectionChanged += (s, e) => HasEntries = Entries.Count > 0;
        }

        ObservableCollection<ValidatorEntry2> _entries = new ObservableCollection<ValidatorEntry2>();
        public ReadOnlyObservableCollection<ValidatorEntry2> Entries { get; private set; }

        public bool HasEntries
        {
            get => GetValue<bool>();
            private set => SetValue(value);
        }

        Dictionary<object, Func<string>> notifierPaths = new Dictionary<object, Func<string>>();

        public void RegisterNotifier(INotifyPropertyChanged notifier, bool recursive = true) => RegisterNotifier(notifier, recursive, () => "");
        private void RegisterNotifier(INotifyPropertyChanged notifier, bool recursive, Func<string> pathFunc)
        {
            if (notifierPaths.ContainsKey(notifier)) throw new DuplicateNotifierException();
            notifierPaths.Add(notifier, pathFunc);
            notifier.PropertyChanged += Notifier_PropertyChanged;
            if (!recursive) return;
            foreach (var pro in TypeDescriptor.GetProperties(notifier.GetType())
                .Cast<PropertyDescriptor>()
                .Where(pro => pro.Attributes.OfType<RecursiveValidationAttribute>().Any())
                .Where(pro => typeof(INotifyPropertyChanged).IsAssignableFrom(pro.PropertyType)))
                RegisterNotifier((INotifyPropertyChanged)pro.GetValue(notifier), true, () => $"{pro.Name}");
            foreach (var pro in TypeDescriptor.GetProperties(notifier.GetType())
                .Cast<PropertyDescriptor>()
                .Where(pro => pro.Attributes.OfType<RecursiveValidationAttribute>().Any())
                .Where(pro => typeof(INotifyCollectionChanged).IsAssignableFrom(pro.PropertyType))
                .Where(pro => pro.PropertyType.IsSubclassOfRawGeneric(typeof(IEnumerable<>))))
                RegisterNotifierCollection(notifier, pro, pathFunc);
            RefreshEntries(notifier, null, pathFunc);
        }
        private void RegisterNotifierCollection(object notifier, PropertyDescriptor property, Func<string> pathFunc)
        {
            var collection = (INotifyCollectionChanged)property.GetValue(notifier);
            foreach (var item in (IEnumerable)collection)
                if (typeof(INotifyPropertyChanged).IsAssignableFrom(item.GetType())) RegisterNotifier((INotifyPropertyChanged)item, true, () => $"{(pathFunc() + property.DisplayName).Trim('.')}[{item.ToString()}]");
            collection.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                    foreach (var item in e.NewItems)
                        if (typeof(INotifyPropertyChanged).IsAssignableFrom(item.GetType()))
                            RegisterNotifier((INotifyPropertyChanged)item, true, () => $"{(pathFunc()+property.DisplayName).Trim('.')}[{item.ToString()}]");
                if (e.OldItems != null)
                    foreach (var item in e.OldItems)
                        if (typeof(INotifyPropertyChanged).IsAssignableFrom(item.GetType()))
                            UnregisterNotifier((INotifyPropertyChanged)item);
                RefreshEntries(notifier, property.Name, pathFunc);
            };
        }
        public void UnregisterNotifier(INotifyPropertyChanged notifier)
        {
            notifier.PropertyChanged -= Notifier_PropertyChanged;
            _entries.RemoveIf(e => e.Object == notifier);
            notifierPaths.Remove(notifier);
        }
        private void Notifier_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _entries.RemoveIf(ent => ent.Object == sender && ent.PropertyName == e.PropertyName);
            foreach (var ent in _entries.Where(ent => ent.Object == sender))
                ent.Path = notifierPaths[sender]();
            RefreshEntries(sender, e.PropertyName, notifierPaths[sender]);
        }
        private void RefreshEntries(object instance, string propertyName, Func<string> pathFunc)
        {
            _entries.RemoveIf(e => e.Object == instance && (string.IsNullOrWhiteSpace(propertyName) || e.PropertyName == propertyName));
            var newEntries = Validate(instance, propertyName, pathFunc);
            foreach (var ent in newEntries)
                if (!_entries.Contains(ent))
                    _entries.Add(ent);
        }
        public ICollection<ValidatorEntry2> Validate(object instance, string propertyName = null) => Validate(instance, propertyName, () => "");
        private ICollection<ValidatorEntry2> Validate(object instance, string propertyName, Func<string> pathFunc)
        {
            if (instance == null) return new ValidatorEntry2[0];
            // Validate all properties of the instance
            var insRes = TypeDescriptor.GetProperties(instance.GetType())
                .Cast<PropertyDescriptor>()
                .Where(pro => propertyName == null || pro.Name == propertyName)
                .Where(pro => pro.Attributes.OfType<ValidationAttribute>().Any())
                .SelectMany(pro => pro.Attributes.OfType<ValidationAttribute>()
                    .Where(att => !att.IsValid(pro.GetValue(instance)))
                    .Select(att => new ValidatorEntry2(instance)
                    {
                        Message = att.FormatErrorMessage(string.Empty),
                        Path = pathFunc(),
                        PropertyName = pro.DisplayName,
                    }));
            // Validate all properties of the metadata type
            var metaAtt = instance.GetType().GetCustomAttribute<MetadataTypeAttribute>(true, false, true);
            if (metaAtt != null)
            {
                var metaRes = TypeDescriptor.GetProperties(metaAtt.MetadataClassType)
                .Cast<PropertyDescriptor>()
                .Where(pro => propertyName == null || pro.Name == propertyName)
                .Where(pro => pro.Attributes.OfType<ValidationAttribute>().Any())
                .SelectMany(pro => pro.Attributes.OfType<ValidationAttribute>()
                    .Where(att => !att.IsValid(TypeDescriptor.GetProperties(instance).Cast<PropertyDescriptor>().First(p => p.Name == pro.Name).GetValue(instance)))
                    .Select(att => new ValidatorEntry2(instance)
                    {
                        Message = att.FormatErrorMessage(string.Empty),
                        Path = pathFunc(),
                        PropertyName = pro.DisplayName,
                    }));
                insRes = insRes.Concat(metaRes);
            }
            // Validate all sub validatables
            var subIns = TypeDescriptor.GetProperties(instance.GetType())
                .Cast<PropertyDescriptor>()
                .Where(pro => propertyName == null || pro.Name == propertyName)
                .Where(pro => pro.Attributes.OfType<RecursiveValidationAttribute>().Any())
                .Where(pro => !pro.PropertyType.IsSubclassOfRawGeneric(typeof(IEnumerable<>)))
                .SelectMany(pro => Validate(pro.GetValue(instance), null, () => $"{pro.DisplayName}"));
            // Validate all sub collection validatables
            var subColIns = TypeDescriptor.GetProperties(instance.GetType())
                .Cast<PropertyDescriptor>()
                .Where(pro => propertyName == null || pro.Name == propertyName)
                .Where(pro => pro.Attributes.OfType<RecursiveValidationAttribute>().Any())
                .Where(pro => pro.PropertyType.IsSubclassOfRawGeneric(typeof(IEnumerable<>)))
                .SelectMany(pro =>
                {
                    List<ValidatorEntry2> res = new List<ValidatorEntry2>();
                    var ienu = (IEnumerable)pro.GetValue(instance);
                    foreach (var t in ienu)
                    {
                        res.AddRange(Validate(t, null, () => $"{pro.DisplayName}[{t.ToString()}]"));
                    }
                    return res;
                });

            return insRes.Concat(subIns).Concat(subColIns).OrderBy(r => r.Path).ThenBy(r => r.PropertyName).ToList();
        }
    }
    public class ValidatorEntry2 : Notifier<ValidatorEntry2>
    {
        public ValidatorEntry2(object @object)
        {
            Object = @object;
        }
        internal object Object { get; set; }
        public string Path
        {
            get => GetValue<string>();
            internal set => SetValue(value);
        }
        public string PropertyName
        {
            get => GetValue<string>();
            internal set => SetValue(value);
        }
        public string Message
        {
            get => GetValue<string>();
            internal set => SetValue(value);
        }

        public override bool Equals(object obj)
        {
            if (obj is ValidatorEntry2 ve)
                return ve.Object == Object && ve.Path == Path && ve.PropertyName == PropertyName && ve.Message == Message;
            return base.Equals(obj);
        }
        public override int GetHashCode() => Object.GetHashCode() ^ Path.GetHashCode() ^ PropertyName.GetHashCode() ^ Message.GetHashCode();
    }
    //public class ValidatorEntryCollection : INotifyCollectionChanged
    //{
    //    public event NotifyCollectionChangedEventHandler CollectionChanged;

    //    public ReadOnlyObservableCollection<ValidatorEntryNamedCollection> ByPath { get; set; }
    //    public ReadOnlyObservableCollection<ValidatorEntryNamedCollection> ByPropertyName { get; set; }
    //}
    //public class ValidatorEntryNamedCollection : ValidatorEntryCollection, INotifyCollectionChanged
    //{
    //    public string Name { get; set; }
    //}
}