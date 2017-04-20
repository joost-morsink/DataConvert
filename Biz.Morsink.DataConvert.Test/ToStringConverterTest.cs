using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Biz.Morsink.DataConvert;
using Biz.Morsink.DataConvert.Converters;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class ToStringConverterTest
    {

        [TestMethod]
        public void ToString_Basics()
        {
            var conv = new DataConverter(new ToStringConverter(false));
            Assert.AreEqual("42", conv.Convert(42).To<string>(), "Int should convert to string representation");
            Assert.AreEqual("1234.56", conv.Convert(1234.56m).To<string>(), "Decimal should convert to standardized string representation");
            Assert.IsNull(conv.Convert<object>(null).To<string>(), "Null should convert to null string");
        }
        [TestMethod]
        public void ToString_SucceedOnNull()
        {
            var conv = new DataConverter(new ToStringConverter(true));
            Assert.IsNotNull(conv.Convert<string>(null).To<string>(), "Null should not convert to null string");
            Assert.AreEqual("", conv.Convert<string>(null).To<string>(), "Null should convert to empty string");
            Assert.AreEqual("42", conv.Convert(42).To<string>(), "Int should convert to string representation");
            Assert.AreEqual("1234.56", conv.Convert(1234.56m).To<string>(), "Decimal should convert to standardized string representation");
        }
        [TestMethod]
        public void ToString_OtherFormatProvider()
        {
            var conv = new DataConverter(new ToStringConverter(false, new CultureInfo("nl-nl")));
            Assert.AreEqual("1234,56", conv.Convert(1234.56m).To<string>(), "NL-nl culture should use comma as a decimal separator");
        }
        [TestMethod]
        public void ToString_NoObjects()
        {
            var conv = new DataConverter(new ToStringConverter(false));
            Assert.AreEqual("ERROR", conv.Convert<object>(42).To<string>("ERROR"), "ToStringConverter should fail on untyped object");
        }
    }
}
