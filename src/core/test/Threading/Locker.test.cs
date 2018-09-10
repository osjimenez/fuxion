using Fuxion.Threading;
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
			var obj = new Locker<MockForLock>(new MockForLock());
			string name = null;
			await obj.ReadAsync(m =>
			{
				name = m.Name;
			});
		}
		[Fact(DisplayName = "Locker - void ReadAsync(P1)")]
		public async Task ReadAsyncP1()
		{
			var obj = new Locker<MockForLock>(new MockForLock());
			string name = null;
			await obj.ReadAsync((l, p) =>
			{
				name = l.Name;
			}, 1);
		}
		[Fact(DisplayName = "Locker - string ReadAsync()")]
		public async Task StringReadAsync()
		{
			var obj = new Locker<MockForLock>(new MockForLock());
			string name = await obj.ReadAsync(m =>
			{
				return m.Name;
			});
		}
		[Fact(DisplayName = "Locker - string ReadAsync(P1)")]
		public async Task StringReadAsyncP1()
		{
			var obj = new Locker<MockForLock>(new MockForLock());
			string name = await obj.ReadAsync((l, p) =>
			{
				return l.Name;
			}, 1);
		}
		[Fact(DisplayName = "Locker - void WriteAsync()")]
		public async Task WriteAsync()
		{
			var obj = new Locker<MockForLock>(new MockForLock());
			string name = null;
			await obj.WriteAsync(m =>
			{
				m.Name = "test";
			});
		}
		[Fact(DisplayName = "Locker - Nested Write()")]
		public async Task NestedWrite()
		{
			var obj = new Locker<MockForLock>(new MockForLock());
			string name = null;
			await obj.WriteAsync(m =>
			{
				obj.Write(m2 =>
				{
					m2.Name = "test";
				});
			});
		}
	}
	public class MockForLock
	{
		public int id { get; set; }
		public string Name { get; set; }
	}
}
