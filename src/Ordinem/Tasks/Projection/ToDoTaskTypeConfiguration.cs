using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordinem.Tasks.Projection
{
	public class ToDoTaskTypeConfiguration : IEntityTypeConfiguration<ToDoTaskDpo>
	{
		public void Configure(EntityTypeBuilder<ToDoTaskDpo> toDoTaskConfiguration)
		{
			toDoTaskConfiguration.ToTable("ToDoTasks", TasksDbContext.DEFAULT_SCHEMA);
			toDoTaskConfiguration.HasKey(d => d.Id);
		}
	}
}
