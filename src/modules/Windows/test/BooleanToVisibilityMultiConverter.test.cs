using Fuxion.Test;
using Fuxion.Windows.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Windows.Test
{

    public class BooleanToVisibilityMultiConverterTest : BaseTest
    {
        public BooleanToVisibilityMultiConverterTest(ITestOutputHelper output) : base(output) { }
        [Fact(DisplayName = "BooleanToVisibilityMultiConverter - AllTrue")]
        public void BooleanToVisibilityMultiConverter_AllTrue()
        {
            var c = new BooleanToVisibilityMultiConverter();
            c.Mode = BooleanMultiConverterMode.AllTrue;
            var res = c.Convert(new[] { true, true }, CultureInfo.CurrentCulture);
            Assert.Equal(Visibility.Visible, res);
            res = c.Convert(new[] { false, false }, CultureInfo.CurrentCulture);
            Assert.Equal(Visibility.Collapsed, res);
            res = c.Convert(new[] { true, false }, CultureInfo.CurrentCulture);
            Assert.Equal(Visibility.Collapsed, res);
            res = c.Convert(new[] { true }, CultureInfo.CurrentCulture);
            Assert.Equal(Visibility.Visible, res);
            res = c.Convert(new[] { false }, CultureInfo.CurrentCulture);
            Assert.Equal(Visibility.Collapsed, res);
        }
        [Fact(DisplayName = "BooleanToVisibilityMultiConverter - AnyTrue")]
        public void BooleanToVisibilityMultiConverter_AnyTrue()
        {
            var c = new BooleanToVisibilityMultiConverter();
            c.Mode = BooleanMultiConverterMode.AnyTrue;
            var res = c.Convert(new[] { true, true }, CultureInfo.CurrentCulture);
            Assert.Equal(Visibility.Visible, res);
            res = c.Convert(new[] { false, false }, CultureInfo.CurrentCulture);
            Assert.Equal(Visibility.Collapsed, res);
            res = c.Convert(new[] { true, false }, CultureInfo.CurrentCulture);
            Assert.Equal(Visibility.Visible, res);
            res = c.Convert(new[] { true }, CultureInfo.CurrentCulture);
            Assert.Equal(Visibility.Visible, res);
            res = c.Convert(new[] { false }, CultureInfo.CurrentCulture);
            Assert.Equal(Visibility.Collapsed, res);
        }
        [Fact(DisplayName = "BooleanToVisibilityMultiConverter - AllFalse")]
        public void BooleanToVisibilityMultiConverter_AllFalse()
        {
            var c = new BooleanToVisibilityMultiConverter();
            c.Mode = BooleanMultiConverterMode.AllFalse;
            var res = c.Convert(new[] { true, true }, CultureInfo.CurrentCulture);
            Assert.Equal(Visibility.Collapsed, res);
            res = c.Convert(new[] { false, false }, CultureInfo.CurrentCulture);
            Assert.Equal(Visibility.Visible, res);
            res = c.Convert(new[] { true, false }, CultureInfo.CurrentCulture);
            Assert.Equal(Visibility.Collapsed, res);
            res = c.Convert(new[] { true }, CultureInfo.CurrentCulture);
            Assert.Equal(Visibility.Collapsed, res);
            res = c.Convert(new[] { false }, CultureInfo.CurrentCulture);
            Assert.Equal(Visibility.Visible, res);
        }
        [Fact(DisplayName = "BooleanToVisibilityMultiConverter - AnyFalse")]
        public void BooleanToVisibilityMultiConverter_AnyFalse()
        {
            var c = new BooleanToVisibilityMultiConverter();
            c.Mode = BooleanMultiConverterMode.AnyFalse;
            var res = c.Convert(new[] { true, true }, CultureInfo.CurrentCulture);
            Assert.Equal(Visibility.Collapsed, res);
            res = c.Convert(new[] { false, false }, CultureInfo.CurrentCulture);
            Assert.Equal(Visibility.Visible, res);
            res = c.Convert(new[] { true, false }, CultureInfo.CurrentCulture);
            Assert.Equal(Visibility.Visible, res);
            res = c.Convert(new[] { true }, CultureInfo.CurrentCulture);
            Assert.Equal(Visibility.Collapsed, res);
            res = c.Convert(new[] { false }, CultureInfo.CurrentCulture);
            Assert.Equal(Visibility.Visible, res);
        }
        [Fact(DisplayName = "BooleanToVisibilityMultiConverter - NullValue")]
        public void BooleanToVisibilityMultiConverter_NullValue()
        {
            var c = new BooleanToVisibilityMultiConverter();
            c.NullValue = Visibility.Hidden;
            var res = c.Convert(null, CultureInfo.CurrentCulture);
            Assert.Equal(Visibility.Hidden, res);
        }
        [Fact(DisplayName = "BooleanToVisibilityMultiConverter - EmptyValue")]
        public void BooleanToVisibilityMultiConverter_EmptyValue()
        {
            var c = new BooleanToVisibilityMultiConverter();
            c.EmptyValue = Visibility.Hidden;
            var res = c.Convert(new bool[] { }, CultureInfo.CurrentCulture);
            Assert.Equal(Visibility.Hidden, res);
        }
    }
}
