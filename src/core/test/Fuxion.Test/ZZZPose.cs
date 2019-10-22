using Pose;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Fuxion.Test
{
	public class ZZZPose
	{
		[Fact]
		public void Pose()
		{
			// Shim static method
			Shim consoleShim = Shim.Replace(() => Console.WriteLine(Is.A<string>())).With(
				delegate (string s) { Console.WriteLine("Hijacked: {0}", s); });
			// Shim static property getter
			Shim dateTimeShim = Shim.Replace(() => DateTime.Now).With(() => new DateTime(2004, 4, 4));
			// Shim static property setter
			Shim setterShim = Shim.Replace(() => Console.Title, true).With((string title) => { Console.Title = "My Title"; });
			// Shim instance property getter
			Shim classPropShim = Shim.Replace(() => Is.A<MyClass>().MyProperty).With((MyClass @this) => 100);
			// Shim instance property setter
			Shim classPropShim1 = Shim.Replace(() => Is.A<MyClass>().MyProperty, true).With((MyClass @this, int prop) => { @this.MyProperty = prop * 10; });
			// Shim constructor
			Shim ctorShim = Shim.Replace(() => new MyClass()).With(() => new MyClass() { MyProperty = 10 });
			// Shim instance method of a Reference Type
			Shim classShim = Shim.Replace(() => Is.A<MyClass>().DoSomething()).With(
				delegate (MyClass @this) { Console.WriteLine("doing someting else"); });
			// Shim method of specific instance of a Reference Type
			MyClass myClass = new MyClass();
			Shim myClassShim = Shim.Replace(() => myClass.DoSomething()).With(
				delegate (MyClass @this) { Console.WriteLine("doing someting else with myClass"); });
			// Shim instance method of a Value Type
			Shim structShim = Shim.Replace(() => Is.A<MyStruct>().DoSomething()).With(
				delegate (ref MyStruct @this) { Console.WriteLine("doing someting else"); });

			// This block executes immediately
			PoseContext.Isolate(() =>
			{
				// All code that executes within this block
				// is isolated and shimmed methods are replaced

				// Outputs "Hijacked: Hello World!"
				Console.WriteLine("Hello World!");

				// Outputs "4/4/04 12:00:00 AM"
				Console.WriteLine(DateTime.Now);

				// Outputs "doing someting else"
				new MyClass().DoSomething();

				// Outputs "doing someting else with myClass"
				myClass.DoSomething();

			}, consoleShim, dateTimeShim, classPropShim, classShim, myClassShim, structShim);
		}
	}
	struct MyStruct {
		public void DoSomething() => Console.WriteLine("doing someting");
	}
	class MyClass
	{
		public int MyProperty { get; set; }
		public void DoSomething() => Console.WriteLine("doing someting");
	}
}
