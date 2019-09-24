using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Ordinem.Calendar.Projection;
using Ordinem.Calendar.Service.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ordinem.Tasks.Service.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AppointmentController : ControllerBase
	{
		public AppointmentController(IMapper mapper, CalendarDbContext context)
		{
			this.mapper = mapper;
			this.context = context;
		}

		private readonly IMapper mapper;
		private readonly CalendarDbContext context;
		[HttpGet]
		// TODO - Automapper ProjectTo doesn't working with Core 3.0 rtm - Remove ToList().AsQueryable()
		public ActionResult<IEnumerable<AppointmentDto>> Get() => Ok(context.Appointments.ToList().AsQueryable().ProjectTo<AppointmentDto>(mapper.ConfigurationProvider));
		[HttpGet("{id}")]
		public ActionResult<AppointmentDto> Get(Guid id) => Ok(mapper.Map<AppointmentDto>(context.Appointments.FirstOrDefault(d => d.Id == id)));
	}
}
