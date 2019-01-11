using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace System.ComponentModel.DataAnnotations
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public class DisplayForClassAttribute : Attribute
	{
		protected readonly DisplayAttribute Attribute = new DisplayAttribute();
		public bool IsFemale { get; set; }
		public string ShortName
		{
			get => Attribute.ShortName;
			set => Attribute.ShortName = value;
		}
		public string Name
		{
			get => Attribute.Name;
			set => Attribute.Name = value;
		}
		public string Description
		{
			get => Attribute.Description;
			set => Attribute.Description = value;
		}
		public string Prompt
		{
			get => Attribute.Prompt;
			set => Attribute.Prompt = value;
		}
		public string GroupName
		{
			get => Attribute.GroupName;
			set => Attribute.GroupName = value;
		}
		public Type ResourceType
		{
			get => Attribute.ResourceType;
			set => Attribute.ResourceType = value;
		}
		public bool AutoGenerateField
		{
			get => Attribute.AutoGenerateField;
			set => Attribute.AutoGenerateField = value;
		}
		public bool AutoGenerateFilter
		{
			get => Attribute.AutoGenerateFilter;
			set => Attribute.AutoGenerateFilter = value;
		}
		public int Order
		{
			get => Attribute.Order;
			set => Attribute.Order = value;
		}
		public string GetShortName() => Attribute.GetShortName();
		public string GetName() => Attribute.GetName();
		public string GetDescription() => Attribute.GetDescription();
		public string GetPrompt() => Attribute.GetPrompt();
		public string GetGroupName() => Attribute.GetGroupName();
		public bool? GetAutoGenerateField() => Attribute.GetAutoGenerateField();
		public bool? GetAutoGenerateFilter() => Attribute.GetAutoGenerateFilter();
		public int? GetOrder() => Attribute.GetOrder();
	}
}
