namespace Fuxion;

public delegate void EventHandler<TSource, TEventArgs>(TSource source, TEventArgs args) where TEventArgs : EventArgs;