using Fuxion.Threading;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;

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
                action.Invoke(
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
                action.Invoke(
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
                    action.Invoke(
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
                    action.Invoke(
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
    public abstract class Notifier<TNotifier> : INotifier<TNotifier> where TNotifier : class, INotifier<TNotifier>
    //public abstract class Notifier<TNotifier> : INotifyPropertyChanged
        //where TNotifier : Notifier<TNotifier>
    {
        protected Notifier()
        {
            // El constructor no será invocado al deserializar la clase porque se utiliza el método FormatterServices.GetUninitializedObject
            // y este crea el objeto sin estado, no se llamará al constructor ni se crearán las instancias de los campos de la clase.
            PropertiesDictionary = ImmutableDictionary.Create<string, object>();
        }
        [OnDeserializing]
        public void OnDeserializing(StreamingContext context)
        {
            //Este método será llamado al deserializar la clase en vez del contructor
            PropertiesDictionary = ImmutableDictionary.Create<string, object>();
        }
        #region Events
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged { add { PropertyChangedEvent += value; } remove { PropertyChangedEvent -= value; } }
        //event PropertyChangingEventHandler INotifyPropertyChanging.PropertyChanging { add { PropertyChangingEvent += value; } remove { PropertyChangingEvent -= value; } }
        public event NotifierPropertyChangedEventHandler<TNotifier> PropertyChanged;
        private event PropertyChangedEventHandler PropertyChangedEvent;
        //private event PropertyChangingEventHandler PropertyChangingEvent;
        #endregion
        #region Set&Get Value
        private volatile ImmutableDictionary<string, object> PropertiesDictionary;
        protected T GetValue<T>(Func<T> defaultValueFunction = null, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            object objValue;
            T value;
            if (PropertiesDictionary.TryGetValue(propertyName, out objValue)) value = (T)objValue;
            else
            {
                value = defaultValueFunction == null ? default(T) : defaultValueFunction.Invoke();
                PropertiesDictionary = PropertiesDictionary.SetItem(propertyName, value);
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
                oldValue = (T)GetType().GetRuntimeProperty(propertyName).GetValue(this, null);
                //oldValue = (T)GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).GetValue(this, null);
            }
            if (raiseOnlyIfNotEquals && EqualityComparer<T>.Default.Equals(oldValue, newValue))
                return false;
            //if (!OnRaisePropertyChanging(propertyName, oldValue, newValue)) return false;
            PropertiesDictionary = PropertiesDictionary.SetItem(propertyName, newValue);
            OnRaisePropertyChanged(propertyName, oldValue, newValue);
            return true;
        }
        private ValueLocker<T> GetLockerProperty<T>(Func<T> defaultValueFunction = null, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            object objValue;
            if (PropertiesDictionary.TryGetValue(propertyName, out objValue))
                return (ValueLocker<T>)objValue;
            T defaultValue = defaultValueFunction == null ? default(T) : defaultValueFunction.Invoke();
            var defaultLocker = new ValueLocker<T>(defaultValue);
            PropertiesDictionary.SetItem(propertyName, defaultLocker);
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
                oldLockerValue = new ValueLocker<T>((T)GetType().GetRuntimeProperty(propertyName).GetValue(this, null));
                PropertiesDictionary = PropertiesDictionary.Add(propertyName, oldLockerValue);
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
        protected void RaisePropertyChanged<T>(Expression<Func<object>> expression, T previousValue, T actualValue)
        {
            string memberName = expression.GetMemberName();
            //PropertyInfo prop = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(p => p.Name == memberName);
            PropertyInfo prop = GetType().GetRuntimeProperty(memberName);
            if (prop == null) throw new ArgumentException("No se ha encontrado la propiedad '" + memberName + "' en el tipo '" + GetType().Name + "'.");
            OnRaisePropertyChanged(memberName, previousValue, actualValue);
        }
        protected virtual void OnRaisePropertyChanged<T>(string propertyName, T previousValue, T actualValue)
        {
            PropertyChangedEventHandler propertyChangedEvent = PropertyChangedEvent;
            if (propertyChangedEvent != null)
                propertyChangedEvent(this, new PropertyChangedEventArgs(propertyName));
            NotifierPropertyChangedEventHandler<TNotifier> propertyChanged = PropertyChanged;
            if (propertyChanged != null)
                propertyChanged(this as TNotifier, new NotifierPropertyChangedEventArgs<TNotifier>(propertyName, (TNotifier)(INotifier<TNotifier>)this, previousValue, actualValue));
        }
        #endregion
    }
}
