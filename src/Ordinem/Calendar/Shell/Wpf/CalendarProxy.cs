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
using Ordinem.Calendar.Domain;
using Fuxion.Application.Commands;

namespace Ordinem.Calendar.Shell.Wpf
{
	public class CalendarProxy
	{
		private static readonly HttpClient client = new HttpClient();
		const string host = "localhost";
		const int port = 5100;
		public AsyncRetryPolicy GetRetryPolicy([CallerMemberName] string callerMemberName = null)
			=> RetryPolicy.Handle<HttpRequestException>()
			//.Or<BrokerUnreachableException>()
			.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(System.Math.Pow(2, retryAttempt)), (ex, time) =>
			{
				Debug.WriteLine($"ERROR '{ex.GetType().Name}' en '{callerMemberName}': {ex.Message}");
			}
		);
		public Task AddToDoTask(Guid id, string name)
			=> SendCalendarCommand(new CreateAppointmentCommand
			{
				Id = id,
				AppointmentName = name
			});
		public Task RenameToDoTask(Guid id, string name)
			=> SendCalendarCommand(new RenameAppointmentCommand
			{
				Id = id,
				NewName = name
			});
		public Task DeleteToDoTask(Guid id)
			=> SendCalendarCommand(new DeleteAppointmentCommand
			{
				Id = id
			});
		private Task SendCalendarCommand(Command command) => GetRetryPolicy().ExecuteAsync(async () =>
		{
			var content = new StringContent(command.ToCommandPod().ToJson(), Encoding.UTF8, "application/json");
			var response = await client.PostAsync($"https://{host}:{port}/api/Calendar/Command", content);
			var responseString = await response.Content.ReadAsStringAsync();
		});
		public Task<AppointmentDto> GetAppointment(Guid toDoTaskId) => GetRetryPolicy().ExecuteAsync(async () =>
		{
			var json = await client.GetStringAsync($"https://{host}:{port}/api/Calendar/Appointment/" + toDoTaskId);
			var res = json.FromJson<AppointmentDto>();
			return res;
		});
		public Task<IEnumerable<AppointmentDto>> GetAppointments() => GetRetryPolicy().ExecuteAsync(async () =>
		{
			var json = await client.GetStringAsync($"https://{host}:{port}/api/Calendar/Appointment");
			var res = json.FromJson<IEnumerable<AppointmentDto>>();
			return res;
		});
	}
}
