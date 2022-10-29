namespace Fuxion;

/// <summary>
///    Clase para implementar argumentos de eventos de un determinado tipo.
/// </summary>
/// <typeparam name="T">Tipo del parametro pasado.</typeparam>
public class EventArgs<T> : EventArgs
{
	/// <summary>
	///    Inicializa una nueva instancia de la clase.
	/// </summary>
	/// <param name="value"></param>
	public EventArgs(T value) => Value = value;
	/// <summary>
	///    Datos pasados como argumento del evento.
	/// </summary>
	public T Value { get; set; }
}