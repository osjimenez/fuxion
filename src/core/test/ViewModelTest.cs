using System;
using Fuxion.Windows;
using Xunit;

namespace Fuxion.Test
{
    public class ViewModelTest
    {
        [Fact]
        public void PropertyChangedRaise()
        {
            //DemoViewModel dvm = new DemoViewModel();
            //bool changed = false;
            //dvm.PropertyChanged += (n, e) => e.Case(() => n.Name, a => {
            //    changed = true;
            //});
            //dvm.Name = "oka";
            //Assert.IsTrue(changed);
        }
        [Fact]
        public void RaisePropertyChangedMethod()
        {
            //DemoViewModel dvm = new DemoViewModel();
            //bool changed = false;
            //dvm.PropertyChanged += (n, e) => e.Case(() => n.Name, a => {
            //    changed = true;
            //});
            //dvm.ChangeName("oka");
            //Assert.IsTrue(changed);
        }
    }
    //public class DemoViewModel : ViewModel<DemoViewModel>
    //{
    //    public string Name
    //    {
    //        get { return GetValue<string>(); }
    //        set { SetValue(value); }
    //    }
    //    public void ChangeName(string newValue)
    //    {
    //        RaisePropertyChanged(() => Name,"", newValue);
    //    }
    //}
}
