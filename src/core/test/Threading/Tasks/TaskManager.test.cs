using Fuxion.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test.Threading.Tasks
{
    public class TaskManagerTest : BaseTest
    {
		public TaskManagerTest(ITestOutputHelper output) : base(output)
		{
			Printer.WriteLineAction = m =>
			{
				try
				{
					output.WriteLine($"({Task.CurrentId}) {m}");
					Debug.WriteLine($"({Task.CurrentId}) {m}");
				}
				catch { }
			};
		}
        #region Help methods
        private Task Start_void()
        {
            Printer.WriteLine($"Start at {DateTime.Now.ToLongTimeString()}");
            return null;
        }
        private Task<int> Start_int()
        {
            Printer.WriteLine($"Start at {DateTime.Now.ToLongTimeString()}");
            return null;
        }
        private void Do_void(Task task, params int[] pars)
        {
            Printer.WriteLine($"Parameters = '{pars.Aggregate("", (a, c) => $"{a}-{c}", a => a.Trim('-'))}'");
            try
            {
                task.Sleep(TimeSpan.FromMilliseconds(1));
            }
            catch (Exception ex)
            {
                Printer.WriteLine("Exception: " + ex.Message);
                Assert.True(false, "Task is not managed by TaskManager");
            }
            Printer.WriteLine($"Finished at {DateTime.Now.ToLongTimeString()}");
        }
        private async Task Do_void_async(Task task, params int[] pars)
        {
            Printer.WriteLine($"Parameters = '{pars.Aggregate("", (a, c) => $"{a}-{c}", a => a.Trim('-'))}'");
            await Task.Delay(100);
            try
            {
                task.Sleep(TimeSpan.FromMilliseconds(100));
            }
            catch (Exception ex)
            {
                Printer.WriteLine("Exception: " + ex.Message);
                Assert.True(false, "Task is not managed by TaskManager");
            }
            Printer.WriteLine($"Finished at {DateTime.Now.ToLongTimeString()}");
        }
        private int Do_int(Task task, params int[] pars)
        {
            Do_void(task, pars);
            return 2;
        }
        private async Task<int> Do_int_async(Task task, params int[] pars)
        {
            await Do_void_async(task, pars);
            return 2;
        }
        private void Assert_void(Task task)
        {
            if (task is Task<Task>)
            {
                Printer.WriteLine("Return task is Task<Task>> !!!");
                (task as Task<Task>).Unwrap().Wait();
            }
            else task.Wait();
        }
        private void Assert_int(Task<int> task)
        {
            var res = task.Result;
            Assert.Equal(2, res);
        }
        #endregion
        [Fact(DisplayName = "TaskManager - SleepCancelation")]
        public void TaskManager_SleepCancelation()
        {
            Task task = null;
            var dt = DateTime.Now;
            Printer.WriteLine("Inicio en " + dt.ToString("HH:mm:ss.fff"));
            task = TaskManager.StartNew(() => {
                //task.Sleep(TimeSpan.FromMilliseconds(2500), TimeSpan.FromMilliseconds(500));
                task.Sleep(TimeSpan.FromMilliseconds(2500));
            });
            task.CancelAndWait();
            Printer.WriteLine("Cancelado en " + DateTime.Now.ToString("HH:mm:ss.fff"));
            Assert.True(dt.AddSeconds(1) > DateTime.Now);
            //Assert.True(dt.AddMilliseconds(400) < DateTime.Now);
        }
        [Fact(DisplayName = "TaskManager - Sleep")]

		public Task TaskManager_Sleep()
        {
            Task task = null;
            var dt = DateTime.Now;
            task = TaskManager.StartNew(() => {
                //task.Sleep(TimeSpan.FromMilliseconds(2500), TimeSpan.FromMilliseconds(500));
                task.Sleep(TimeSpan.FromMilliseconds(2500));
            });
            return task.ContinueWith(_=> {
                Assert.True(dt.AddSeconds(2) < DateTime.Now);
            });
        }

		public static IEnumerable<object[]> GenerateStartNewValues(int length)
		{
			var res = new List<object[]>();
			for (int i = 0; i < System.Math.Pow(2, length); i++)
			{
				BitArray b = new BitArray(new int[] { i });
				var bits = b.Cast<bool>().Take(length).ToArray();
				// Solo dejo los @void y sync
				//if (!bits[4])
					res.Add(bits.Cast<object>().ToArray());
			}
			return res;
		}
		[Theory(DisplayName = "TaskManager - StartNew")]
		[MemberData(nameof(GenerateStartNewValues), 6)]
		public async void TaskManager_StartNew(bool @void, bool sync, bool create, bool sequentially, bool onlyLast, bool cancel)//, bool wait)
		{
			// test constants
			int runDelay = 500;
			int interval = runDelay / 10;
			string cancelledResult = "Canceled";
			string doneResult = "Done";

			var concurrencyProfile = new ConcurrencyProfile
			{
				Sequentially = sequentially,
				ExecuteOnlyLast = onlyLast,
				CancelPrevious = cancel,
				//WaitForCancelPrevious = wait
			};
			object seqLock = new object();
			string seq = null;
			int seqNum = 1;
			#region Run modes
			Task Action_Sync()
			{
				void Do()
				{
					var mySeqNum = seqNum++;
					Printer.WriteLine("Do");
					try
					{
						lock (seqLock)
						{
							seq += $"S{mySeqNum}-";
							Printer.WriteLine("Start");
						}
						Task.Delay(runDelay, TaskManager.Current.GetCancellationToken().Value).Wait();
						lock (seqLock)
						{
							seq += $"E{mySeqNum}-";
							Printer.WriteLine("End");
						}
					}
					catch
					{
						lock (seqLock)
						{
							seq += $"E{mySeqNum}X-";
							Printer.WriteLine("End - in catch");
						}
						throw;
					}
				}
				if (create)
				{
					var task = TaskManager.Create(() => Do(), concurrencyProfile: concurrencyProfile);
					task.Start();
					return task;
				}
				else return TaskManager.StartNew(() => Do(), concurrencyProfile: concurrencyProfile);
			}
			Task Action_Async()
			{
				async Task Do()
				{
					var mySeqNum = seqNum++;
					Printer.WriteLine("Do");
					try
					{
						lock (seqLock)
						{
							seq += $"S{mySeqNum}-";
							Printer.WriteLine("Start");
						}
						await Task.Delay(runDelay, TaskManager.Current.GetCancellationToken().Value);
						lock (seqLock)
						{
							seq += $"E{mySeqNum}-";
							Printer.WriteLine("End");
						}
					}
					catch
					{
						lock (seqLock)
						{
							seq += $"E{mySeqNum}X-";
							Printer.WriteLine("End - in catch");
						}
						throw;
					}
				}
				if (create)
				{
					var task = TaskManager.Create(async () => await Do(), concurrencyProfile: concurrencyProfile);
					task.Start();
					return task;
				}
				else return TaskManager.StartNew(async () => await Do(), concurrencyProfile: concurrencyProfile);
			}
			Task<string> Func_Sync()
			{
				string Do()
				{
					var mySeqNum = seqNum++;
					Printer.WriteLine("Do");
					try
					{
						lock (seqLock)
						{
							seq += $"S{mySeqNum}-";
							Printer.WriteLine("Start");
						}
						Task.Delay(runDelay, TaskManager.Current.GetCancellationToken().Value).Wait();
						lock (seqLock)
						{
							seq += $"E{mySeqNum}-";
							Printer.WriteLine("End");
						}
						return doneResult;
					}
					catch
					{
						lock (seqLock)
						{
							seq += $"E{mySeqNum}X-";
							Printer.WriteLine("End - in catch");
						}
						throw;
					}
				}
				if (create)
				{
					var task = TaskManager.Create(() => Do(), concurrencyProfile: concurrencyProfile);
					task.Start();
					return task;
				}
				else return TaskManager.StartNew(() => Do(), concurrencyProfile: concurrencyProfile);
			}
			Task<string> Func_Async()
			{
				async Task<string> Do()
				{
					var mySeqNum = seqNum++;
					Printer.WriteLine("Do");
					try
					{
						lock (seqLock)
						{
							seq += $"S{mySeqNum}-";
							Printer.WriteLine("Start");
						}
						await Task.Delay(runDelay, TaskManager.Current.GetCancellationToken().Value);
						lock (seqLock)
						{
							seq += $"E{mySeqNum}-";
							Printer.WriteLine("End");
						}
						return doneResult;
					}
					catch
					{
						lock (seqLock)
						{
							seq += $"E{mySeqNum}X-";
							Printer.WriteLine("End - in catch");
						}
						throw;
					}
				}
				if (create)
				{
					var task = TaskManager.Create(async () => await Do(), concurrencyProfile: concurrencyProfile);
					task.Start();
					return task;
				}
				else return TaskManager.StartNew(async () => await Do(), concurrencyProfile: concurrencyProfile);
			}
			#endregion

			var results = new(bool WasCancelled, string Result)[3];

			#region Run
			Task<(bool WasCancelled, string Result)>[] res = new Task<(bool WasCancelled, string Result)>[3];
			for (int i = 0; i < 3; i++)
			{
				res[i] = Task.Run(async () =>
				{
					try
					{
						if (@void)
						{
							if (sync)
								await Action_Sync();
							else
								await Action_Async();
							return (false, doneResult);
						}
						else
						{
							return sync
								? (false, await Func_Sync())
								: (false, await Func_Async());
						}
					}
					catch (TaskCanceledByConcurrencyException)
					{
						Printer.WriteLine("TaskCanceledByConcurrencyException");
						return (true, cancelledResult);
					}
					catch (TaskCanceledException)
					{
						Printer.WriteLine("TaskCanceledException");
						return (true, cancelledResult);
					}
					catch (AggregateException ex) when (ex.Flatten().InnerException is TaskCanceledException)
					{
						Printer.WriteLine("AggregateException");
						return (true, cancelledResult);
					}
				});
				await Task.Delay(interval);
			}
			await Task.WhenAll(res);
			results[0] = res[0].Result;
			results[1] = res[1].Result;
			results[2] = res[2].Result;
			seq = seq.Trim('-');
			for (var i = 0; i < results.Length; i++)
				Printer.WriteLine($"Result {i}: WasCanceled<{results[i].WasCancelled}>,Result<{results[i].Result}>");
			Printer.WriteLine($"Sequence: " + seq);
			#endregion

			#region Assert

			if (sequentially)
			{
				if (cancel)
				{
					if (onlyLast)
					{
						Assert.Equal("S1-E1", seq);
					}
					else
					{
						Assert.Equal("S1-E1X-S2-E2X-S3-E3", seq);
					}
				}
				else
				{
					if (onlyLast)
					{
						Assert.Equal("S1-E1", seq);
					}
					else
					{
						Assert.Equal("S1-E1-S2-E2-S3-E3", seq);
					}
				}
			}
			else
			{
				if (cancel)
				{
					if (onlyLast)
					{
						Assert.Contains("S1", seq);
						Assert.Contains("E1", seq);
					}
					else
					{
						Assert.Contains("S1", seq);
						Assert.Contains("E1X", seq);
						Assert.Contains("S2", seq);
						Assert.Contains("E2X", seq);
						Assert.Contains("S3", seq);
						Assert.Contains("E3", seq);
					}
				}
				else
				{
					if (onlyLast)
					{
						Assert.Contains("S1", seq);
						Assert.Contains("E1", seq);
					}
					else
					{
						Assert.Contains("S1", seq);
						Assert.Contains("E1", seq);
						Assert.Contains("S2", seq);
						Assert.Contains("E2", seq);
						Assert.Contains("S2", seq);
						Assert.Contains("E2", seq);
					}
				}
			}
			


			//if (cancel)
			//{
			//	Assert.True(results[0].WasCancelled);
			//	Assert.True(results[1].WasCancelled);
			//	Assert.True(!results[2].WasCancelled);
			//	if (!@void)
			//	{
			//		Assert.Equal(cancelledResult, results[0].Result);
			//		Assert.Equal(cancelledResult, results[1].Result);
			//		Assert.Equal(doneResult, results[2].Result);
			//	}
			//}
			//else
			//{
			//	if (!@void)
			//	{
			//		if (onlyLast)
			//		{
			//			Assert.Equal(cancelledResult, results[0].Result);
			//			Assert.Equal(cancelledResult, results[1].Result);
			//			Assert.Equal(doneResult, results[2].Result);
			//		}
			//		else
			//		{
			//			Assert.Equal(doneResult, results[0].Result);
			//			Assert.Equal(doneResult, results[1].Result);
			//			Assert.Equal(doneResult, results[2].Result);
			//		}
			//	}
			//}
			//if (sequentially)
			//{
			//	if(cancel)
			//		Assert.Equal("S1-E1X-S2-E2X-S3-E3", seq);
			//	else if(onlyLast)
			//		Assert.Equal("S1-E1", seq);
			//	else
			//		Assert.Equal("S1-E1-S2-E2-S3-E3", seq);
			//}
			//if (onlyLast)
			//{
			//	//if (cancel)
			//	//	Assert.Equal("S1-E1X", seq);
			//	//else
			//	//	Assert.Equal("", seq);
			//}
			#endregion
		}
		#region void_StartNew
		[Fact]
        public void TaskManager_void_StartNew()
        {
            var task = Start_void();
            task = TaskManager.StartNew(() => Do_void(task));
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_p1()
        {
            var task = Start_void();
            task = TaskManager.StartNew(p1 => Do_void(task, p1), 1);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_p1_p2()
        {
            var task = Start_void();
            task = TaskManager.StartNew((p1, p2) => Do_void(task, p1, p2), 1, 2);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_p1_p2_p3()
        {
            var task = Start_void();
            task = TaskManager.StartNew((p1, p2, p3) => Do_void(task, p1, p2, p3), 1, 2, 3);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_p1_p2_p3_p4()
        {
            var task = Start_void();
            task = TaskManager.StartNew((p1, p2, p3, p4) => Do_void(task, p1, p2, p3, p4), 1, 2, 3, 4);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_p1_p2_p3_p4_p5()
        {
            var task = Start_void();
            task = TaskManager.StartNew((p1, p2, p3, p4, p5) => Do_void(task, p1, p2, p3, p4, p5), 1, 2, 3, 4, 5);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_p1_p2_p3_p4_p5_p6()
        {
            var task = Start_void();
            task = TaskManager.StartNew((p1, p2, p3, p4, p5, p6) => Do_void(task, p1, p2, p3, p4, p5, p6), 1, 2, 3, 4, 5, 6);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_p1_p2_p3_p4_p5_p6_p7()
        {
            var task = Start_void();
            task = TaskManager.StartNew((p1, p2, p3, p4, p5, p6, p7) => Do_void(task, p1, p2, p3, p4, p5, p6, p7), 1, 2, 3, 4, 5, 6, 7);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_p1_p2_p3_p4_p5_p6_p7_p8()
        {
            var task = Start_void();
            task = TaskManager.StartNew((p1, p2, p3, p4, p5, p6, p7, p8) => Do_void(task, p1, p2, p3, p4, p5, p6, p7, p8), 1, 2, 3, 4, 5, 6, 7, 8);
            Assert_void(task);
        }
        #endregion
        #region void_StartNew_async
        [Fact]
        public void TaskManager_void_StartNew_async()
        {
            var task = Start_void();
            task = TaskManager.StartNew(async () => await Do_void_async(task));
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_Startnew_async_p1()
        {
            Task task = Start_void();
            task = TaskManager.StartNew(async p1 => await Do_void_async(task, p1), 1);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_async_p1_p2()
        {
            Task task = Start_void();
            task = TaskManager.StartNew(async (p1, p2) => await Do_void_async(task, p1, p2), 1, 2);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_async_p1_p2_p3()
        {
            Task task = Start_void();
            task = TaskManager.StartNew(async (p1, p2, p3) => await Do_void_async(task, p1, p2, p3), 1, 2, 3);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_async_p1_p2_p3_p4()
        {
            Task task = Start_void();
            task = TaskManager.StartNew(async (p1, p2, p3, p4) => await Do_void_async(task, p1, p2, p3, p4), 1, 2, 3, 4);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_async_p1_p2_p3_p4_p5()
        {
            Task task = Start_void();
            task = TaskManager.StartNew(async (p1, p2, p3, p4, p5) => await Do_void_async(task, p1, p2, p3, p4, p5), 1, 2, 3, 4, 5);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_async_p1_p2_p3_p4_p5_p6()
        {
            Task task = Start_void();
            task = TaskManager.StartNew(async (p1, p2, p3, p4, p5, p6) => await Do_void_async(task, p1, p2, p3, p4, p5, p6), 1, 2, 3, 4, 5, 6);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_async_p1_p2_p3_p4_p5_p6_p7()
        {
            Task task = Start_void();
            task = TaskManager.StartNew(async (p1, p2, p3, p4, p5, p6, p7) => await Do_void_async(task, p1, p2, p3, p4, p5, p6, p7), 1, 2, 3, 4, 5, 6, 7);
            Assert_void(task);
        }
        [Fact]
        public void TaskManager_void_StartNew_async_p1_p2_p3_p4_p5_p6_p7_p8()
        {
            Task task = Start_void();
            task = TaskManager.StartNew(async (p1, p2, p3, p4, p5, p6, p7, p8) => await Do_void_async(task, p1, p2, p3, p4, p5, p6, p7, p8), 1, 2, 3, 4, 5, 6, 7, 8);
            Assert_void(task);
        }
        //[Fact]
        //public void TaskManager_void_StartNew_async_p1_p2_p3_p4_p5_p6_p7_p8_p9()
        //{
        //    Task task = Start_void();
        //    task = TaskManager.StartNew(async (p1, p2, p3, p4, p5, p6, p7, p8, p9) => await Do_void_async(task, p1, p2, p3, p4, p5, p6, p7, p8, p9), 1, 2, 3, 4, 5, 6, 7, 8, 9);
        //    Assert_void(task);
        //}
        #endregion
        #region int_StartNew
        [Fact]
        public void TaskManager_int_StartNew()
        {
            var task = Start_int();
            task = TaskManager.StartNew(() => Do_int(task));
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_p1()
        {
            var task = Start_int();
            task = TaskManager.StartNew(p1 => Do_int(task, p1), 1);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_p1_p2()
        {
            var task = Start_int();
            task = TaskManager.StartNew((p1, p2) => Do_int(task, p1, p2), 1, 2);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_p1_p2_p3()
        {
            var task = Start_int();
            task = TaskManager.StartNew((p1, p2, p3) => Do_int(task, p1, p2, p3), 1, 2, 3);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_p1_p2_p3_p4()
        {
            var task = Start_int();
            task = TaskManager.StartNew((p1, p2, p3, p4) => Do_int(task, p1, p2, p3, p4), 1, 2, 3, 4);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_p1_p2_p3_p4_p5()
        {
            var task = Start_int();
            task = TaskManager.StartNew((p1, p2, p3, p4, p5) => Do_int(task, p1, p2, p3, p4, p5), 1, 2, 3, 4, 5);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_p1_p2_p3_p4_p5_p6()
        {
            var task = Start_int();
            task = TaskManager.StartNew((p1, p2, p3, p4, p5, p6) => Do_int(task, p1, p2, p3, p4, p5, p6), 1, 2, 3, 4, 5, 6);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_p1_p2_p3_p4_p5_p6_p7()
        {
            var task = Start_int();
            task = TaskManager.StartNew((p1, p2, p3, p4, p5, p6, p7) => Do_int(task, p1, p2, p3, p4, p5, p6, p7), 1, 2, 3, 4, 5, 6, 7);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_p1_p2_p3_p4_p5_p6_p7_p8()
        {
            var task = Start_int();
            task = TaskManager.StartNew((p1, p2, p3, p4, p5, p6, p7, p8) => Do_int(task, p1, p2, p3, p4, p5, p6, p7, p8), 1, 2, 3, 4, 5, 6, 7, 8);
            Assert_int(task);
        }
        //[Fact]
        //public void TaskManager_int_StartNew_p1_p2_p3_p4_p5_p6_p7_p8_p9()
        //{
        //    var task = Start_int();
        //    task = TaskManager.StartNew((p1, p2, p3, p4, p5, p6, p7, p8, p9) => Do_int(task, p1, p2, p3, p4, p5, p6, p7, p8, p9), 1, 2, 3, 4, 5, 6, 7, 8, 9);
        //    Assert_int(task);
        //}
        #endregion
        #region int_StartNew_async
        [Fact]
        public void TaskManager_int_StartNew_async()
        {
            var task = Start_int();
            task = TaskManager.StartNew(async () => await Do_int_async(task));
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_async_p1()
        {
            var task = Start_int();
            task = TaskManager.StartNew(async p1 => await Do_int_async(task, p1), 1);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_async_p1_p2()
        {
            var task = Start_int();
            task = TaskManager.StartNew(async (p1, p2) => await Do_int_async(task, p1, p2), 1, 2);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_async_p1_p2_p3()
        {
            var task = Start_int();
            task = TaskManager.StartNew(async (p1, p2, p3) => await Do_int_async(task, p1, p2, p3), 1, 2, 3);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_async_p1_p2_p3_p4()
        {
            var task = Start_int();
            task = TaskManager.StartNew(async (p1, p2, p3, p4) => await Do_int_async(task, p1, p2, p3, p4), 1, 2, 3, 4);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_async_p1_p2_p3_p4_p5()
        {
            var task = Start_int();
            task = TaskManager.StartNew(async (p1, p2, p3, p4, p5) => await Do_int_async(task, p1, p2, p3, p4, p5), 1, 2, 3, 4, 5);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_async_p1_p2_p3_p4_p5_p6()
        {
            var task = Start_int();
            task = TaskManager.StartNew(async (p1, p2, p3, p4, p5, p6) => await Do_int_async(task, p1, p2, p3, p4, p5, p6), 1, 2, 3, 4, 5, 6);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_async_p1_p2_p3_p4_p5_p6_p7()
        {
            var task = Start_int();
            task = TaskManager.StartNew(async (p1, p2, p3, p4, p5, p6, p7) => await Do_int_async(task, p1, p2, p3, p4, p5, p6, p7), 1, 2, 3, 4, 5, 6, 7);
            Assert_int(task);
        }
        [Fact]
        public void TaskManager_int_StartNew_async_p1_p2_p3_p4_p5_p6_p7_p8()
        {
            var task = Start_int();
            task = TaskManager.StartNew(async (p1, p2, p3, p4, p5, p6, p7, p8) => await Do_int_async(task, p1, p2, p3, p4, p5, p6, p7, p8), 1, 2, 3, 4, 5, 6, 7, 8);
            Assert_int(task);
        }
        //[Fact]
        //public void TaskManager_int_StartNew_async_p1_p2_p3_p4_p5_p6_p7_p8_p9()
        //{
        //    var task = Start_int();
        //    task = TaskManager.StartNew(async (p1, p2, p3, p4, p5, p6, p7, p8, p9) => await Do_int_async(task, p1, p2, p3, p4, p5, p6, p7, p8, p9), 1, 2, 3, 4, 5, 6, 7, 8, 9);
        //    Assert_int(task);
        //}
        #endregion
    }
}
