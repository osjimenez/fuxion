using System;
namespace Fuxion
{
	/// <summary>
	/// Clase para implementar argumentos de eventos de un determinado tipo.
	/// </summary>
	/// <typeparam name="T">Tipo del parametro pasado.</typeparam>
	public class EventArgs<T> : EventArgs
	{
		T _Value;
		/// <summary>
		/// Datos pasados como argumento del evento.
		/// </summary>
		public T Value
		{
			get { return _Value; }
			set { _Value = value; }
		}
		/// <summary>
		/// Inicializa una nueva instancia de la clase.
		/// </summary>
		/// <param name="value"></param>
		public EventArgs(T value)
		{
			_Value = value;
		}
	}
}
