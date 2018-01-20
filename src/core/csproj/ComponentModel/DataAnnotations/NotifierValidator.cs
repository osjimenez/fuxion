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
            StringMessages = new ReadOnlyObservableCollection<string>(_stringMessages);
            ((INotifyCollectionChanged)Messages).CollectionChanged += (s, e) =>
            {
                HasMessages = Messages.Count > 0;
                if (e.NewItems != null)
                    foreach (var item in e.NewItems.Cast<NotifierValidatorMessage>())
                        _stringMessages.Add(item.Message);
                if (e.OldItems != null)
                    foreach (var item in e.OldItems.Cast<NotifierValidatorMessage>())
                        _stringMessages.Remove(item.Message);
            };
            ((INotifyCollectionChanged)Messages).CollectionChanged += (s, e) => ValidationChanged?.Invoke(this, EventArgs.Empty);
        }

        ObservableCollection<NotifierValidatorMessage> _messages = new ObservableCollection<NotifierValidatorMessage>();
        public ReadOnlyObservableCollection<NotifierValidatorMessage> Messages { get; private set; }
        ObservableCollection<string> _stringMessages = new ObservableCollection<string>();
        public ReadOnlyObservableCollection<string> StringMessages { get; private set; }

        public bool HasMessages
        {
            get => GetValue<bool>();
            private set => SetValue(value);
        }

        public event EventHandler ValidationChanged;

        Dictionary<object, Func<string>> paths = new Dictionary<object, Func<string>>();

        public void RegisterNotifier(INotifyPropertyChanged notifier, bool recursive = true) => RegisterNotifier(notifier, recursive, () => "");
        private void RegisterNotifier(INotifyPropertyChanged notifier, bool recursive, Func<string> pathFunc)
        {
            if (notifier == null) throw new NullReferenceException($"The parameter '{nameof(notifier)}' cannot be null");
            if (paths.ContainsKey(notifier)) return;//throw new DuplicateNotifierException();
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
                //.Where(pro => pro.Attributes.OfType<RecursiveValidationAttribute>().Any())
                .Where(pro => typeof(INotifyCollectionChanged).IsAssignableFrom(pro.PropertyType))
                .Where(pro => pro.PropertyType.IsSubclassOfRawGeneric(typeof(IEnumerable<>))))
                RegisterNotifierCollection(notifier, pro, pathFunc);
            RefreshEntries(notifier, null, pathFunc);
        }
        private void RegisterNotifierCollection(INotifyPropertyChanged notifier, PropertyDescriptor property, Func<string> pathFunc)
        {
            var recursive = property.Attributes.OfType<RecursiveValidationAttribute>().Any();
            var collection = (INotifyCollectionChanged)property.GetValue(notifier);
            foreach (var item in (IEnumerable)collection)
                if (typeof(INotifyPropertyChanged).IsAssignableFrom(item.GetType())) RegisterNotifier((INotifyPropertyChanged)item, true, () => $"{(pathFunc() + property.GetDisplayName()).Trim('.')}[{item.ToString()}]");
            collection.CollectionChanged += (s, e) =>
            {
                if (recursive && e.NewItems != null)
                    foreach (var item in e.NewItems)
                        if (typeof(INotifyPropertyChanged).IsAssignableFrom(item.GetType()))
                            RegisterNotifier((INotifyPropertyChanged)item, true, () => $"{(pathFunc()+property.GetDisplayName()).Trim('.')}[{item.ToString()}]");
                if (recursive && e.OldItems != null)
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
            foreach (var ent in _messages.Where(ent => ent.Object == sender))
                ent.Path = paths[sender]();

            var notifier = TypeDescriptor.GetProperties(sender)
                .Cast<PropertyDescriptor>()
                .Where(pro => pro.Name == e.PropertyName)
                .Where(pro => pro.Attributes.OfType<RecursiveValidationAttribute>().Any())
                .Where(pro => typeof(INotifyPropertyChanged).IsAssignableFrom(pro.PropertyType))
                .Select(pro => (INotifyPropertyChanged)pro.GetValue(sender))
                .Where(obj => obj != null)
                .FirstOrDefault();
            if (notifier != null) RegisterNotifier(notifier, true, () => paths[sender]() + e.PropertyName);

            var collection = TypeDescriptor.GetProperties(sender)
                .Cast<PropertyDescriptor>()
                .Where(pro => pro.Name == e.PropertyName)
                //.Where(pro => pro.Attributes.OfType<RecursiveValidationAttribute>().Any())
                .Where(pro => typeof(INotifyCollectionChanged).IsAssignableFrom(pro.PropertyType))
                .Where(pro => (INotifyCollectionChanged)pro.GetValue(sender) != null)
                .FirstOrDefault();
            if (collection != null) RegisterNotifierCollection((INotifyPropertyChanged)sender, collection, paths[sender]);

            var conditionalAtt = sender.GetType().GetCustomAttribute<ConditionalValidationAttribute>(true, false, true);
            RefreshEntries(sender, conditionalAtt != null ? null : e.PropertyName, paths[sender]);
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
            var conditionalAtt = instance.GetType().GetCustomAttribute<ConditionalValidationAttribute>(true, false, true);
            if(conditionalAtt != null && !conditionalAtt.Check(instance)) return new NotifierValidatorMessage[0];
            // Validate all properties of the instance
            var insRes = TypeDescriptor.GetProperties(instance.GetType())
                .Cast<PropertyDescriptor>()
                .Where(pro => propertyName == null || pro.Name == propertyName)
                .Where(pro => pro.Attributes.OfType<ValidationAttribute>().Any())
                .SelectMany(pro => pro.Attributes.OfType<ValidationAttribute>()
                    .Select(att =>
                    {
                        var val = pro.GetValue(instance);
                        var context = new ValidationContext(instance)
                        {
                            MemberName = pro.Name
                        };
                        return new
                        {
                            Attribute = att,
                            att.RequiresValidationContext,
                            Value = val,
                            Context = context,
                            IsValid = att.RequiresValidationContext ? null : (bool?)att.IsValid(val),
                            Result = att.GetValidationResult(val, context)
                        };
                    })
                    .Where(tup => (tup.RequiresValidationContext && tup.Result != ValidationResult.Success) || (!tup.RequiresValidationContext && !tup.IsValid.Value))
                    .Select(tup => new NotifierValidatorMessage(instance)
                    {
                        Message = tup.RequiresValidationContext
                            ? tup.Result.ErrorMessage
                            : tup.Attribute.FormatErrorMessage(pro.GetDisplayName()),
                        Path = pathFunc(),
                        PropertyDisplayName = pro.GetDisplayName(),
                        PropertyName = pro.Name,
                    })).ToList();
            // Validate all properties of the metadata type
            var metaAtt = instance.GetType().GetCustomAttribute<MetadataTypeAttribute>(true, false, true);
            if (metaAtt != null)
            {
                var metaRes = TypeDescriptor.GetProperties(metaAtt.MetadataClassType)
                .Cast<PropertyDescriptor>()
                .Where(pro => propertyName == null || pro.Name == propertyName)
                .Where(pro => pro.Attributes.OfType<ValidationAttribute>().Any())
                .SelectMany(pro => pro.Attributes.OfType<ValidationAttribute>()
                    .Select(att =>
                    {
                        var val = TypeDescriptor.GetProperties(instance).Cast<PropertyDescriptor>().First(p => p.Name == pro.Name).GetValue(instance);
                        return new
                        {
                            Attribute = att,
                            att.RequiresValidationContext,
                            Value = val,
                            Context = new ValidationContext(instance),
                            IsValid = att.RequiresValidationContext ? null : (bool?)att.IsValid(val),
                            Result = att.GetValidationResult(val, new ValidationContext(instance))
                        };
                    })
                    .Where(tup => (tup.RequiresValidationContext && tup.Result != ValidationResult.Success) || (!tup.RequiresValidationContext && !tup.IsValid.Value))
                    .Select(tup => new NotifierValidatorMessage(instance)
                    {
                        Message = tup.RequiresValidationContext
                            ? tup.Result.ErrorMessage
                            : tup.Attribute.FormatErrorMessage(pro.GetDisplayName()),
                        Path = pathFunc(),
                        PropertyDisplayName = pro.GetDisplayName(),
                        PropertyName = pro.Name,
                    })).ToList();
                insRes = insRes.Concat(metaRes).ToList();
            }
            // Validate all sub validatables
            var subIns = TypeDescriptor.GetProperties(instance.GetType())
                .Cast<PropertyDescriptor>()
                .Where(pro => propertyName == null || pro.Name == propertyName)
                .Where(pro => pro.Attributes.OfType<RecursiveValidationAttribute>().Any())
                .Where(pro => !pro.PropertyType.IsSubclassOfRawGeneric(typeof(IEnumerable<>)))
                .SelectMany(pro => Validate(pro.GetValue(instance), null, () => $"{pro.GetDisplayName()}")).ToList();
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
                            res.AddRange(Validate(t, null, () => $"{pro.GetDisplayName()}[{t.ToString()}]"));
                        }
                    return res;
                }).ToList();

            return insRes.Concat(subIns).Concat(subColIns).OrderBy(r => r.Path).ThenBy(r => r.PropertyDisplayName).ToList();
        }

        public void ClearCustoms() => _messages.RemoveIf(m => m.Object is CustomValidatorEntryObject);
        public IDisposable AddCustom(string message, string path = null, string propertyName = null, string propertyDisplayName = null)
        {
            var res = new CustomValidatorEntryObject().AsDisposable(o => _messages.RemoveIf(m => m.Object == o));
            _messages.Add(new NotifierValidatorMessage(res.Value)
            {
                Path = path,
                PropertyName = propertyName,
                PropertyDisplayName = propertyDisplayName,
                Message = message
            });
            return res;
        }
        class CustomValidatorEntryObject { }
    }
}