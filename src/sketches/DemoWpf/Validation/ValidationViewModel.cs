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

namespace DemoWpf.Validation
{
    [ConditionalValidation(typeof(ValidationViewModel),nameof(MustBeValidate))]
    public class ValidationViewModel : Notifier<ValidationViewModel>, IDataErrorInfo //,INotifyDataErrorInfo
    {
        public ValidationViewModel()
        {
            Validator.RegisterNotifier(this);
            //Validator = new ValidationHelper();
            //NotifyDataErrorInfoAdapter = new NotifyDataErrorInfoAdapter(Validator);

            //Validator.AddRule(nameof(Name),
            //      () => RuleResult.Assert(!string.IsNullOrEmpty(Name), "Name is required"));
        }

        public GenericCommand DoCommand => new GenericCommand(() =>
        {
            //var validationResults = new List<ValidationResult>();
            //Validator.TryValidateObject(this, new ValidationContext(this), validationResults);
            Debug.WriteLine("");
        });
        public GenericCommand AddCommand => new GenericCommand(() =>
        {
            ValidationRecursiveCollection.Add(new ValidationRecursiveViewModel(Validator)
            {
                Id = -1,
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
        public int? Id { get => GetValue<int?>(() => -1); set => SetValue(value); }

        [Display(Name = nameof(Name), ResourceType = typeof(TextLocalized))]
        [Required(ErrorMessageResourceType = typeof(TextLocalized), ErrorMessageResourceName = nameof(TextLocalized.Required))]
        [StringLength(10, ErrorMessageResourceType = typeof(TextLocalized), ErrorMessageResourceName = nameof(TextLocalized.StringLength))]
        [CustomValidation(typeof(ValidationViewModel), nameof(ValidationViewModel.ValidateName))]
        public string Name { get => GetValue(() => "Osca"); set => SetValue(value); }

        public static ValidationResult ValidateName(string value)
        {
            if (value.ToLower().Contains("oscar"))
                return new ValidationResult("El nombre no puede ser Oscar");
            return ValidationResult.Success;
        }
        [Required(ErrorMessage = "ValidationRecursive debe setearse")]
        [RecursiveValidation]
        public ValidationRecursiveViewModel ValidationRecursive {
            get => GetValue(() => new ValidationRecursiveViewModel(Validator));
            set => SetValue(value);
        }
        [RecursiveValidation]
        [EnsureMinimumElements(1, ErrorMessage = "El menos debe haber un elemento")]
        public ObservableCollection<ValidationRecursiveViewModel> ValidationRecursiveCollection
        {
            get => GetValue(() => new ObservableCollection<ValidationRecursiveViewModel>(new[]{
                new ValidationRecursiveViewModel(Validator)
                {
                    Id = -1,
                    Name = "Oscar"
                }
            }));
            set => SetValue(value);
        }

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
        public string Error => null;
        public string this[string columnName]
        {
            get
            {
                var res = Validator.Validate(this, columnName);
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
        [Required(AllowEmptyStrings = false, ErrorMessage = "Ponme un nombre, chiquitin !")]
        [StringLength(10, ErrorMessage = "El nombre no puede exceder de 10 caracteres de longitud")]
        [CustomValidation(typeof(ValidationViewModel), nameof(ValidationViewModel.ValidateName))]
        public string Name { get => GetValue(() => "Osca"); set => SetValue(value); }
        public static ValidationResult ValidateName(string value)
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
        #region IDataErrorInfo
        public string Error => null;
        public string this[string columnName]
        {
            get
            {
                var res = Validator.Validate(this, columnName);
                if (res.Any())
                    return res.First().Message;
                return null;
            }
        }
        #endregion
        public override string ToString() => Name;
    }
}
