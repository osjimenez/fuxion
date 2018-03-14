using Fuxion.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
				task.Sleep(TimeSpan.FromMilliseconds(2500));
			});
			return task.ContinueWith(_ =>
			{
				Assert.True(dt.AddSeconds(2) < DateTime.Now);
			});
		}

		public static IEnumerable<object[]> GenerateTheoryParameters(int maxParNum)
		{
			var res = new List<object[]>();
			for (int i = 0; i < System.Math.Pow(2, 6); i++)
			{
				BitArray b = new BitArray(new int[] { i });
				var bits = b.Cast<bool>().Take(6).ToList();
				//if (!bits[3] || bits[4] || !bits[5]) continue; // Disable cases generation
				var strings = new[] {
					bits[0] ? "VOID  " : "RESULT", // 0
					bits[1] ? "SYNC  " : "ASYNC ", // 1
					bits[2] ? "CREATE" : "START ", // 2
					bits[3] ? "SEQUEN" : "PARALL", // 3
					bits[4] ? "LAST  " : "ALL   ", // 4
					bits[5] ? "CANCEL" : "NO_CAN", // 5
					};
				for (int j = 0; j < maxParNum + 1; j++)
					res.Add(strings.Cast<object>().ToList().Transform(ss => ss.Add(j)).ToArray());
			}
			return res;
		}
		[Theory(DisplayName = "TaskManager")]
		[MemberData(nameof(GenerateTheoryParameters), 9)]
		public async void TaskManager_Theory2(params object[] _)
			=> await TaskManager_Theory((string)_[0], (string)_[1], (string)_[2], (string)_[3], (string)_[4], (string)_[5], (int)_[6]);
		[Theory(DisplayName = "TaskManager")]
		[MemberData(nameof(GenerateTheoryParameters), 9)]
		public async Task TaskManager_Theory(string r, string c, string m, string p, string o, string n, int parNum)
		{
			#region Variables
			// Convert parameters to bool
			bool @void = r == "VOID  ";
			bool sync = c == "SYNC  ";
			bool create = m == "CREATE";
			bool sequentially = p == "SEQUEN";
			bool onlyLast = o == "LAST  ";
			bool cancel = n == "CANCEL";

			int runDelay = 5;
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
				Printer.WriteLine($"{nameof(parNum)}: {parNum}");
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
			var results = new(bool WasCancelled, string Result)[3];

			#region Methods
			void AddToSeq(string val)
			{
				lock (seqLocker)
				{
					seq += val;
				}
			}
			object[] GenerateParameters(Delegate del, ConcurrencyProfile pro)
			{
				var res = new List<object>();
				res.Add(del);
				for (var i = 0; i < parNum; i++)
					res.Add(i);
				if (!create) res.Add(null);
				res.Add(null);
				res.Add(pro);
				return res.ToArray();
			}
			MethodInfo GetMethod()
			{
				var mets = typeof(TaskManager).GetMethods().ToList();
				mets = mets.Where(me => me.Name == (create ? nameof(TaskManager.Create) : nameof(TaskManager.StartNew))).ToList();
				mets = mets.Where(me => me.GetParameters().Count() == (3 + parNum + (create ? 0 : 1))).ToList();
				mets = mets.Where(me => me.GetGenericArguments().Count() == (@void ? parNum : parNum + 1)).ToList();
				mets = mets.Where(me => me.GetParameters().First().ParameterType.Name.StartsWith(@void && sync
								? "Action"
								: "Func")).ToList();

				mets = mets.Where(me => me.GetParameters().First().ParameterType.GetGenericArguments().Count() == (!sync
								? parNum + 1
								: @void
									? parNum
									: parNum + 1)).ToList();
				mets = mets.Where(me =>
					@void && parNum == 0
					||
					sync && !typeof(Task).IsAssignableFrom(me.GetParameters().First().ParameterType.GetGenericArguments().Last())
					||
					!sync && typeof(Task).IsAssignableFrom(me.GetParameters().First().ParameterType.GetGenericArguments().Last())).ToList();
				var met = mets.Single();
				if (met.IsGenericMethod)
				{
					var args = met.GetGenericArguments().Select(a => typeof(int)).ToArray();
					if (!@void)
						args[args.Length - 1] = typeof(string);
					met = met.MakeGenericMethod(args);
				}
				Printer.WriteLine("Method: " + met.GetSignature(true, true, true, false, true, false));
				return met;
			}
			Delegate GetDelegate(int order)
			{
				void Void_Sync()
				{
					Printer.WriteLine($"Do {order}");
					try
					{
						AddToSeq($"S{order}-");
						Printer.WriteLine("Start " + order);
						Task.Delay(runDelay, TaskManager.Current.GetCancellationToken().Value).Wait();
						AddToSeq($"E{order}-");
						Printer.WriteLine("End " + order);
					}
					catch
					{
						AddToSeq($"E{order}X-");
						Printer.WriteLine($"End {order} - in catch");
						throw;
					}
				}
				async Task Void_Async()
				{
					Printer.WriteLine($"Do {order}");
					try
					{
						AddToSeq($"S{order}-");
						Printer.WriteLine("Start " + order);
						await Task.Delay(runDelay, TaskManager.Current.GetCancellationToken().Value);
						AddToSeq($"E{order}-");
						Printer.WriteLine("End " + order);
					}
					catch
					{
						AddToSeq($"E{order}X-");
						Printer.WriteLine($"End {order} - in catch");
						throw;
					}
				}
				string Result_Sync()
				{
					Printer.WriteLine($"Do {order}");
					try
					{
						AddToSeq($"S{order}-");
						Printer.WriteLine("Start " + order);
						Task.Delay(runDelay, TaskManager.Current.GetCancellationToken().Value).Wait();
						AddToSeq($"E{order}-");
						Printer.WriteLine("End " + order);
						return $"{doneResult}_{parNum}";
					}
					catch
					{
						AddToSeq($"E{order}X-");
						Printer.WriteLine($"End {order} - in catch");
						throw;
					}
				}
				async Task<string> Result_Async()
				{
					Printer.WriteLine($"Do {order}");
					try
					{
						AddToSeq($"S{order}-");
						Printer.WriteLine("Start " + order);
						await Task.Delay(runDelay, TaskManager.Current.GetCancellationToken().Value);
						AddToSeq($"E{order}-");
						Printer.WriteLine("End " + order);
						return $"{doneResult}_{parNum}";
					}
					catch
					{
						AddToSeq($"E{order}X-");
						Printer.WriteLine($"End {order} - in catch");
						throw;
					}
				}
				if (@void)
				{
					if (sync)
					{
						switch (parNum)
						{
							case 0:
								return new Action(() => Void_Sync());
							case 1:
								return new Action<int>(s => Void_Sync());
							case 2:
								return new Action<int, int>((s1, s2) => Void_Sync());
							case 3:
								return new Action<int, int, int>((s1, s2, s3) => Void_Sync());
							case 4:
								return new Action<int, int, int, int>((s1, s2, s3, s4) => Void_Sync());
							case 5:
								return new Action<int, int, int, int, int>((s1, s2, s3, s4, s5) => Void_Sync());
							case 6:
								return new Action<int, int, int, int, int, int>((s1, s2, s3, s4, s5, s6) => Void_Sync());
							case 7:
								return new Action<int, int, int, int, int, int, int>((s1, s2, s3, s4, s5, s6, s7) => Void_Sync());
							case 8:
								return new Action<int, int, int, int, int, int, int, int>((s1, s2, s3, s4, s5, s6, s7, s8) => Void_Sync());
							case 9:
								return new Action<int, int, int, int, int, int, int, int, int>((s1, s2, s3, s4, s5, s6, s7, s8, s9) => Void_Sync());
							default:
								return null;
						}
					}
					else
					{
						switch (parNum)
						{
							case 0:
								return new Func<Task>(() => Void_Async());
							case 1:
								return new Func<int, Task>(s => Void_Async());
							case 2:
								return new Func<int, int, Task>((s1, s2) => Void_Async());
							case 3:
								return new Func<int, int, int, Task>((s1, s2, s3) => Void_Async());
							case 4:
								return new Func<int, int, int, int, Task>((s1, s2, s3, s4) => Void_Async());
							case 5:
								return new Func<int, int, int, int, int, Task>((s1, s2, s3, s4, s5) => Void_Async());
							case 6:
								return new Func<int, int, int, int, int, int, Task>((s1, s2, s3, s4, s5, s6) => Void_Async());
							case 7:
								return new Func<int, int, int, int, int, int, int, Task>((s1, s2, s3, s4, s5, s6, s7) => Void_Async());
							case 8:
								return new Func<int, int, int, int, int, int, int, int, Task>((s1, s2, s3, s4, s5, s6, s7, s8) => Void_Async());
							case 9:
								return new Func<int, int, int, int, int, int, int, int, int, Task>((s1, s2, s3, s4, s5, s6, s7, s8, s9) => Void_Async());
							default:
								return null;
						}
					}
				}
				else
				{
					if (sync)
					{
						switch (parNum)
						{
							case 0:
								return new Func<string>(() => Result_Sync());
							case 1:
								return new Func<int, string>(s => Result_Sync());
							case 2:
								return new Func<int, int, string>((s1, s2) => Result_Sync());
							case 3:
								return new Func<int, int, int, string>((s1, s2, s3) => Result_Sync());
							case 4:
								return new Func<int, int, int, int, string>((s1, s2, s3, s4) => Result_Sync());
							case 5:
								return new Func<int, int, int, int, int, string>((s1, s2, s3, s4, s5) => Result_Sync());
							case 6:
								return new Func<int, int, int, int, int, int, string>((s1, s2, s3, s4, s5, s6) => Result_Sync());
							case 7:
								return new Func<int, int, int, int, int, int, int, string>((s1, s2, s3, s4, s5, s6, s7) => Result_Sync());
							case 8:
								return new Func<int, int, int, int, int, int, int, int, string>((s1, s2, s3, s4, s5, s6, s7, s8) => Result_Sync());
							case 9:
								return new Func<int, int, int, int, int, int, int, int, int, string>((s1, s2, s3, s4, s5, s6, s7, s8, s9) => Result_Sync());
							default:
								return null;
						}
					}
					else
					{
						switch (parNum)
						{
							case 0:
								return new Func<Task<string>>(() => Result_Async());
							case 1:
								return new Func<int, Task<string>>(s => Result_Async());
							case 2:
								return new Func<int, int, Task<string>>((s1, s2) => Result_Async());
							case 3:
								return new Func<int, int, int, Task<string>>((s1, s2, s3) => Result_Async());
							case 4:
								return new Func<int, int, int, int, Task<string>>((s1, s2, s3, s4) => Result_Async());
							case 5:
								return new Func<int, int, int, int, int, Task<string>>((s1, s2, s3, s4, s5) => Result_Async());
							case 6:
								return new Func<int, int, int, int, int, int, Task<string>>((s1, s2, s3, s4, s5, s6) => Result_Async());
							case 7:
								return new Func<int, int, int, int, int, int, int, Task<string>>((s1, s2, s3, s4, s5, s6, s7) => Result_Async());
							case 8:
								return new Func<int, int, int, int, int, int, int, int, Task<string>>((s1, s2, s3, s4, s5, s6, s7, s8) => Result_Async());
							case 9:
								return new Func<int, int, int, int, int, int, int, int, int, Task<string>>((s1, s2, s3, s4, s5, s6, s7, s8, s9) => Result_Async());
							default:
								return null;
						}
					}
				}
			}
			Task Action_Sync(int order)
			{
				try
				{
					if (create)
					{
						var task = (Task)GetMethod().Invoke(null, GenerateParameters(GetDelegate(order), concurrencyProfile));
						task.Start();
						return task;
					}
					else return (Task)GetMethod().Invoke(null, GenerateParameters(GetDelegate(order), concurrencyProfile));
				}
				catch (Exception ex)
				{
					Printer.WriteLine($"Error '{ex.GetType().Name}': {ex.Message}");
					throw;
				}
				finally { are.Set(); }
			}
			Task Action_Async(int order)
			{
				try
				{
					if (create)
					{
						var task = (Task)GetMethod().Invoke(null, GenerateParameters(GetDelegate(order), concurrencyProfile));
						task.Start();
						return task;
					}
					else return (Task)GetMethod().Invoke(null, GenerateParameters(GetDelegate(order), concurrencyProfile));
				}
				catch (Exception ex)
				{
					Printer.WriteLine($"Error '{ex.GetType().Name}': {ex.Message}");
					throw;
				}
				finally { are.Set(); }
			}
			Task<string> Func_Sync(int order)
			{
				try
				{
					if (create)
					{
						var task = (Task<string>)GetMethod().Invoke(null, GenerateParameters(GetDelegate(order), concurrencyProfile));
						task.Start();
						return task;
					}
					else return (Task<string>)GetMethod().Invoke(null, GenerateParameters(GetDelegate(order), concurrencyProfile));
				}
				catch (Exception ex)
				{
					Printer.WriteLine($"Error '{ex.GetType().Name}': {ex.Message}");
					throw;
				}
				finally { are.Set(); }
			}
			Task<string> Func_Async(int order)
			{
				try
				{
					if (create)
					{
						var task = (Task<string>)GetMethod().Invoke(null, GenerateParameters(GetDelegate(order), concurrencyProfile));
						task.Start();
						return task;
					}
					else return (Task<string>)GetMethod().Invoke(null, GenerateParameters(GetDelegate(order), concurrencyProfile));
				}
				catch (Exception ex)
				{
					Printer.WriteLine($"Error '{ex.GetType().Name}': {ex.Message}");
					throw;
				}
				finally { are.Set(); }
			}
			#endregion
			#endregion

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

			#region Assert sequence
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

			#region Assert results
			if (!@void)
			{
				if (!results[0].WasCancelled) Assert.Equal($"{doneResult}_{parNum}", results[0].Result);
				if (!results[1].WasCancelled) Assert.Equal($"{doneResult}_{parNum}", results[1].Result);
				if (!results[2].WasCancelled) Assert.Equal($"{doneResult}_{parNum}", results[2].Result);
			}
			#endregion
		}
	}
}