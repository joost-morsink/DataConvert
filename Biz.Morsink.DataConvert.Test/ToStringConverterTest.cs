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
            Assert.AreEqual("42", conv.Convert(42).To<string>());
            Assert.AreEqual("1234.56", conv.Convert(1234.56m).To<string>());
            Assert.IsNull(conv.Convert<object>(null).To<string>());
        }
        [TestMethod]
        public void ToString_SucceedOnNull() {
            var conv = new DataConverter(new ToStringConverter(true));
            Assert.IsNotNull(conv.Convert<string>(null).To<string>());
            Assert.AreEqual("", conv.Convert<string>(null).To<string>());
            Assert.AreEqual("42", conv.Convert(42).To<string>());
            Assert.AreEqual("1234.56", conv.Convert(1234.56m).To<string>());
        }
        [TestMethod]
        public void ToString_OtherFormatProvider()
        {
            var conv = new DataConverter(new ToStringConverter(false, CultureInfo.GetCultureInfo("nl-nl")));
            Assert.AreEqual("1234,56", conv.Convert(1234.56m).To<string>());
        }
        [TestMethod]
        public void ToString_NoObjects()
        {
            var conv = new DataConverter(new ToStringConverter(false));
            Assert.AreEqual("ERROR", conv.Convert<object>(42).To<string>("ERROR"));
        }
    }
}
