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
    public class NotifierValidator : Notifier<NotifierValidator>
    {
        public NotifierValidator()
        {
            Messages = new ReadOnlyObservableCollection<NotifierValidatorMessage>(_messages);
            _messages.CollectionChanged += (s, e) => HasMessages = Messages.Count > 0;
        }

        ObservableCollection<NotifierValidatorMessage> _messages = new ObservableCollection<NotifierValidatorMessage>();
        public ReadOnlyObservableCollection<NotifierValidatorMessage> Messages { get; private set; }

        public bool HasMessages
        {
            get => GetValue<bool>();
            private set => SetValue(value);
        }

        Dictionary<object, Func<string>> paths = new Dictionary<object, Func<string>>();

        public void RegisterNotifier(INotifyPropertyChanged notifier, bool recursive = true) => RegisterNotifier(notifier, recursive, () => "");
        private void RegisterNotifier(INotifyPropertyChanged notifier, bool recursive, Func<string> pathFunc)
        {
            if (notifier == null) throw new NullReferenceException($"The parameter '{nameof(notifier)}' cannot be null");
            if (paths.ContainsKey(notifier)) throw new DuplicateNotifierException();
            paths.Add(notifier, pathFunc);
            notifier.PropertyChanged += Notifier_PropertyChanged;
            if (!recursive) return;
            foreach (var pro in TypeDescriptor.GetProperties(notifier.GetType())
                .Cast<PropertyDescriptor>()
                .Where(pro => pro.Attributes.OfType<RecursiveValidationAttribute>().Any())
                .Where(pro => typeof(INotifyPropertyChanged).IsAssignableFrom(pro.PropertyType)))
            {
                var val = (INotifyPropertyChanged)pro.GetValue(notifier);
                if(val != null)
                    RegisterNotifier(val, true, () => $"{pro.Name}");
            }
            foreach (var pro in TypeDescriptor.GetProperties(notifier.GetType())
                .Cast<PropertyDescriptor>()
                .Where(pro => pro.Attributes.OfType<RecursiveValidationAttribute>().Any())
                .Where(pro => typeof(INotifyCollectionChanged).IsAssignableFrom(pro.PropertyType))
                .Where(pro => pro.PropertyType.IsSubclassOfRawGeneric(typeof(IEnumerable<>))))
                RegisterNotifierCollection(notifier, pro, pathFunc);
            RefreshEntries(notifier, null, pathFunc);
        }
        private void RegisterNotifierCollection(INotifyPropertyChanged notifier, PropertyDescriptor property, Func<string> pathFunc)
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
            if (notifier == null) throw new NullReferenceException($"The parameter '{nameof(notifier)}' cannot be null");
            notifier.PropertyChanged -= Notifier_PropertyChanged;
            _messages.RemoveIf(e => e.Object == notifier);
            paths.Remove(notifier);
        }
        private void Notifier_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //_messages.RemoveIf(ent => ent.Object == sender && ent.PropertyName == e.PropertyName);

            foreach (var ent in _messages.Where(ent => ent.Object == sender))
                ent.Path = paths[sender]();

            var notifier = TypeDescriptor.GetProperties(sender)
                .Cast<PropertyDescriptor>()
                .Where(pro => pro.DisplayName == e.PropertyName)
                .Where(pro => typeof(INotifyPropertyChanged).IsAssignableFrom(pro.PropertyType))
                .Select(pro => (INotifyPropertyChanged)pro.GetValue(sender))
                .Where(obj => obj != null)
                .FirstOrDefault();
            if (notifier != null) RegisterNotifier(notifier, true, () => paths[sender]() + e.PropertyName);

            var collection = TypeDescriptor.GetProperties(sender)
                .Cast<PropertyDescriptor>()
                .Where(pro => pro.DisplayName == e.PropertyName)
                .Where(pro => typeof(INotifyCollectionChanged).IsAssignableFrom(pro.PropertyType))
                .Where(pro => (INotifyCollectionChanged)pro.GetValue(sender) != null)
                //.Where(obj => obj != null)
                .FirstOrDefault();
            if (collection != null) RegisterNotifierCollection((INotifyPropertyChanged)sender, collection, paths[sender]);

            RefreshEntries(sender, e.PropertyName, paths[sender]);
        }
        private void RefreshEntries(object instance, string propertyName, Func<string> pathFunc)
        {
            var newEntries = Validate(instance, propertyName, pathFunc);
            _messages.RemoveIf(e => !newEntries.Contains(e) && e.Object == instance && (string.IsNullOrWhiteSpace(propertyName) || e.PropertyName == propertyName));
            foreach (var ent in newEntries)
                if (!_messages.Contains(ent))
                    _messages.Add(ent);
        }
        public ICollection<NotifierValidatorMessage> Validate(object instance, string propertyName = null) => Validate(instance, propertyName, () => "");
        private ICollection<NotifierValidatorMessage> Validate(object instance, string propertyName, Func<string> pathFunc)
        {
            if (instance == null) return new NotifierValidatorMessage[0];
            // Validate all properties of the instance
            var insRes = TypeDescriptor.GetProperties(instance.GetType())
                .Cast<PropertyDescriptor>()
                .Where(pro => propertyName == null || pro.Name == propertyName)
                .Where(pro => pro.Attributes.OfType<ValidationAttribute>().Any())
                .SelectMany(pro => pro.Attributes.OfType<ValidationAttribute>()
                    .Where(att => !att.IsValid(pro.GetValue(instance)))
                    .Select(att => new NotifierValidatorMessage(instance)
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
                    .Select(att => new NotifierValidatorMessage(instance)
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
                    List<NotifierValidatorMessage> res = new List<NotifierValidatorMessage>();
                    var ienu = (IEnumerable)pro.GetValue(instance);
                    if(ienu != null)
                        foreach (var t in ienu)
                        {
                            res.AddRange(Validate(t, null, () => $"{pro.DisplayName}[{t.ToString()}]"));
                        }
                    return res;
                });

            return insRes.Concat(subIns).Concat(subColIns).OrderBy(r => r.Path).ThenBy(r => r.PropertyName).ToList();
        }
    }
}