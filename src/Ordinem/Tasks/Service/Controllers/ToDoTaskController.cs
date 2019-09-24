using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Ordinem.Tasks.Projection;
using Ordinem.Tasks.Service.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ordinem.Tasks.Service.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ToDoTaskController : ControllerBase
	{
		public ToDoTaskController(IMapper mapper, TasksDbContext context)
		{
			this.mapper = mapper;
			this.context = context;
		}

		private readonly IMapper mapper;
		private readonly TasksDbContext context;
		[HttpGet]
		// TODO - Automapper ProjectTo doesn't working with Core 3.0 rtm - Remove ToList().AsQueryable()
		public ActionResult<IEnumerable<ToDoTaskDto>> Get() => Ok(context.ToDoTasks.ToList().AsQueryable().ProjectTo<ToDoTaskDto>(mapper.ConfigurationProvider));
		[HttpGet("{id}")]
		public ActionResult<ToDoTaskDto> Get(Guid id) => Ok(mapper.Map<ToDoTaskDto>(context.ToDoTasks.FirstOrDefault(d => d.Id == id)));
	}
}
