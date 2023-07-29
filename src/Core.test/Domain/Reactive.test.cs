using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Timers;
using Fuxion.Testing;
using Microsoft.Reactive.Testing;
using Xunit;
using Xunit.Abstractions;
using Timer = System.Threading.Timer;

namespace Fuxion.Domain.Test;

public class ReactiveTest : BaseTest<ReactiveTest>
{
	public ReactiveTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "Reactive first")]
	public void First()
	{
		var numbers = new MySequenceOfNumbers();
		var observer = new MyConsoleObserver<int>(Output);
		numbers.Subscribe(observer);
		
		
	}
	[Fact(DisplayName = "Subject")]
	public void SubjectTest()
	{
		//Takes an IObservable<string> as its parameter. 
		//Subject<string> implements this interface.
		void WriteSequenceToConsole(IObservable<string> sequence)
		{
			//The next two lines are equivalent.
			//sequence.Subscribe(value=>Console.WriteLine(value));
			sequence.Subscribe(Output.WriteLine);
		}
		ISubject<string> subject = new Subject<string>();
		subject.OnNext("a");
		//WriteSequenceToConsole(subject);
		subject.Subscribe(Output.WriteLine);
		subject.OnNext("b");
		subject.OnNext("c");
		Output.WriteLine("===========================");
		subject = new ReplaySubject<string>();
		subject.OnNext("a");
		subject.Subscribe(Output.WriteLine);
		subject.OnNext("b");
		subject.OnNext("c");
		Output.WriteLine("===========================");
		var bufferSize = 2;
		subject = new ReplaySubject<string>(bufferSize);
		subject.OnNext("a");
		subject.OnNext("b");
		subject.OnNext("c");
		subject.Subscribe(Output.WriteLine);
		subject.OnNext("d");
		Output.WriteLine("===========================");
		var window = TimeSpan.FromMilliseconds(150);
		subject = new ReplaySubject<string>(window);
		subject.OnNext("w");
		Thread.Sleep(TimeSpan.FromMilliseconds(100));
		subject.OnNext("x");
		Thread.Sleep(TimeSpan.FromMilliseconds(100));
		subject.OnNext("y");
		subject.Subscribe(Output.WriteLine);
		subject.OnNext("z");
		Output.WriteLine("===========================");
		subject = new BehaviorSubject<string>("a");
		subject.Subscribe(Output.WriteLine);
		subject.OnNext("b");
		subject.OnNext("c");
		Output.WriteLine("===========================");
		subject = new BehaviorSubject<string>("a");
		subject.OnNext("b");
		subject.Subscribe(Output.WriteLine);
		subject.OnNext("c");
		subject.OnNext("d");
		Output.WriteLine("===========================");
		subject = new BehaviorSubject<string>("a");
		subject.OnNext("b");
		subject.OnNext("c");
		subject.OnCompleted();
		subject.Subscribe(Console.WriteLine);
		Output.WriteLine("===========================");
		subject = new AsyncSubject<string>();
		subject.OnNext("a");
		WriteSequenceToConsole(subject);
		subject.OnNext("b");
		subject.OnNext("c");
		subject.OnCompleted();
		Output.WriteLine("===========================");
		subject = new Subject<string>();
		subject.Subscribe(Console.WriteLine);
		subject.OnNext("a");
		subject.OnNext("b");
		subject.OnCompleted();
		subject.OnNext("c"); // This is an invalid usage
		Output.WriteLine("=== Factory");
		var subject2 = Subject.Create(new MyConsoleObserver<string>(Output), new MySequenceOfNumbers());
		subject2.OnNext("Hi");
		subject2.Subscribe(val => Output.WriteLine("val = " + val));
	}
	[Fact(DisplayName = "Lifetime")]
	public void Lifetime()
	{
		ISubject<int> subject = new Subject<int>();
		var firstSubscription = subject.Subscribe(value => 
			Output.WriteLine("1st subscription received {0}", value));
		var secondSubscription = subject.Subscribe(value => 
			Output.WriteLine("2nd subscription received {0}", value));
		subject.OnNext(0);
		subject.OnNext(1);
		subject.OnNext(2);
		subject.OnNext(3);
		firstSubscription.Dispose();
		Output.WriteLine("Disposed of 1st subscription");
		subject.OnNext(4);
		subject.OnNext(5);
		Output.WriteLine("===========================");
		subject = new Subject<int>();
		subject.Subscribe(
			i => Output.WriteLine(i.ToString()), 
			() => Output.WriteLine("Completed"));
		subject.OnNext(1);
		subject.OnCompleted();
		subject.OnNext(2);
		Output.WriteLine("===========================");
		var disposable = Disposable.Create(() => Output.WriteLine("Being disposed."));
		Output.WriteLine("Calling dispose...");
		disposable.Dispose();
		Output.WriteLine("Calling again...");
		disposable.Dispose();
	}
	[Fact(DisplayName = "Simple Factory")]
	public async Task SimpleFactory()
	{
		var singleValue = Observable.Return("Value");
		singleValue.Subscribe(Output.WriteLine);
		//which could have also been simulated with a replay subject
		ISubject<string> subject = new ReplaySubject<string>();
		subject.OnNext("Value");
		subject.OnCompleted();
		Output.WriteLine("===========================");
		var throws = Observable.Throw<string>(new Exception($"FAILED"));
		Output.WriteLine($"Throws => {Assert.Throws<Exception>(() => throws.Subscribe(_ => { }, onError: ex => throw ex)).Message}");
		//Behaviorally equivalent to
		subject = new ReplaySubject<string>(); 
		subject.OnError(new Exception());
		Output.WriteLine("===========================");
		var printGO = Observable.Create<string>(o =>
		{
			o.OnNext("One");
			return () => Output.WriteLine("Disposed");
		});
		var dis = printGO.Subscribe(Output.WriteLine);
		dis.Dispose();
		Output.WriteLine("===========================");
		var ob = Observable.Create<string>(
			observer =>
			{
				var timer = new System.Timers.Timer();
				timer.Interval = 300;
				void Elapsed(object? sender, ElapsedEventArgs e)
				{
					observer.OnNext("tick");
					Output.WriteLine(e.SignalTime.ToString());
				}
				timer.Elapsed += Elapsed;
				timer.Start();
				return () =>
				{
					timer.Elapsed -= Elapsed;
					timer.Dispose();
				};
			});
		var subscription = ob.Subscribe(Output.WriteLine);
		await Task.Delay(1000);
		subscription.Dispose();
		Output.WriteLine("===========================");
		var interval = Observable.Interval(TimeSpan.FromMilliseconds(250));
		dis = interval.Subscribe(
			_ => Output.WriteLine(_.ToString()),
			() => Output.WriteLine("completed"));
		await Task.Delay(1000);
		dis.Dispose();
		Output.WriteLine("===========================");
		var timer = Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(250));//,DateTimeOffset.Now.AddMilliseconds(500));//TimeSpan.FromMilliseconds(500));
		dis = timer.Subscribe(
			_ => Output.WriteLine(_.ToString()),
			() => Output.WriteLine("completed"));
		await Task.Delay(1000);
		dis.Dispose();
		Output.WriteLine("===========================");
		var start = Observable.Start(() =>
		{
			Output.WriteLine("Working away");
			for (int i = 0; i < 5; i++)
			{
				Thread.Sleep(100);
				Output.WriteLine(".");
			}
		});
		dis = start.Subscribe(
			unit => Output.WriteLine("Unit published"), 
			() => Output.WriteLine("Action completed"));
		await Task.Delay(1000);
		dis.Dispose();
		Output.WriteLine("===========================");
		var t = Task.Factory.StartNew(()=>"Test");
		var source = t.ToObservable();
		source.Subscribe(
			Output.WriteLine,
			() => Output.WriteLine("completed"));
	}
	[Fact(DisplayName = "Finally")]
	public void Finally()
	{
		var source = new Subject<int>();
		var result = source.Finally(() => Console.WriteLine("Finally"));
		result.Subscribe(i => Output.WriteLine(i.ToString()),
			//Console.WriteLine,
			ex => Output.WriteLine($"Error '{ex.GetType().Name}': {ex.Message}"),
			() => Output.WriteLine("Completed"));
		source.OnNext(1);
		source.OnNext(2);
		source.OnNext(3);
		//Brings the app down. Finally action is not called.
		source.OnError(new Exception("Fail"));
	}
	[Fact(DisplayName = "Test scheduler")]
	public void TestScheduler()
	{
		var scheduler = new TestScheduler();
		var source = scheduler.CreateHotObservable(new Recorded<Notification<long>>(10000000, Notification.CreateOnNext(0L)), new Recorded<Notification<long>>(20000000, Notification.CreateOnNext(1L)),
			new Recorded<Notification<long>>(30000000, Notification.CreateOnNext(2L)), new Recorded<Notification<long>>(40000000, Notification.CreateOnNext(3L)),
			new Recorded<Notification<long>>(40000000, Notification.CreateOnCompleted<long>()));
		var testObserver = scheduler.Start(
			() => source,
		0,
		TimeSpan.FromSeconds(1).Ticks,
		TimeSpan.FromSeconds(5).Ticks);
		Output.WriteLine("Time is {0} ticks", scheduler.Clock);
		Output.WriteLine("Received {0} notifications", testObserver.Messages.Count);
		foreach (Recorded<Notification<long>> message in testObserver.Messages)
		{
			Output.WriteLine("  {0} @ {1}", message.Value, message.Time);
		}
	}
	[Fact]
	public void AAA()
	{
		var s = new Source<Command>();
		var s2 = s.OfType<RenameCommand>();
		s2.Subscribe(c => Output.WriteLine($"Command received {c}"),
			ex=>Output.WriteLine($"Error occurred {ex}"),
			()=>Output.WriteLine($"Command listening stopped !"));
		s.Post(new Command());
		s.Post(new RenameCommand(""));

		s.StringDemo.Subscribe(c => Output.WriteLine($"Property was setted with value={c}"));
		s.StringDemo.Value = "nuevo";
		Assert.Equal("nuevo",s.StringDemo);
		
		
		// Comandos
		
		// Subc Respuesta
		
		// Lanzo el comando
	}
	public record Person
	{
		public Person(string firstName, string lastName) => (FirstName, LastName) = (firstName, lastName);
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string[] PhoneNumbers { get; set; } = Array.Empty<string>();
	}
	[Fact]
	public void BBB()
	{
		Person person1 = new("Nancy", "Davolio") { PhoneNumbers = new string[1] };
		Output.WriteLine(person1.ToString());
		// output: Person { FirstName = Nancy, LastName = Davolio, PhoneNumbers = System.String[] }

		var person2 = person1 with { FirstName = "John" };
		Output.WriteLine(person2.ToString());
		// output: Person { FirstName = John, LastName = Davolio, PhoneNumbers = System.String[] }
		Output.WriteLine((person1 == person2).ToString()); // output: False

		person2 = person1 with { PhoneNumbers = new string[1] };
		Output.WriteLine(person2.ToString());
		// output: Person { FirstName = Nancy, LastName = Davolio, PhoneNumbers = System.String[] }
		Output.WriteLine((person1 == person2).ToString()); // output: False

		person2 = person1 with { };
		Output.WriteLine((person1 == person2).ToString()); // output: True
	}
}

public class Source<T> : IObservable<T>
{
	readonly Subject<T> subject = new();
	public IDisposable Subscribe(IObserver<T> observer) => subject.Subscribe(observer);
	public void Post(T value) => subject.OnNext(value);

	// BehaviorSubject<string> bs = new("default");
	public Property<string> StringDemo { get; } = string.Empty;
	public ReadOnlyProperty<string?> NullableStringDemo { get; } = default(string);
	public ReadOnlyProperty<Event> Agg { get; } = default(Event)!;

}

public class Agg
{
	Source<object> s = new();
	CompositeDisposable dis = new();
	public Agg()
	{
		FullName = new(_fullName);
		s.Post(new CreatedEvent());
		dis.Add(s.OfType<RenameCommand>().Subscribe(c => FirstName.Value = c.NewName));
		dis.Add(s.OfType<RenamedEvent>().Subscribe(c => _fullName.Value = FirstName + " " + LastName));
	}
	readonly Property<string> _fullName = "";
	public ReadOnlyProperty<string> FullName { get; }
	public Property<string> FirstName { get; } = "";
	public Property<string> LastName { get; } = "";
	public Property<int> Age { get; } = default(int);
}

public class ReadOnlyProperty<T> : IObservable<T>
{
	public ReadOnlyProperty(Property<T> property)
	{
		_property = property;
	}
	protected readonly Property<T> _property;
	public static implicit operator T(ReadOnlyProperty<T> pro) => pro._property.Value;
	public static implicit operator ReadOnlyProperty<T>(T value) => new (value);
	public IDisposable Subscribe(IObserver<T> observer) => _property.Subscribe(observer);
}
/// <typeparam name="T">MUST BE INMUTABLE</typeparam>
public class Property<T> : IObservable<T>
{
	Property(T defaultValue) 
	{
		_subject = new(defaultValue);
	}
	protected readonly BehaviorSubject<T> _subject;
	public T Value
	{
		get => _subject.Value;
		set => _subject.OnNext(value);
	}
	public static implicit operator T(Property<T> pro) => pro.Value;
	public static implicit operator Property<T>(T value) => new (value);
	public IDisposable Subscribe(IObserver<T> observer) => _subject.Subscribe(observer);
}
// public class NodeProperty<T> : IObservable<T>
// {
// 	NodeProperty(T defaultValue) 
// 	{
// 		// _subject = new(defaultValue);
// 		_node.Producer.Produce(defaultValue);
// 	}
// 	INode _node;
// 	// protected readonly BehaviorSubject<T> _subject;
// 	public T Value
// 	{
// 		get => _subject.Value;
// 		set => _subject.OnNext(value);
// 	}
// 	public static implicit operator T(NodeProperty<T> pro) => pro.Value;
// 	public static implicit operator NodeProperty<T>(T value) => new (value);
// 	public IDisposable Subscribe(IObserver<T> observer) => _subject.Subscribe(observer);
// }

public record PropertyChangedEvent<T>
{
	public PropertyChangedEvent(string propertyName, T newValue) => (PropertyName, NewValue) = (propertyName, newValue);
	public string PropertyName { get; set; }
	public T NewValue { get; set; }
}
public record Event();
public record CreatedEvent() : Event;

public record RenamedEvent : Event
{
	public RenamedEvent(string newFirstName, string newLastName) => (NewFirstName, NewLastName) = (newFirstName, newLastName);
	public string NewFirstName { get; set; }
	public string NewLastName { get; set; }
}
public record Command();

public record RenameCommand : Command
{
	public RenameCommand(string newName) => NewName = newName;
	public string NewName { get; set; }
}
public class MyConsoleObserver<T> : IObserver<T>
{
	ITestOutputHelper _output;
	public MyConsoleObserver(ITestOutputHelper output)
	{
		_output = output;
	}
	public void OnNext(T value)
	{
		_output.WriteLine("Received value {0}", value);
	}
	public void OnError(Exception error)
	{
		_output.WriteLine("Sequence faulted with {0}", error);
	}
	public void OnCompleted()
	{
		_output.WriteLine("Sequence terminated");
	}
}
public class MySequenceOfNumbers : IObservable<int>
{
	public IDisposable Subscribe(IObserver<int> observer)
	{
		observer.OnNext(1);
		observer.OnNext(2);
		observer.OnNext(3);
		observer.OnCompleted();
		return Disposable.Empty;
	}
}