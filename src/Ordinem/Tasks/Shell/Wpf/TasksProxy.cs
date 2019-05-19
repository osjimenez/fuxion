using Fuxion.Domain;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Ordinem.Tasks.Domain;
using Fuxion.Application.Commands;

namespace Ordinem.Tasks.Shell.Wpf
{
	public class TasksProxy
	{
		private static readonly HttpClient client = new HttpClient();
		const string host = "localhost";
		const int port = 5100;
		public AsyncRetryPolicy GetRetryPolicy([CallerMemberName] string? callerMemberName = null)
			=> RetryPolicy.Handle<HttpRequestException>()
			//.Or<BrokerUnreachableException>()
			.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(System.Math.Pow(2, retryAttempt)), (ex, time) =>
			{
				Debug.WriteLine($"ERROR '{ex.GetType().Name}' en '{callerMemberName}': {ex.Message}");
			}
		);
		public Task AddToDoTask(Guid id, string name)
			=> SendToDoTaskCommand(new CreateToDoTaskCommand(id, name));
		public Task RenameToDoTask(Guid id, string name)
			=> SendToDoTaskCommand(new RenameToDoTaskCommand(id, name));
		public Task DeleteToDoTask(Guid id)
			=> SendToDoTaskCommand(new DeleteToDoTaskCommand
			{
				Id = id
			});
		private Task SendToDoTaskCommand(Command command) => GetRetryPolicy().ExecuteAsync(async () =>
		{
			var content = new StringContent(command.ToCommandPod().ToJson(), Encoding.UTF8, "application/json");
			var response = await client.PostAsync($"https://{host}:{port}/api/Tasks/Command", content);
			var responseString = await response.Content.ReadAsStringAsync();
		});
		public Task<ToDoTaskDto> GetToDoTask(Guid toDoTaskId) => GetRetryPolicy().ExecuteAsync(async () =>
		{
			var json = await client.GetStringAsync($"https://{host}:{port}/api/Tasks/ToDoTask/" + toDoTaskId);
			var res = json.FromJson<ToDoTaskDto>();
			return res;
		});
		public Task<IEnumerable<ToDoTaskDto>> GetToDoTasks() => GetRetryPolicy().ExecuteAsync(async () =>
		{
			var json = await client.GetStringAsync($"https://{host}:{port}/api/Tasks/ToDoTask");
			var res = json.FromJson<IEnumerable<ToDoTaskDto>>();
			return res;
		});
	}
}
