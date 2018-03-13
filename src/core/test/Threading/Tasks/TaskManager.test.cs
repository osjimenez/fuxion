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
					//var message = $"{(Task.CurrentId != null ? $"({Task.CurrentId.Value}) " : "")}{m}";
					var message = $"{m}{(Task.CurrentId != null ? $" - {Task.CurrentId.Value}" : "")}";
					output.WriteLine(message);
					Debug.WriteLine(message);
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
			task = TaskManager.StartNew(() =>
			{
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
			task = TaskManager.StartNew(() =>
			{
				//task.Sleep(TimeSpan.FromMilliseconds(2500), TimeSpan.FromMilliseconds(500));
				task.Sleep(TimeSpan.FromMilliseconds(2500));
			});
			return task.ContinueWith(_ =>
			{
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
				//if (bits[0] && bits[1] && !bits[2] && bits[3])// && !bits[4])
					res.Add(bits.Cast<object>().ToArray());
			}
			return res;
		}
		[Theory(DisplayName = "TaskManager - StartNew")]
		[MemberData(nameof(GenerateStartNewValues), 6)]
		public async void TaskManager_StartNew(bool @void, bool sync, bool create, bool sequentially, bool onlyLast, bool cancel)
		{
			// test constants
			int runDelay = 10;
			string cancelledResult = "Canceled";
			string doneResult = "Done";

			using (Printer.Indent2("Test constants"))
			{
				Printer.WriteLine($"{nameof(runDelay)}: {runDelay}");
				Printer.WriteLine($"{nameof(cancelledResult)}: {cancelledResult}");
				Printer.WriteLine($"{nameof(doneResult)}: {doneResult}");
			}
			using (Printer.Indent2("Test parameters"))
			{
				Printer.WriteLine($"{nameof(@void)}: {@void}");
				Printer.WriteLine($"{nameof(sync)}: {sync}");
				Printer.WriteLine($"{nameof(create)}: {create}");
				Printer.WriteLine($"{nameof(sequentially)}: {sequentially}");
				Printer.WriteLine($"{nameof(onlyLast)}: {onlyLast}");
				Printer.WriteLine($"{nameof(cancel)}: {cancel}");
			}
			Printer.WriteLine("==============");

			var concurrencyProfile = new ConcurrencyProfile
			{
				Sequentially = sequentially,
				ExecuteOnlyLast = onlyLast,
				CancelPrevious = cancel,
			};
			AutoResetEvent are = new AutoResetEvent(true);
			object seqLocker = new object();
			string seq = null;
			#region Run modes
			void AddToSeq(string val)
			{
				lock (seqLocker)
				{
					seq += val;
				}
			}
			Task Action_Sync(int num)
			{
				void Do()
				{
					Printer.WriteLine($"Do {num}");
					try
					{
						AddToSeq($"S{num}-");
						Printer.WriteLine("Start " + num);
						Task.Delay(runDelay, TaskManager.Current.GetCancellationToken().Value).Wait();
						AddToSeq($"E{num}-");
						Printer.WriteLine("End " + num);
					}
					catch
					{
						AddToSeq($"E{num}X-");
						Printer.WriteLine($"End {num} - in catch");
						throw;
					}
				}
				if (create)
				{
					var task = TaskManager.Create(() => Do(), concurrencyProfile: concurrencyProfile);
					are.Set();
					task.Start();
					return task;
				}
				else
				{
					var task = TaskManager.StartNew(() => Do(), concurrencyProfile: concurrencyProfile);
					are.Set();
					return task;
				}
			}
			Task Action_Async(int num)
			{
				async Task Do()
				{
					Printer.WriteLine($"Do {num}");
					try
					{
						AddToSeq($"S{num}-");
						Printer.WriteLine("Start " + num);
						await Task.Delay(runDelay, TaskManager.Current.GetCancellationToken().Value);
						AddToSeq($"E{num}-");
						Printer.WriteLine("End " + num);
					}
					catch
					{
						AddToSeq($"E{num}X-");
						Printer.WriteLine($"End {num} - in catch");
						throw;
					}
				}
				if (create)
				{
					var task = TaskManager.Create(async () => await Do(), concurrencyProfile: concurrencyProfile);
					are.Set();
					task.Start();
					return task;
				}
				else
				{
					var task = TaskManager.StartNew(async () => await Do(), concurrencyProfile: concurrencyProfile);
					are.Set();
					return task;
				}
			}
			Task<string> Func_Sync(int num)
			{
				string Do()
				{
					Printer.WriteLine($"Do {num}");
					try
					{
						AddToSeq($"S{num}-");
						Printer.WriteLine("Start " + num);
						Task.Delay(runDelay, TaskManager.Current.GetCancellationToken().Value).Wait();
						AddToSeq($"E{num}-");
						Printer.WriteLine("End " + num);
						return doneResult;
					}
					catch
					{
						AddToSeq($"E{num}X-");
						Printer.WriteLine($"End {num} - in catch");
						throw;
					}
				}
				if (create)
				{
					var task = TaskManager.Create(() => Do(), concurrencyProfile: concurrencyProfile);
					are.Set();
					task.Start();
					return task;
				}
				else
				{
					var task = TaskManager.StartNew(() => Do(), concurrencyProfile: concurrencyProfile);
					are.Set();
					return task;
				}
			}
			Task<string> Func_Async(int num)
			{
				async Task<string> Do()
				{
					Printer.WriteLine($"Do {num}");
					try
					{
						AddToSeq($"S{num}-");
						Printer.WriteLine("Start " + num);
						await Task.Delay(runDelay, TaskManager.Current.GetCancellationToken().Value);
						AddToSeq($"E{num}-");
						Printer.WriteLine("End " + num);
						return doneResult;
					}
					catch
					{
						AddToSeq($"E{num}X-");
						Printer.WriteLine($"End {num} - in catch");
						throw;
					}
				}
				if (create)
				{
					var task = TaskManager.Create(async () => await Do(), concurrencyProfile: concurrencyProfile);
					are.Set();
					task.Start();
					return task;
				}
				else
				{
					var task = TaskManager.StartNew(async () => await Do(), concurrencyProfile: concurrencyProfile);
					are.Set();
					return task;
				}
			}
			#endregion

			var results = new(bool WasCancelled, string Result)[3];

			#region Run
			using (Printer.Indent2("Run"))
			{
				Task<(bool WasCancelled, string Result)>[] res = new Task<(bool WasCancelled, string Result)>[3];
				int num = 1;
				object numLocker = new object();
				int GetNum()
				{
					lock (numLocker)
					{
						return num++;
					}
				}
				for (int i = 0; i < 3; i++)
				{
					res[i] = Task.Run(async () =>
					{
						are.WaitOne();
						var currentNum = GetNum();
						Printer.WriteLine($"Test Run {currentNum}");
						try
						{
							if (@void)
							{
								if (sync)
									await Action_Sync(currentNum);
								else
									await Action_Async(currentNum);
								return (false, doneResult);
							}
							else
							{
								return sync
									? (false, await Func_Sync(currentNum))
									: (false, await Func_Async(currentNum));
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
				}
				await Task.WhenAll(res);
				results[0] = res[0].Result;
				results[1] = res[1].Result;
				results[2] = res[2].Result;
				seq = seq.Trim('-');
				for (var i = 0; i < results.Length; i++)
					Printer.WriteLine($"Result {i + 1}: WasCanceled<{results[i].WasCancelled}>,Result<{results[i].Result}>");
				Printer.WriteLine($"Sequence: " + seq);
			}
			#endregion

			#region Assert

			if (sequentially)
			{
				if (cancel)
				{
					if (onlyLast)
					{
						// Only last call executed sequentially canceling previous
						Assert.True(seq == "S1-E1-S2-E2-S3-E3" || seq == "S1-E1-S2-E2X-S3-E3" || seq == "S1-E1X-S2-E2X-S3-E3" || seq == "S1-E1X-S2-E2-S3-E3" 
							|| seq == "S1-E1-S3-E3" || seq == "S1-E1X-S3-E3"
							|| seq == "S2-E2-S3-E3" || seq == "S2-E2X-S3-E3"
							|| seq == "S3-E3");
					}
					else
					{
						// All executed sequentially canceling previous
						Assert.True(seq == "S1-E1-S2-E2-S3-E3" || seq == "S1-E1-S2-E2X-S3-E3" || seq == "S1-E1X-S2-E2-S3-E3" || seq == "S1-E1X-S2-E2X-S3-E3");
					}
				}
				else
				{
					if (onlyLast)
					{
						// Only last call executed sequentially without cancelations
						Assert.True(seq == "S1-E1-S2-E2-S3-E3" || seq == "S1-E1-S3-E3" || seq == "S2-E2-S3-E3" || seq == "S3-E3");
					}
					else
					{
						// All executed sequentially without cancelations
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
						// Only last call executed in any order canceling previous
						Assert.True(seq.Contains("S3") && seq.Contains("E3"));
					}
					else
					{
						// All executed in any order canceling previous
						Assert.True(
							(seq.Contains("S1") && seq.Contains("E1X") || seq.Contains("S1") && seq.Contains("E1"))
							&&
							(seq.Contains("S2") && seq.Contains("E2X") || seq.Contains("S2") && seq.Contains("E2"))
							&& seq.Contains("S3") && seq.Contains("E3"));
					}
				}
				else
				{
					if (onlyLast)
					{
						// Only last call executed in any order without cancelations
						Assert.True(seq.Contains("S3") && seq.Contains("E3"));
					}
					else
					{
						// All executed in any order without cancelations
						Assert.Contains("S1", seq);
						Assert.Contains("E1", seq);
						Assert.Contains("S2", seq);
						Assert.Contains("E2", seq);
						Assert.Contains("S2", seq);
						Assert.Contains("E2", seq);
					}
				}
			}
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
