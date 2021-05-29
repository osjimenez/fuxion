using Fuxion.Testing;
using Fuxion.Threading;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test.Threading
{
	public class LockerTest : BaseTest
	{
		public LockerTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
		[Fact(DisplayName = "Locker - void ReadAsync()")]
		public async Task ReadAsync()
		{
			var obj = new Locker<MockForLock>(new MockForLock(1, "test"));
			string name = "";
			await obj.ReadAsync(m =>
			{
				name = m.Name;
			});
			Assert.Equal("test", name);
		}
		[Fact(DisplayName = "Locker - void ReadAsync(P1)")]
		public async Task ReadAsyncP1()
		{
			var obj = new Locker<MockForLock>(new MockForLock(1, "test"));
			string name = "";
			await obj.ReadAsync((l, p) =>
			{
				name = l.Name;
				Assert.Equal(1, p);
			}, 1);
			Assert.Equal("test", name);
		}
		[Fact(DisplayName = "Locker - string ReadAsync()")]
		public async Task StringReadAsync()
		{
			var obj = new Locker<MockForLock>(new MockForLock(1, "test"));
			string name = await obj.ReadAsync(m =>
			{
				return m.Name;
			});
			Assert.Equal("test", name);
		}
		[Fact(DisplayName = "Locker - string ReadAsync(P1)")]
		public async Task StringReadAsyncP1()
		{
			var obj = new Locker<MockForLock>(new MockForLock(1, "test"));
			string name = await obj.ReadAsync((l, p) =>
			{
				Assert.Equal(1, p);
				return l.Name;
			}, 1);
			Assert.Equal("test", name);
		}
		[Fact(DisplayName = "Locker - void WriteAsync()")]
		public async Task WriteAsync()
		{
			var obj = new Locker<MockForLock>(new MockForLock(1, "noTest"));
			await obj.WriteAsync(m =>
			{
				m.Name = "test";
			});
			Assert.Equal("test", obj.Read(_ => _.Name));
		}
		[Fact(DisplayName = "Locker - Nested Write()")]
		public async Task NestedWrite()
		{
			var obj = new Locker<MockForLock>(new MockForLock(1, "noTest"));
			await obj.WriteAsync(m =>
			{
				obj.Write(m2 =>
				{
					m2.Name = "test";
				});
			});
			Assert.Equal("test", obj.Read(_ => _.Name));
		}
	}
	public class MockForLock
	{
		public MockForLock(int id, string name)
		{
			Id = id;
			Name = name;
		}
		public int Id { get; set; }
		public string Name { get; set; }
	}
}
