using Fuxion.Threading;
using Fuxion.Windows.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
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
		public TNotifier Notifier { get; private set; }
		public TValue PreviousValue { get; private set; }
		public TValue ActualValue { get; private set; }
	}
	public delegate void NotifierPropertyChangedEventHandler<TNotifier>(TNotifier notifier, NotifierPropertyChangedEventArgs<TNotifier> e);
	public class NotifierPropertyChangedEventArgs<TNotifier> : PropertyChangedEventArgs
	{
		public NotifierPropertyChangedEventArgs(string? propertyName, TNotifier notifier, object? previousValue, object? actualValue) : base(propertyName)
		{
			Notifier = notifier;
			PreviousValue = previousValue;
			ActualValue = actualValue;
		}
		// NULLABLE - Method removed
		public NotifierPropertyChangedEventArgs<T> ConvertToNotifier<T>(T notifier) => new NotifierPropertyChangedEventArgs<T>(PropertyName, notifier, PreviousValue, ActualValue);
		private object? PreviousValue { get; set; }
		private object? ActualValue { get; set; }
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
						// TODO Nullables - Remove exception
						(TValue)PreviousValue!,
						// TODO Nullables - Remove exception
						(TValue)ActualValue!));
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
						// TODO Nullables - Remove exception
						(TValue)PreviousValue!,
						// TODO Nullables - Remove exception
						(TValue)ActualValue!));
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
							// TODO Nullables - Remove exception
							(TValue)PreviousValue!,
							// TODO Nullables - Remove exception
							(TValue)ActualValue!));
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
			foreach (var pn in propertyNames)
			{
				if (pn == PropertyName)
				{
					action(
						new NotifierPropertyChangedCaseEventArgs<TNotifier, TValue>(
							Notifier,
							// TODO Nullables - Remove exception
							(TValue)PreviousValue!,
							// TODO Nullables - Remove exception
							(TValue)ActualValue!));
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
		public bool Is<TValue>(Expression<Func<TValue>> expression) => expression.GetMemberName() == PropertyName;
		/// <summary>
		///     Comprueba la propiedad que ha cambiado.
		/// </summary>
		/// <param name="expressions">Expressiones que determinan las propiedades que se comprobarán.</param>
		/// <returns>Devuelve true si alguna de las expressiones determina la propiedad que ha cambiado.</returns>
		public bool IsAny(params Expression<Func<object?>>[] expressions) => expressions.Any(e => e.GetMemberName() == PropertyName);
		#endregion
	}
	#endregion
	public interface INotifier<TNotifier> : INotifyPropertyChanged where TNotifier : INotifier<TNotifier>
	{
		new event NotifierPropertyChangedEventHandler<TNotifier> PropertyChanged;
	}
	public class NotifierJsonConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			if (value != null && value.GetType().IsSubclassOfRawGeneric(typeof(Notifier<>)))
			{
				var ti = value.GetType().GetTypeInfo();
				while (ti != null && (!ti.IsGenericType || ti.GetGenericTypeDefinition() != typeof(Notifier<>)))
				{
					ti = ti.BaseType?.GetTypeInfo();
				}
				var field = ti?.GetDeclaredField("PropertiesDictionary");
				var pros = (Dictionary<string, object>)(field?.GetValue(value) ?? new NullReferenceException($"The '{nameof(value)}' parameter cannot be reflected prior to write it at json"));
				writer.WriteStartObject();
				foreach (var pro in pros.Where(p => p.Key != "UseSynchronizerOnRaisePropertyChanged"))
				{
					writer.WritePropertyName(pro.Key);
					serializer.Serialize(writer, pro.Value);
				}
				writer.WriteEndObject();
				Debug.WriteLine("");
			}
			else
			{
				throw new InvalidCastException($"Type '{value?.GetType().Name ?? "null"}' isn't a subclass of '{typeof(Notifier<>).Name}'");
			}
		}
		public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			if (existingValue == null) existingValue = Activator.CreateInstance(objectType);
			// Load JObject from stream
			var jObject = JObject.Load(reader);
			if (existingValue != null)
				serializer.Populate(jObject.CreateReader(), existingValue);
			return existingValue;
		}
		public override bool CanConvert(Type objectType) =>
			//return _types.Any(t => t == objectType);
			false;
		public override bool CanRead => true;
	}
	[DataContract(IsReference = true)]
	//[DataContract]
	//[JsonConverter(typeof(NotifierJsonConverter))]
	public abstract class Notifier<TNotifier> : INotifier<TNotifier>, IInvokable where TNotifier : class, INotifier<TNotifier>
	{
		protected Notifier()
		{
			// El constructor no será invocado al deserializar la clase porque se utiliza el método FormatterServices.GetUninitializedObject
			// y este crea el objeto sin estado, no se llamará al constructor ni se crearán las instancias de los campos de la clase.
			PropertiesDictionary = new Dictionary<string, object?>();
			OnInitialize();
		}
		[EditorBrowsable(EditorBrowsableState.Never)]
		[OnDeserializing]
		public void OnDeserializing(StreamingContext context)
		{
			//Este método será llamado al deserializar la clase en vez del contructor
			PropertiesDictionary = new Dictionary<string, object?>();
			OnInitialize();
		}
		protected virtual void OnInitialize() { }

		[XmlIgnore]
		[JsonIgnore]
		bool IInvokable.UseInvoker { get; set; } = true;

		#region Events
		event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged { add { PropertyChangedEvent += value; } remove { PropertyChangedEvent -= value; } }
		//event PropertyChangingEventHandler INotifyPropertyChanging.PropertyChanging { add { PropertyChangingEvent += value; } remove { PropertyChangingEvent -= value; } }
		public event NotifierPropertyChangedEventHandler<TNotifier>? PropertyChanged;
		private event PropertyChangedEventHandler? PropertyChangedEvent;
		//private event PropertyChangingEventHandler PropertyChangingEvent;
		#endregion
		#region Set&Get Value
		private volatile Dictionary<string, object?> PropertiesDictionary;
		private string GetPropertyKey(string propertyName) => propertyName;
		//=> GetType().GetTypeInfo().GetAllProperties().Where(p => p.Name == propertyName).First().DeclaringType.GetSignature(true) + "." + propertyName;

		protected T GetValue<T>(Func<T>? defaultValueFunction = null, [CallerMemberName] string propertyName = "")
		{
			if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
			object? value;
			if (PropertiesDictionary.TryGetValue(GetPropertyKey(propertyName), out var objValue)) value = objValue;
			else
			{
				value = defaultValueFunction == null ? default : defaultValueFunction();
				PropertiesDictionary[GetPropertyKey(propertyName)] = value;
			}
			return (T)(value!);
		}
		protected bool SetValue<T>(T newValue, bool raiseOnlyIfNotEquals = true, [CallerMemberName] string propertyName = "")
		{
			if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
			T? oldValue;
			if (PropertiesDictionary.ContainsKey(GetPropertyKey(propertyName)))
				oldValue = GetValue<T>(propertyName: propertyName);
			else
			{
				PropertyInfo? pro = null;
				var props = GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.Name == propertyName).ToList();
				if (props.Count == 0) throw new NotifierException($"Cannot find a property with name '{propertyName}' in type '{GetType().GetSignature(true)}'");
				if (props.Count > 1)
				{
					var parentType = GetType();
					while (pro == null && parentType != null)
					{
						var pp = props.Where(p => p.DeclaringType == parentType).ToList();
						if (pp.Count == 1)
							pro = pp.First();
						parentType = parentType.GetTypeInfo().BaseType;
					}
					if (pro == null) throw new NotifierException($"Cannot find a property with name '{propertyName}' in type '{GetType().GetSignature(true)}'");
				}
				else
				{
					pro = props.First();
				}
				oldValue = (T?)pro.GetValue(this, null);
				//oldValue = (T)GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).GetValue(this, null);
			}
			if (raiseOnlyIfNotEquals && EqualityComparer<T>.Default.Equals(oldValue, newValue))
				return false;
			//if (!OnRaisePropertyChanging(propertyName, oldValue, newValue)) return false;
			PropertiesDictionary[GetPropertyKey(propertyName)] = newValue;
			OnRaisePropertyChanged(propertyName, oldValue, newValue);
			return true;
		}
		private Locker<T> GetLockerProperty<T>(Func<T>? defaultValueFunction = null, [CallerMemberName] string propertyName = "")
		{
			if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
			if (PropertiesDictionary.TryGetValue(GetPropertyKey(propertyName), out var objValue))
				return (Locker<T>)(objValue!);
			var defaultValue = defaultValueFunction == null ? default : defaultValueFunction();
			var defaultLocker = new Locker<T>(defaultValue!);
			PropertiesDictionary[GetPropertyKey(propertyName)] = defaultLocker;
			return defaultLocker;
		}
		protected T GetLockedValue<T>(Func<T>? defaultValueFunction = null, [CallerMemberName] string propertyName = "") where T : struct => OnGetLockedValue(defaultValueFunction, propertyName);
		protected string GetLockedValue(Func<string>? defaultValueFunction = null, [CallerMemberName] string propertyName = "") => OnGetLockedValue(defaultValueFunction, propertyName);
		private T OnGetLockedValue<T>(Func<T>? defaultValueFunction = null, [CallerMemberName] string propertyName = "")
		{
			if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
			var locker = GetLockerProperty(defaultValueFunction, propertyName);
			var value = locker.Read(_ => _);
			//T value = locker.ObjectLocked;

			//T interceptedValue = OnRaisePropertyRead(propertyName, value);
			//if (!EqualityComparer<T>.Default.Equals(value, interceptedValue))
			//    ((ValueLocker<T>)PropertiesDictionary[propertyName]).WriteRef(interceptedValue);
			//return interceptedValue;
			return value;
		}
		protected bool SetLockedValue<T>(T value, bool raiseOnlyIfNotEquals = true, [CallerMemberName] string propertyName = "") where T : struct => OnSetLockedValue(value, raiseOnlyIfNotEquals, propertyName);
		protected bool SetLockedValue(string value, bool raiseOnlyIfNotEquals = true, [CallerMemberName] string propertyName = "") => OnSetLockedValue(value, raiseOnlyIfNotEquals, propertyName);
		private bool OnSetLockedValue<T>(T value, bool raiseOnlyIfNotEquals = true, [CallerMemberName] string propertyName = "")
		{
			if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
			Locker<T> oldLockerValue;
			if (PropertiesDictionary.ContainsKey(GetPropertyKey(propertyName)))
			{
				oldLockerValue = GetLockerProperty<T>(propertyName: propertyName);
				//OnRaisePropertyRead(propertyName, oldLockerValue.ObjectLocked);
			}
			else
			{
				// oldLockerValue = new ValueLocker<T>((T)GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).GetValue(this, null));
				var obj = (T?)GetType()
					.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
					.Single(p => p.Name == propertyName)
					.GetValue(this, null);
				if (!typeof(T).IsNullable() && obj == null) throw new NullReferenceException($"The value from property '{propertyName}' was null");
				oldLockerValue = new Locker<T>(obj!);
				PropertiesDictionary[GetPropertyKey(propertyName)] = oldLockerValue;
			}
			//if (raiseOnlyIfNotEquals && ((oldLockerValue == null && value == null) || EqualityComparer<T>.Default.Equals(oldLockerValue.Read(_ => _), value)))
			if (raiseOnlyIfNotEquals && (value == null || EqualityComparer<T>.Default.Equals(oldLockerValue.Read(_ => _), value)))
				return false;
			var oldValue = oldLockerValue.Read(_ => _);
			//if (!OnRaisePropertyChanging(propertyName, oldValue, value)) return false;
			((Locker<T>)PropertiesDictionary[GetPropertyKey(propertyName)]!).WriteObject(value);
			OnRaisePropertyChanged(propertyName, oldValue, value);
			return true;
		}
		#endregion
		#region Set&Get Field
		protected bool SetField<T>(ref T field, T value, bool raiseOnlyIfNotEquals = true, [CallerMemberName] string propertyName = "")
		{
			if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
			var oldValue = GetField(ref field, propertyName);
			if (raiseOnlyIfNotEquals && ((field == null && value == null) || (field != null && field.Equals(value))))
				return false;
			//if (!OnRaisePropertyChanging(propertyName, oldValue, value)) return false;
			field = value;
			OnRaisePropertyChanged(propertyName, oldValue, value);
			return true;
		}
		protected T GetField<T>(ref T field, [CallerMemberName] string propertyName = "")
		{
			if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
			//T interceptedValue = OnRaisePropertyRead(propertyName, field);
			//if (!EqualityComparer<T>.Default.Equals(field, interceptedValue))
			//    field = interceptedValue;
			//return interceptedValue;
			return field;
		}
		protected bool SetLockedField<T>(ref Locker<T> lockedField, T value, bool raiseOnlyIfNotEquals = true, [CallerMemberName] string propertyName = "") where T : struct => OnSetLockedField(propertyName, ref lockedField, value, raiseOnlyIfNotEquals);
		protected bool SetLockedField(ref Locker<string> lockedField, string value, bool raiseOnlyIfNotEquals = true, [CallerMemberName] string propertyName = "") => OnSetLockedField(propertyName, ref lockedField, value, raiseOnlyIfNotEquals);
		private bool OnSetLockedField<T>(string? propertyName, ref Locker<T> lockedField, T value, bool raiseOnlyIfNotEquals)
		{
			if (propertyName == null) throw new ArgumentNullException("propertyName");
			var oldValue = OnGetLockedField(ref lockedField, propertyName);
			if (
				raiseOnlyIfNotEquals
				&&
				(
					(lockedField == null && value == null) ||
					(lockedField != null && lockedField.Read(_ => _) == null && value == null) ||
					(lockedField != null && lockedField.Read(_ => _) != null && lockedField.Read(_ => _)!.Equals(value))
					)
				)
				return false;
			//if (!OnRaisePropertyChanging(propertyName, oldValue, value)) return false;
			if (lockedField == null) lockedField = new Locker<T>(value);
			else lockedField.WriteObject(value);
			OnRaisePropertyChanged(propertyName, oldValue, value);
			return true;
		}
		protected T GetLockedField<T>(ref Locker<T> lockedField, [CallerMemberName] string propertyName = "") where T : struct => OnGetLockedField(ref lockedField, propertyName);
		protected string GetLockedField(ref Locker<string> lockedField, [CallerMemberName] string propertyName = "") => OnGetLockedField(ref lockedField, propertyName);
		private T OnGetLockedField<T>(ref Locker<T> lockedField, [CallerMemberName] string propertyName = "")
		{
			if (propertyName == null) throw new ArgumentNullException("propertyName");
			var value = lockedField == null ? default : lockedField.Read(_ => _);
			//T interceptedValue = OnRaisePropertyRead(propertyName, value);
			//if (EqualityComparer<T>.Default.Equals(value, interceptedValue)) return interceptedValue;
			//if (lockedField == null) lockedField = new ValueLocker<T>(interceptedValue);
			//else lockedField.WriteRef(interceptedValue);
			//return interceptedValue;
			return value!;
		}
		#endregion

		#region RaisePropertyChanged
		[EditorBrowsable(EditorBrowsableState.Never)]
		[XmlIgnore]
		[JsonIgnore]
		public virtual bool IsPropertyChangedEnabled { get => GetValue(() => true); set => SetValue(value); }
		protected void RaisePropertyChanged<T>(Expression<Func<object>> expression, T previousValue, T actualValue) => RaisePropertyChanged(expression.GetMemberName(), previousValue, actualValue);
		protected void RaisePropertyChanged<T>(string propertyName, T previousValue, T actualValue) => OnRaisePropertyChanged(propertyName, previousValue, actualValue);
		protected virtual void OnRaisePropertyChanged<T>(string propertyName, T previousValue, T actualValue)
		{
			if (!IsPropertyChangedEnabled) return;
			var action = new Action<string, T, T>((pro, pre, act) =>
			{
				PropertyChangedEvent?.Invoke(this, new PropertyChangedEventArgs(pro));
				PropertyChanged?.Invoke((TNotifier)(INotifier<TNotifier>)this, new NotifierPropertyChangedEventArgs<TNotifier>(pro, (TNotifier)(INotifier<TNotifier>)this, pre, act));
			});
			this.Invoke(action, propertyName, previousValue, actualValue);
		}
		#endregion

		#region Binding
		public INotifierBinding<TNotifier, TProperty> Binding<TProperty>(Expression<Func<TProperty>> sourcePropertyExpression)
			=> new NotifierBinding<TNotifier, TProperty>(this, sourcePropertyExpression);
		#endregion
	}
	public interface INotifierBinding<TNotifier, TProperty> where TNotifier : class, INotifier<TNotifier>
	{

	}

	internal class NotifierBinding<TNotifier, TProperty> : INotifierBinding<TNotifier, TProperty> where TNotifier : class, INotifier<TNotifier>
	{
		public NotifierBinding(Notifier<TNotifier> notifier, Expression<Func<TProperty>> sourcePropertyExpression)
		{
			Notifier = notifier;
			SourcePropertyExpression = sourcePropertyExpression;
		}
		public Notifier<TNotifier> Notifier { get; set; }
		public Expression<Func<TProperty>> SourcePropertyExpression { get; set; }
	}
	public static class INotifierBindingExtensions
	{
		public static void OneWayTo<TNotifier, TProperty>(this INotifierBinding<TNotifier, TProperty> me, object target, Expression<Func<TProperty>> targetPropertyExpression) where TNotifier : class, INotifier<TNotifier>
		{
			if (!(me is NotifierBinding<TNotifier, TProperty> not)) throw new InvalidCastException();
			not.Notifier.PropertyChanged += (s, e) => e.Case(not.SourcePropertyExpression, a => targetPropertyExpression.GetPropertyInfo().SetValue(target, a.ActualValue));
			targetPropertyExpression.GetPropertyInfo().SetValue(target, not.SourcePropertyExpression.GetPropertyInfo().GetValue(not.Notifier));
		}
		public static void OneWayTo<TNotifier, TProperty, TTargetProperty>(this INotifierBinding<TNotifier, TProperty> me, object target,
			Expression<Func<TTargetProperty>> targetPropertyExpression,
			Func<TProperty, TTargetProperty> transformFuction) where TNotifier : class, INotifier<TNotifier>
		{
			if (!(me is NotifierBinding<TNotifier, TProperty> not)) throw new InvalidCastException();
			not.Notifier.PropertyChanged += (s, e) => e.Case(not.SourcePropertyExpression, a => targetPropertyExpression.GetPropertyInfo().SetValue(target, transformFuction(a.ActualValue)));
			var val = (TProperty?)not.SourcePropertyExpression.GetPropertyInfo().GetValue(not.Notifier);
			// NULLABLE - Unexpected null
			if (val == null) throw new NullReferenceException($"Unexpected null");
			targetPropertyExpression.GetPropertyInfo().SetValue(target, transformFuction(val));
		}
		public static void TwoWayTo<TNotifier, TProperty, TTargetNotifier>(this INotifierBinding<TNotifier, TProperty> me, INotifier<TTargetNotifier> target, Expression<Func<TProperty>> targetPropertyExpression)
			where TNotifier : class, INotifier<TNotifier>
			where TTargetNotifier : class, INotifier<TTargetNotifier>
		{
			if (!(me is NotifierBinding<TNotifier, TProperty> not)) throw new InvalidCastException();
			not.Notifier.PropertyChanged += (s, e) => e.Case(not.SourcePropertyExpression, a => targetPropertyExpression.GetPropertyInfo().SetValue(target, a.ActualValue));
			target.PropertyChanged += (s, e) => e.Case(targetPropertyExpression, a => not.SourcePropertyExpression.GetPropertyInfo().SetValue(not.Notifier, a.ActualValue));
			targetPropertyExpression.GetPropertyInfo().SetValue(target, not.SourcePropertyExpression.GetPropertyInfo().GetValue(not.Notifier));
		}
	}
}
