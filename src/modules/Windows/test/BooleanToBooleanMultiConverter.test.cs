using Fuxion.Test;
using Fuxion.Windows.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Windows.Test
{

    public class BooleanToBooleanMultiConverterTest : BaseTest
    {
        public BooleanToBooleanMultiConverterTest(ITestOutputHelper output) : base(output) { }
        [Fact(DisplayName = "BooleanToBooleanMultiConverter - AllTrue")]
        public void BooleanToBooleanMultiConverter_AllTrue()
        {
            var c = new BooleanToBooleanMultiConverter();
            c.Mode = BooleanMultiConverterMode.AllTrue;
            Assert.True(c.Convert(new[] { true, true }, CultureInfo.CurrentCulture));
            Assert.False(c.Convert(new[] { false, false }, CultureInfo.CurrentCulture));
            Assert.False(c.Convert(new[] { true, false }, CultureInfo.CurrentCulture));
            Assert.True(c.Convert(new[] { true }, CultureInfo.CurrentCulture));
            Assert.False(c.Convert(new[] { false }, CultureInfo.CurrentCulture));
        }
        [Fact(DisplayName = "BooleanToBooleanMultiConverter - AnyTrue")]
        public void BooleanToBooleanMultiConverter_AnyTrue()
        {
            var c = new BooleanToBooleanMultiConverter();
            c.Mode = BooleanMultiConverterMode.AnyTrue;
            Assert.True(c.Convert(new[] { true, true }, CultureInfo.CurrentCulture));
            Assert.False(c.Convert(new[] { false, false }, CultureInfo.CurrentCulture));
            Assert.True(c.Convert(new[] { true, false }, CultureInfo.CurrentCulture));
            Assert.True(c.Convert(new[] { true }, CultureInfo.CurrentCulture));
            Assert.False(c.Convert(new[] { false }, CultureInfo.CurrentCulture));
        }
        [Fact(DisplayName = "BooleanToBooleanMultiConverter - AllFalse")]
        public void BooleanToBooleanMultiConverter_AllFalse()
        {
            var c = new BooleanToBooleanMultiConverter();
            c.Mode = BooleanMultiConverterMode.AllFalse;
            Assert.False(c.Convert(new[] { true, true }, CultureInfo.CurrentCulture));
            Assert.True(c.Convert(new[] { false, false }, CultureInfo.CurrentCulture));
            Assert.False(c.Convert(new[] { true, false }, CultureInfo.CurrentCulture));
            Assert.False(c.Convert(new[] { true }, CultureInfo.CurrentCulture));
            Assert.True(c.Convert(new[] { false }, CultureInfo.CurrentCulture));
        }
        [Fact(DisplayName = "BooleanToBooleanMultiConverter - AnyFalse")]
        public void BooleanToBooleanMultiConverter_AnyFalse()
        {
            var c = new BooleanToBooleanMultiConverter();
            c.Mode = BooleanMultiConverterMode.AnyFalse;
            Assert.False(c.Convert(new[] { true, true }, CultureInfo.CurrentCulture));
            Assert.True(c.Convert(new[] { false, false }, CultureInfo.CurrentCulture));
            Assert.True(c.Convert(new[] { true, false }, CultureInfo.CurrentCulture));
            Assert.False(c.Convert(new[] { true }, CultureInfo.CurrentCulture));
            Assert.True(c.Convert(new[] { false }, CultureInfo.CurrentCulture));
        }
        [Fact(DisplayName = "BooleanToBooleanMultiConverter - NullValue")]
        public void BooleanToBooleanMultiConverter_NullValue()
        {
            var c = new BooleanToBooleanMultiConverter();
            Assert.False(c.Convert(null, CultureInfo.CurrentCulture));
        }
        [Fact(DisplayName = "BooleanToBooleanMultiConverter - EmptyValue")]
        public void BooleanToBooleanMultiConverter_EmptyValue()
        {
            var c = new BooleanToBooleanMultiConverter();
            Assert.False(c.Convert(new bool[] { }, CultureInfo.CurrentCulture));
        }
    }
}
