using Fuxion.Factories;
using Fuxion.Threading;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml.Serialization;

namespace Fuxion.ComponentModel
{
    #region PropertyChangedEvent
    public class NotifierPropertyChangedCaseEventArgs<TNotifier, TValue> : EventArgs
    {
        internal NotifierPropertyChangedCaseEventArgs(TNotifier notifier, TValue previousValue, TValue actualValue)
        {
            Notifier = notifier;
            PreviousValue = previousValue;
            ActualValue = actualValue;
        }
        /// <summary>
        ///     Objeto que notifica el cambio en una propiedad.
        /// </summary>
        public TNotifier Notifier { get; private set; }
        /// <summary>
        ///     Valor de la propiedad antes del cambio.
        /// </summary>
        public TValue PreviousValue { get; private set; }
        /// <summary>
        ///     Valor actual de la propiedad.
        /// </summary>
        public TValue ActualValue { get; private set; }
    }
    public delegate void NotifierPropertyChangedEventHandler<TNotifier>(TNotifier notifier, NotifierPropertyChangedEventArgs<TNotifier> e);
    public class NotifierPropertyChangedEventArgs<TNotifier> : PropertyChangedEventArgs
    {
        public NotifierPropertyChangedEventArgs(string propertyName, TNotifier notifier, object previousValue, object actualValue) : base(propertyName)
        {
            Notifier = notifier;
            PreviousValue = previousValue;
            ActualValue = actualValue;
        }
        public NotifierPropertyChangedEventArgs<T> ConvertToNotifier<T>(T notifier)
        {
            return new NotifierPropertyChangedEventArgs<T>(PropertyName, notifier, PreviousValue, ActualValue);
        }
        private object PreviousValue { get; set; }
        private object ActualValue { get; set; }
        private TNotifier Notifier { get; set; }
        #region Case
        /// <summary>
        ///     Permite ejecutar una acción cuando una propiedad ha cambiado.
        /// </summary>
        /// <typeparam name="TValue">Tipo de la propiedad que ha cambiado.</typeparam>
        /// <param name="propertyName">Nombre de la propiedad.</param>
        /// <param name="action">
        ///     Acción que se llevará a cabo. Los dos parámetros son el valor anterior y el valor actual
        ///     respectivamente.
        /// </param>
        public void Case<TValue>(string propertyName,
            Action<NotifierPropertyChangedCaseEventArgs<TNotifier, TValue>> action)
        {
            if (propertyName == PropertyName)
                action(
                    new NotifierPropertyChangedCaseEventArgs<TNotifier, TValue>(
                        Notifier,
                        (TValue)PreviousValue,
                        (TValue)ActualValue));
        }
        /// <summary>
        ///     Permite ejecutar una acción cuando una propiedad ha cambiado.
        /// </summary>
        /// <typeparam name="TValue">Tipo de la propiedad que ha cambiado.</typeparam>
        /// <param name="expression">Expresión que determina la propiedad.</param>
        /// <param name="action">
        ///     Acción que se llevará a cabo. Los dos parámetros son el valor anterior y el valor actual
        ///     respectivamente.
        /// </param>
        public void Case<TValue>(Expression<Func<TValue>> expression,
            Action<NotifierPropertyChangedCaseEventArgs<TNotifier, TValue>> action)
        {
            if (expression.GetMemberName() == PropertyName)
                action(
                    new NotifierPropertyChangedCaseEventArgs<TNotifier, TValue>(
                        Notifier,
                        (TValue)PreviousValue,
                        (TValue)ActualValue));
        }
        /// <summary>
        ///     Permite ejecutar una acción cuando una propiedad ha cambiado.
        /// </summary>
        /// <typeparam name="TValue">Tipo de la propiedad que ha cambiado.</typeparam>
        /// <param name="action">
        ///     Acción que se llevará a cabo. Los dos parámetros son el valor anterior y el valor actual
        ///     respectivamente.
        /// </param>
        /// <param name="expressions">Expresiones que determinan las propiedades.</param>
        public void Case<TValue>(Action<NotifierPropertyChangedCaseEventArgs<TNotifier, TValue>> action,
            params Expression<Func<TValue>>[] expressions)
        {
            foreach (var exp in expressions)
                if (exp.GetMemberName() == PropertyName)
                {
                    action(
                        new NotifierPropertyChangedCaseEventArgs<TNotifier, TValue>(
                            Notifier,
                            (TValue)PreviousValue,
                            (TValue)ActualValue));
                    return;
                }
        }
        /// <summary>
        ///     Permite ejecutar una acción cuando una propiedad ha cambiado.
        /// </summary>
        /// <typeparam name="TValue">Tipo de la propiedad que ha cambiado.</typeparam>
        /// <param name="action">
        ///     Acción que se llevará a cabo. Los dos parámetros son el valor anterior y el valor actual
        ///     respectivamente.
        /// </param>
        /// <param name="propertyNames">Nombres de las propiedades.</param>
        public void Case<TValue>(Action<NotifierPropertyChangedCaseEventArgs<TNotifier, TValue>> action,
            params string[] propertyNames)
        {
            foreach (string pn in propertyNames)
            {
                if (pn == PropertyName)
                {
                    action(
                        new NotifierPropertyChangedCaseEventArgs<TNotifier, TValue>(
                            Notifier,
                            (TValue)PreviousValue,
                            (TValue)ActualValue));
                    return;
                }
            }
        }
        #endregion
        #region Is & IsAny
        /// <summary>
        ///     Comprueba la propiedad que ha cambiado.
        /// </summary>
        /// <typeparam name="TValue">Tipo de la propiedad que ha cambiado.</typeparam>
        /// <param name="expression">Expressión que determina la propiedad.</param>
        /// <returns>Devuelve true si la expressión determina la propiedad que ha cambiado.</returns>
        public bool Is<TValue>(Expression<Func<TValue>> expression) { return expression.GetMemberName() == PropertyName; }
        /// <summary>
        ///     Comprueba la propiedad que ha cambiado.
        /// </summary>
        /// <param name="expressions">Expressiones que determinan las propiedades que se comprobarán.</param>
        /// <returns>Devuelve true si alguna de las expressiones determina la propiedad que ha cambiado.</returns>
        public bool IsAny(params Expression<Func<object>>[] expressions) { return expressions.Any(e => e.GetMemberName() == PropertyName); }
        #endregion
    }
    #endregion
    public interface INotifier<TNotifier> : INotifyPropertyChanged where TNotifier : INotifier<TNotifier>
    {
        new event NotifierPropertyChangedEventHandler<TNotifier> PropertyChanged;
    }
    [DataContract(IsReference = true)]
    public abstract class Notifier<TNotifier> : INotifier<TNotifier> where TNotifier : class, INotifier<TNotifier>
    {
        protected Notifier()
        {
            // El constructor no será invocado al deserializar la clase porque se utiliza el método FormatterServices.GetUninitializedObject
            // y este crea el objeto sin estado, no se llamará al constructor ni se crearán las instancias de los campos de la clase.
            PropertiesDictionary = new Dictionary<string, object>();
            Synchronizer = Factory.Get<INotifierSynchronizer>();
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OnDeserializing]
        public void OnDeserializing(StreamingContext context)
        {
            //Este método será llamado al deserializar la clase en vez del contructor
            PropertiesDictionary = new Dictionary<string, object>();
            Synchronizer = Factory.Get<INotifierSynchronizer>();
        }
        #region Events
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged { add { PropertyChangedEvent += value; } remove { PropertyChangedEvent -= value; } }
        //event PropertyChangingEventHandler INotifyPropertyChanging.PropertyChanging { add { PropertyChangingEvent += value; } remove { PropertyChangingEvent -= value; } }
        public event NotifierPropertyChangedEventHandler<TNotifier> PropertyChanged;
        private event PropertyChangedEventHandler PropertyChangedEvent;
        //private event PropertyChangingEventHandler PropertyChangingEvent;
        #endregion
        #region Set&Get Value
        private volatile Dictionary<string, object> PropertiesDictionary;
        protected T GetValue<T>(Func<T> defaultValueFunction = null, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            object objValue;
            T value;
            if (PropertiesDictionary.TryGetValue(propertyName, out objValue)) value = (T)objValue;
            else
            {
                value = defaultValueFunction == null ? default(T) : defaultValueFunction();
                PropertiesDictionary[propertyName] = value;
            }
            return value;
        }
        protected bool SetValue<T>(T newValue, bool raiseOnlyIfNotEquals = true, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            T oldValue;
            if (PropertiesDictionary.ContainsKey(propertyName))
                oldValue = GetValue<T>(propertyName: propertyName);
            else
            {
                PropertyInfo pro = null;
                var props = GetType().GetTypeInfo().GetAllProperties().Where(p => p.Name == propertyName).ToList();
                if (props.Count == 0) throw new NotifierException($"Cannot find a property with name '{propertyName}' in type '{GetType().GetSignature(true)}'");
                if (props.Count > 1)
                {
                    var parentType = this.GetType();
                    while (pro == null && parentType != null)
                    {
                        var pp = props.Where(p => p.DeclaringType == parentType).ToList();
                        if (pp.Count == 1)
                            pro = pp.First();
                        parentType = parentType.GetTypeInfo().BaseType;
                    }
                    if(pro == null) throw new NotifierException($"Cannot find a property with name '{propertyName}' in type '{GetType().GetSignature(true)}'");
                }
                else
                {
                    pro = props.First();
                }
                oldValue = (T)pro.GetValue(this, null);
                //oldValue = (T)GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).GetValue(this, null);
            }
            if (raiseOnlyIfNotEquals && EqualityComparer<T>.Default.Equals(oldValue, newValue))
                return false;
            //if (!OnRaisePropertyChanging(propertyName, oldValue, newValue)) return false;
            PropertiesDictionary[propertyName] = newValue;
            OnRaisePropertyChanged(propertyName, oldValue, newValue);
            return true;
        }
        private ValueLocker<T> GetLockerProperty<T>(Func<T> defaultValueFunction = null, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            object objValue;
            if (PropertiesDictionary.TryGetValue(propertyName, out objValue))
                return (ValueLocker<T>)objValue;
            T defaultValue = defaultValueFunction == null ? default(T) : defaultValueFunction();
            var defaultLocker = new ValueLocker<T>(defaultValue);
            PropertiesDictionary[propertyName] = defaultLocker;
            return defaultLocker;
        }
        protected T GetLockedValue<T>(Func<T> defaultValueFunction = null, [CallerMemberName] string propertyName = null) where T : struct { return OnGetLockedValue(defaultValueFunction, propertyName); }
        protected string GetLockedValue(Func<string> defaultValueFunction = null, [CallerMemberName] string propertyName = null) { return OnGetLockedValue(defaultValueFunction, propertyName); }
        private T OnGetLockedValue<T>(Func<T> defaultValueFunction = null, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            ValueLocker<T> locker = GetLockerProperty(defaultValueFunction, propertyName);
            T value = locker.ObjectLocked;
            //T interceptedValue = OnRaisePropertyRead(propertyName, value);
            //if (!EqualityComparer<T>.Default.Equals(value, interceptedValue))
            //    ((ValueLocker<T>)PropertiesDictionary[propertyName]).WriteRef(interceptedValue);
            //return interceptedValue;
            return value;
        }
        protected bool SetLockedValue<T>(T value, bool raiseOnlyIfNotEquals = true, [CallerMemberName] string propertyName = null) where T : struct { return OnSetLockedValue(value, raiseOnlyIfNotEquals, propertyName); }
        protected bool SetLockedValue(string value, bool raiseOnlyIfNotEquals = true, [CallerMemberName] string propertyName = null) { return OnSetLockedValue(value, raiseOnlyIfNotEquals, propertyName); }
        private bool OnSetLockedValue<T>(T value, bool raiseOnlyIfNotEquals = true, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            ValueLocker<T> oldLockerValue;
            if (PropertiesDictionary.ContainsKey(propertyName))
            {
                oldLockerValue = GetLockerProperty<T>(propertyName: propertyName);
                //OnRaisePropertyRead(propertyName, oldLockerValue.ObjectLocked);
            }
            else
            {
                // oldLockerValue = new ValueLocker<T>((T)GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).GetValue(this, null));
                oldLockerValue = new ValueLocker<T>((T)GetType().GetTypeInfo().GetAllProperties().Single(p => p.Name == propertyName).GetValue(this, null));
                PropertiesDictionary[propertyName] = oldLockerValue;
            }
            if (raiseOnlyIfNotEquals && ((oldLockerValue == null && value == null) || EqualityComparer<T>.Default.Equals(oldLockerValue.ObjectLocked, value)))
                return false;
            T oldValue = oldLockerValue.ObjectLocked;
            //if (!OnRaisePropertyChanging(propertyName, oldValue, value)) return false;
            ((ValueLocker<T>)PropertiesDictionary[propertyName]).WriteRef(value);
            OnRaisePropertyChanged(propertyName, oldValue, value);
            return true;
        }
        #endregion
        #region Set&Get Field
        protected bool SetField<T>(ref T field, T value, bool raiseOnlyIfNotEquals = true, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            T oldValue = GetField(ref field, propertyName);
            if (raiseOnlyIfNotEquals && ((field == null && value == null) || (field != null && field.Equals(value))))
                return false;
            //if (!OnRaisePropertyChanging(propertyName, oldValue, value)) return false;
            field = value;
            OnRaisePropertyChanged(propertyName, oldValue, value);
            return true;
        }
        protected T GetField<T>(ref T field, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            //T interceptedValue = OnRaisePropertyRead(propertyName, field);
            //if (!EqualityComparer<T>.Default.Equals(field, interceptedValue))
            //    field = interceptedValue;
            //return interceptedValue;
            return field;
        }
        protected bool SetLockedField<T>(ref ValueLocker<T> lockedField, T value, bool raiseOnlyIfNotEquals = true, [CallerMemberName] String propertyName = null) where T : struct { return OnSetLockedField(propertyName, ref lockedField, value, raiseOnlyIfNotEquals); }
        protected bool SetLockedField(ref ValueLocker<string> lockedField, string value, bool raiseOnlyIfNotEquals = true, [CallerMemberName] String propertyName = null) { return OnSetLockedField(propertyName, ref lockedField, value, raiseOnlyIfNotEquals); }
        private bool OnSetLockedField<T>(string propertyName, ref ValueLocker<T> lockedField, T value, bool raiseOnlyIfNotEquals)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            T oldValue = OnGetLockedField(ref lockedField, propertyName);
            if (
                raiseOnlyIfNotEquals
                &&
                (
                    (lockedField == null && value == null) ||
                    (lockedField != null && lockedField.ObjectLocked == null && value == null) ||
                    (lockedField != null && lockedField.ObjectLocked != null && lockedField.ObjectLocked.Equals(value))
                    )
                )
                return false;
            //if (!OnRaisePropertyChanging(propertyName, oldValue, value)) return false;
            if (lockedField == null) lockedField = new ValueLocker<T>(value);
            else lockedField.WriteRef(value);
            OnRaisePropertyChanged(propertyName, oldValue, value);
            return true;
        }
        protected T GetLockedField<T>(ref ValueLocker<T> lockedField, [CallerMemberName] string propertyName = null) where T : struct { return OnGetLockedField(ref lockedField, propertyName); }
        protected string GetLockedField(ref ValueLocker<string> lockedField, [CallerMemberName] string propertyName = null) { return OnGetLockedField(ref lockedField, propertyName); }
        private T OnGetLockedField<T>(ref ValueLocker<T> lockedField, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            T value = lockedField == null ? default(T) : lockedField.ObjectLocked;
            //T interceptedValue = OnRaisePropertyRead(propertyName, value);
            //if (EqualityComparer<T>.Default.Equals(value, interceptedValue)) return interceptedValue;
            //if (lockedField == null) lockedField = new ValueLocker<T>(interceptedValue);
            //else lockedField.WriteRef(interceptedValue);
            //return interceptedValue;
            return value;
        }
        #endregion

        #region RaisePropertyChanged
        [EditorBrowsable(EditorBrowsableState.Never)]
        [XmlIgnore]
        [JsonIgnore]
        public INotifierSynchronizer Synchronizer { get; set; }
        //[EditorBrowsable(EditorBrowsableState.Never)]
        [XmlIgnore]
        [JsonIgnore]
        protected bool UseSynchronizerOnRaisePropertyChanged { get { return GetValue(() => true); } set { SetValue(value); } }
        protected void RaisePropertyChanged<T>(Expression<Func<object>> expression, T previousValue, T actualValue)
        {
            string memberName = expression.GetMemberName();
            PropertyInfo prop = GetType().GetTypeInfo().GetAllProperties().Single(p => p.Name == memberName);
            if (prop == null) throw new ArgumentException("No se ha encontrado la propiedad '" + memberName + "' en el tipo '" + GetType().Name + "'.");
            OnRaisePropertyChanged(memberName, previousValue, actualValue);
        }
        protected virtual void OnRaisePropertyChanged<T>(string propertyName, T previousValue, T actualValue)
        {
            var action = new Action<string, T, T>((pro, pre, act) => {
                PropertyChangedEvent?.Invoke(this, new PropertyChangedEventArgs(pro));
                PropertyChanged?.Invoke(this as TNotifier, new NotifierPropertyChangedEventArgs<TNotifier>(pro, (TNotifier)(INotifier<TNotifier>)this, pre, act));
            });
            if (UseSynchronizerOnRaisePropertyChanged && Synchronizer != null) Synchronizer.Invoke(action, propertyName, previousValue, actualValue).Wait();
            else action(propertyName, previousValue, actualValue);
        }
        #endregion

        #region Binding
        public INotifierBinding<TNotifier,TProperty> Binding<TProperty>(Expression<Func<TProperty>> sourcePropertyExpression)
        {
            return new NotifierBinding<TNotifier, TProperty>
            {
                Notifier = this,
                SourcePropertyExpression = sourcePropertyExpression
            };
        }
        #endregion
    }
    public interface INotifierBinding<TNotifier, TProperty> where TNotifier : class, INotifier<TNotifier>
    {

    }
    class NotifierBinding<TNotifier, TProperty> : INotifierBinding<TNotifier, TProperty> where TNotifier : class, INotifier<TNotifier>
    {
        public Notifier<TNotifier> Notifier { get; set; }
        public Expression<Func<TProperty>> SourcePropertyExpression { get; set; }
    }
    public static class INotifierBindingExtensions {
        public static void OneWayTo<TNotifier, TProperty>(this INotifierBinding<TNotifier, TProperty> me, object target, Expression<Func<TProperty>> targetPropertyExpression) where TNotifier : class, INotifier<TNotifier>
        {
            var not = me as NotifierBinding<TNotifier, TProperty>;
            not.Notifier.PropertyChanged += (s, e) => e.Case(not.SourcePropertyExpression, a => targetPropertyExpression.GetPropertyInfo().SetValue(target, a.ActualValue));
            targetPropertyExpression.GetPropertyInfo().SetValue(target, not.SourcePropertyExpression.GetPropertyInfo().GetValue(not.Notifier));
        }
        public static void OneWayTo<TNotifier, TProperty, TTargetProperty>(this INotifierBinding<TNotifier, TProperty> me, object target,
            Expression<Func<TTargetProperty>> targetPropertyExpression,
            Func<TProperty,TTargetProperty> transformFuction) where TNotifier : class, INotifier<TNotifier>
        {
            var not = me as NotifierBinding<TNotifier, TProperty>;
            not.Notifier.PropertyChanged += (s, e) => e.Case(not.SourcePropertyExpression, a => targetPropertyExpression.GetPropertyInfo().SetValue(target, transformFuction(a.ActualValue)));
            targetPropertyExpression.GetPropertyInfo().SetValue(target, transformFuction((TProperty)not.SourcePropertyExpression.GetPropertyInfo().GetValue(not.Notifier)));
        }
        public static void TwoWayTo<TNotifier, TProperty, TTargetNotifier>(this INotifierBinding<TNotifier, TProperty> me, INotifier<TTargetNotifier> target, Expression<Func<TProperty>> targetPropertyExpression) 
            where TNotifier : class, INotifier<TNotifier>
            where TTargetNotifier : class, INotifier<TTargetNotifier>
        {
            var not = me as NotifierBinding<TNotifier, TProperty>;
            not.Notifier.PropertyChanged += (s, e) => e.Case(not.SourcePropertyExpression, a => targetPropertyExpression.GetPropertyInfo().SetValue(target, a.ActualValue));
            target.PropertyChanged += (s, e) => e.Case(targetPropertyExpression, a => not.SourcePropertyExpression.GetPropertyInfo().SetValue(not.Notifier, a.ActualValue));
            targetPropertyExpression.GetPropertyInfo().SetValue(target, not.SourcePropertyExpression.GetPropertyInfo().GetValue(not.Notifier));
        }
    }
}
