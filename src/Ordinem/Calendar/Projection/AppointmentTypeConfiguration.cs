using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordinem.Calendar.Projection
{
	public class AppointmentTypeConfiguration : IEntityTypeConfiguration<AppointmentDpo>
	{
		public void Configure(EntityTypeBuilder<AppointmentDpo> toDoTaskConfiguration)
		{
			toDoTaskConfiguration.ToTable("Appointments", CalendarDbContext.DEFAULT_SCHEMA);
			toDoTaskConfiguration.HasKey(d => d.Id);
		}
	}
}
