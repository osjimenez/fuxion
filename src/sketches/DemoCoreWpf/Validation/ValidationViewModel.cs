using Fuxion.ComponentModel;
using Fuxion.ComponentModel.DataAnnotations;
using Fuxion.Threading.Tasks;
using Fuxion.Windows.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DemoCoreWpf.Validation
{
	[ConditionalValidation(typeof(ValidationViewModel), nameof(MustBeValidate))]
	public class ValidationViewModel : Notifier<ValidationViewModel>, IDataErrorInfo //,INotifyDataErrorInfo
	{
		public ValidationViewModel()
		{
			Validator.RegisterNotifier(this);
			Validator.ValidationChanged += (s, e) => SaveCommand.RaiseCanExecuteChanged();
			PropertyChanged += (s, e) =>
			{
				e.Case(() => Name, a =>
				{
					foreach (var dis in nameDisposables)
						dis.Dispose();
					nameDisposables = new List<IDisposable>();
				});
			};
		}
		List<IDisposable> nameDisposables = new List<IDisposable>();
		public GenericCommand SaveCommand => GetValue(() => new GenericCommand(() =>
		 {
			 Validator.ClearCustoms();
			 if (Name.Contains("O"))
			 {
				 nameDisposables.Add(Validator.AddCustom($"El nombre '{Name}' esta repetido"));
			 }
			 if (Validator.HasMessages) SaveCommand.RaiseCanExecuteChanged();
			 else MessageBox.Show("Saved !!!");
		 }, () => !Validator.HasMessages));
		public GenericCommand AddCommand => new GenericCommand(() =>
		{
			ValidationRecursiveCollection.Add(new ValidationRecursiveViewModel(Validator)
			{
				Id = 1,
				Name = "Osca"
			});
		});
		public GenericCommand<ValidationRecursiveViewModel> RemoveCommand => new GenericCommand<ValidationRecursiveViewModel>(ent =>
		{
			ValidationRecursiveCollection.Remove(ent);
		});

		public bool IsValid { get => GetValue(() => true); set => SetValue(value); }
		public bool MustBeValidate() => IsValid;

		[Display(Name = nameof(Id), ResourceType = typeof(TextLocalized))]
		[Required(ErrorMessageResourceType = typeof(TextLocalized), ErrorMessageResourceName = nameof(TextLocalized.Required))]
		[Range(1, 100, ErrorMessageResourceType = typeof(TextLocalized), ErrorMessageResourceName = nameof(TextLocalized.Range))]
		public int? Id { get => GetValue<int?>(() => 1); set => SetValue(value); }

		public bool NameMustBeLower { get => GetValue<bool>(); set => SetValue(value); }
		[Display(Name = nameof(Name), ResourceType = typeof(TextLocalized))]
		[Required(ErrorMessageResourceType = typeof(TextLocalized), ErrorMessageResourceName = nameof(TextLocalized.Required))]
		[StringLength(10, ErrorMessageResourceType = typeof(TextLocalized), ErrorMessageResourceName = nameof(TextLocalized.StringLength))]
		[CustomValidation(typeof(ValidationViewModel), nameof(ValidationViewModel.ValidateName))]
		[CustomValidationWithContext(typeof(ValidationViewModel), nameof(ValidationViewModel.ValidateNameCrossField))]
		public string Name { get => GetValue(() => "Osca"); set => SetValue(value); }
		public static ValidationResult? ValidateName(string value)
		{
			if (value.ToLower().Contains("oscar"))
				return new ValidationResult($"El '{TextLocalized.Name}' no puede ser Oscar");
			return ValidationResult.Success;
		}
		public static ValidationResult? ValidateNameCrossField(string value, ValidationContext context)
		{
			if (((ValidationViewModel)context.ObjectInstance).NameMustBeLower && value.ToLower() != value)
				return new ValidationResult($"El '{TextLocalized.Name}' debe estar en minúsculas");
			return ValidationResult.Success;
		}

		[Required(ErrorMessage = "ValidationRecursive debe setearse")]
		[Display(Name = "Recursivo")]
		[RecursiveValidation]
		public ValidationRecursiveViewModel ValidationRecursive
		{
			get => GetValue(() => new ValidationRecursiveViewModel(Validator));
			set => SetValue(value);
		}
		[RecursiveValidation]
		[Display(Name = "Colección recursiva")]
		[EnsureMinimumElements(1, ErrorMessageResourceType = typeof(TextLocalized), ErrorMessageResourceName = nameof(TextLocalized.AtLeastOneElement))]
		[EnsureNoDuplicates(typeof(ValidationViewModel), nameof(CompareValidationRecursiveViewModelByName), ErrorMessageResourceType = typeof(TextLocalized), ErrorMessageResourceName = nameof(TextLocalized.ElementDuplicated))]
		public ObservableCollection<ValidationRecursiveViewModel> ValidationRecursiveCollection
		{
			get => GetValue(() => new ObservableCollection<ValidationRecursiveViewModel>(new[]{
				new ValidationRecursiveViewModel(Validator)
				{
					Id = 1,
					Name = "Osca"
				}
			}));
			set => SetValue(value);
		}
		public static bool CompareValidationRecursiveViewModelByName(ValidationRecursiveViewModel a, ValidationRecursiveViewModel b) => a.Name == b.Name;

		[NullableEmailAddress(ErrorMessageResourceType = typeof(TextLocalized), ErrorMessageResourceName = nameof(TextLocalized.InvalidEmail))]
		public string Email { get => GetValue<string>(); set => SetValue(value); }

		[Hostname(ErrorMessageResourceType = typeof(TextLocalized), ErrorMessageResourceName = nameof(TextLocalized.InvalidHostname))]
		public string Hostname { get => GetValue<string>(); set => SetValue(value); }

		[IpAddress(ErrorMessageResourceType = typeof(TextLocalized), ErrorMessageResourceName = nameof(TextLocalized.InvalidIPAddress))]
		public string IPAddress { get => GetValue<string>(); set => SetValue(value); }

		public GenericCommand BaseAsNullCommand => GetValue(() => new GenericCommand(() => Base = null));
		public GenericCommand BaseAsDerived1Command => GetValue(() => new GenericCommand(() => Base = new Derived1ValidationViewModel(Validator)));
		public GenericCommand BaseAsDerived2Command => GetValue(() => new GenericCommand(() => Base = new Derived2ValidationViewModel(Validator)));
		[RecursiveValidation]
		public BaseValidationViewModel? Base { get => GetValue<BaseValidationViewModel>(); set => SetValue(value); }

		public NotifierValidator Validator
		{
			get => GetValue(() =>
			{
				var val = new NotifierValidator();
				//val.RegisterNotifier(this);
				return val;
			});
		}
		#region IDataErrorInfo
		public string? Error => null;
		public string? this[string columnName]
		{
			get
			{
				var res = NotifierValidator.Validate(this, columnName);
				if (res.Any())
					return res.First().Message;
				return null;
			}
		}
		#endregion
	}
	public class ValidationRecursiveViewModel : Notifier<ValidationRecursiveViewModel>, IDataErrorInfo
	{
		public ValidationRecursiveViewModel(NotifierValidator validator) { Validator = validator; }

		[Required(ErrorMessage = "El id es requerido amigo mio !!")]
		[Range(1, 999999, ErrorMessage = "El id debe ser mayor que 0.")]
		public int? Id { get => GetValue<int?>(() => 1); set => SetValue(value); }
		[Display(Name = "Nombre")]
		[Required(AllowEmptyStrings = false, ErrorMessage = "Ponme un nombre, chiquitin !")]
		[StringLength(10, ErrorMessage = "El nombre no puede exceder de 10 caracteres de longitud")]
		[CustomValidation(typeof(ValidationViewModel), nameof(ValidationViewModel.ValidateName))]
		public string Name { get => GetValue(() => "Osca"); set => SetValue(value); }
		public static ValidationResult? ValidateName(string value)
		{
			if (value.ToLower().Contains("oscar"))
				return new ValidationResult("El nombre no puede ser Oscar");
			return ValidationResult.Success;
		}
		public NotifierValidator Validator
		{
			get => GetValue<NotifierValidator>();
			set => SetValue(value);
		}

		public GenericCommand BaseAsNullCommand => GetValue(() => new GenericCommand(() => Base2 = null));
		public GenericCommand BaseAsDerived1Command => GetValue(() => new GenericCommand(() => Base2 = new Derived1ValidationViewModel(Validator)));
		public GenericCommand BaseAsDerived2Command => GetValue(() => new GenericCommand(() => Base2 = new Derived2ValidationViewModel(Validator)));
		[RecursiveValidation]
		public BaseValidationViewModel? Base2 { get => GetValue<BaseValidationViewModel>(); set => SetValue(value); }

		#region IDataErrorInfo
		public string? Error => null;
		public string? this[string columnName]
		{
			get
			{
				var res = NotifierValidator.Validate(this, columnName);
				if (res.Any())
					return res.First().Message;
				return null;
			}
		}
		#endregion
		public override string ToString() => Name;
	}
	public class BaseValidationViewModel : Notifier<ValidationRecursiveViewModel>, IDataErrorInfo
	{
		public BaseValidationViewModel(NotifierValidator validator) { Validator = validator; }
		public NotifierValidator Validator
		{
			get => GetValue<NotifierValidator>();
			set => SetValue(value);
		}
		#region IDataErrorInfo
		public string? Error => null;
		public string? this[string columnName]
		{
			get
			{
				var res = NotifierValidator.Validate(this, columnName);
				if (res.Any())
					return res.First().Message;
				return null;
			}
		}
		#endregion
	}
	public class Derived1ValidationViewModel : BaseValidationViewModel
	{
		public Derived1ValidationViewModel(NotifierValidator validator) : base(validator) { }
		[Display(Name = "Derived1")]
		[Required(AllowEmptyStrings = false, ErrorMessage = "Ponme un Derived1")]
		[StringLength(10, ErrorMessage = "Derived1 no puede exceder de 10 caracteres de longitud")]
		public string Derived1 { get => GetValue<string>(); set => SetValue(value); }
	}
	public class Derived2ValidationViewModel : BaseValidationViewModel
	{
		public Derived2ValidationViewModel(NotifierValidator validator) : base(validator) { }
		[Display(Name = "Derived2")]
		[Required(AllowEmptyStrings = false, ErrorMessage = "Ponme un Derived2")]
		[StringLength(10, ErrorMessage = "Derived2 no puede exceder de 10 caracteres de longitud")]
		public string Derived2 { get => GetValue<string>(); set => SetValue(value); }
	}
}
