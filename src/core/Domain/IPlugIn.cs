namespace Fuxion.Domain;

public interface IPlugIn
{
	
}

public interface IPluggable
{
	
}

public class Aggregate
{
	
}

public class State
{
	
}
// Sources y Destinations en vez de Handlers y Dispatchers respectivamente ?
// Los sources son IObservables para gestionar las subscripciones
// No hacen falta los States, los sources son States
// Al enviar un mensaje a un Destination, nos devuelve un Source. Este source contiene la respuesta al mensaje, cuando llegue, sincrono o asincronoro, eso se modela en otra capa/sitio/lugar/...
// Una forma de limitar los mensajes a ambitos, son los TypeKey. Esto puede ser una pata importante para la certificacion, validacion, etc de las fuentes y los destinos.
// Esto va con el Policy, los TypeKey no son los AuthenticationMethods?
// La cuestion es, una determinada TypeKey viene de un ambito distinto en cuanto que

// Los Sources context son 

public interface IDispatcher<THandler> : IPluggable
{
	
}

public interface IObservableDispatcher : IDispatcher<IHandler<IMessage>> // For events
{
	
}
public class RouterDispatcher : IDispatcher<IHandler<IMessage>>
{
	
}

public class RestDispatcher : IDispatcher<OrchestatiorHandler>
{
	
}

public class ServiceProviderDispatcher : IDispatcher<IHandler<IMessage>>
{
	
}


public interface IHandler<TMessage>
{
	
}

public class TaskHandler : IHandler<IMessage> // TasksProxy of Ordiner.Tasks.Shell.Wpf project ? 
{
	
}
public class OrderHandler : IHandler<IMessage> //  In the object, before saving, pending changes. ChangeHandler, maybe?
{
	
}
public class OrchestatiorHandler : IHandler<IMessage>
{
	
}
public interface IMessage
{
	
}


public record MyMessage(string Name, int Age);

public class MyAggregate
{
	public void HandleMessage(MyMessage msg) => MakeMessage(msg.Name, msg.Age);
	public void MakeMessage(string Name, int Age)
	{
		
	}
}