namespace Fuxion;

/// <summary>
///    Clase para implementar argumentos de eventos de un determinado tipo.
/// </summary>
/// <typeparam name="T">Tipo del parametro pasado.</typeparam>
public class EventArgs<T>(T value) : EventArgs
{
	/// <summary>
	///    Datos pasados como argumento del evento.
	/// </summary>
	public T Value { get; set; } = value;
}