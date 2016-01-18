using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Diagnostics;
using Fuxion.ComponentModel;

namespace Fuxion.Windows
{
    public abstract class ViewModel<TViewModel> : Notifier<TViewModel>, IViewModel
        where TViewModel : ViewModel<TViewModel>
    {
        protected ViewModel()
        {
            Debug.WriteLine("============================ CREATING VIEWMODEL: " + GetType().Name);
        }
        public Guid Id { get { return GetValue<Guid>(); } set { SetValue(value); } }
        public abstract string Name { get; set; }
        #region Equals
        public override int GetHashCode() { return Id.GetHashCode(); }
        public override bool Equals(object obj) { return obj is IViewModel ? ((IViewModel)obj).Id == Id : false; }
        public static bool operator ==(ViewModel<TViewModel> a, IViewModel b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.Id == b.Id;
        }
        public static bool operator !=(ViewModel<TViewModel> a, IViewModel b) { return !(a == b); }
        #endregion
    }
}
