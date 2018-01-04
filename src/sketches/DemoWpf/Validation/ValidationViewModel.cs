using Fuxion.ComponentModel;
using Fuxion.ComponentModel.DataAnnotations;
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
using System.Threading.Tasks;

namespace DemoWpf.Validation
{
    public class ValidationViewModel : Notifier<ValidationViewModel>, IDataErrorInfo //, INotifyDataErrorInfo
    {
        public ValidationViewModel()
        {
            //Validator = new ValidationHelper();
            //NotifyDataErrorInfoAdapter = new NotifyDataErrorInfoAdapter(Validator);

            //Validator.AddRule(nameof(Name),
            //      () => RuleResult.Assert(!string.IsNullOrEmpty(Name), "Name is required"));
        }

        public GenericCommand DoCommand => new GenericCommand(() =>
        {
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(this, new ValidationContext(this), validationResults);
            Debug.WriteLine("");
        });

        [Required(ErrorMessage = "El id es requerido amigo mio !!")]
        [Range(1, 999999, ErrorMessage = "El id debe ser mayor que 0.")]
        public int? Id { get => GetValue<int?>(() => 1); set => SetValue(value); }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Ponme un nombre, chiquitin !")]
        [StringLength(10, ErrorMessage = "El nombre no puede exceder de 10 caracteres de longitud")]
        [CustomValidation(typeof(CustomValidations), nameof(CustomValidations.ValidateName))]
        public string Name { get => GetValue<string>(() => "Oscar"); set => SetValue(value); }


        // check for general model error    
        public string Error => null;
        // check for property errors    
        public string this[string columnName]
        {
            get
            {
                var validationResults = new List<ValidationResult>();

                
                if (Validator.TryValidateProperty(
                        GetType().GetProperty(columnName).GetValue(this)
                        , new ValidationContext(this)
                        {
                            MemberName = columnName
                        }
                        , validationResults))
                    return null;

                return validationResults.First().ErrorMessage;
            }
        }


        public Validator2 Validator2 { get => GetValue(() => new Validator2(this)); }
        //protected ValidationHelper Validator { get; private set; }
        //private NotifyDataErrorInfoAdapter NotifyDataErrorInfoAdapter { get; set; }

        //#region INotifyDataErrorInfo
        //public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged
        //{
        //    add { NotifyDataErrorInfoAdapter.ErrorsChanged += value; }
        //    remove { NotifyDataErrorInfoAdapter.ErrorsChanged -= value; }
        //}
        //public bool HasErrors => NotifyDataErrorInfoAdapter.HasErrors;
        //public IEnumerable GetErrors(string propertyName) => NotifyDataErrorInfoAdapter.GetErrors(propertyName);
        //#endregion
    }
    public static class CustomValidations
    {
        public static ValidationResult ValidateName(string value)
        {
            if(value.ToLower().Contains("oscar"))
                return new ValidationResult("El nombre no puede ser Oscar");
            return ValidationResult.Success;
        }
    }
    
}
