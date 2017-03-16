using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Biz.Morsink.DataConvert.Converters;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class DefaultConvertConverterTest
    {
        private DataConverter converter;

        [TestInitialize]
        public void Init()
        {
            converter = new DataConverter(new[] { new DefaultConvertConverter() });
        }
        [TestMethod]
        public void DefaultConvert_Happy()
        {
            Assert.AreEqual((short)1234, converter.Convert(1234).To<short>());
            Assert.AreEqual(123m, converter.Convert(123).To<decimal>());
            Assert.AreEqual(3.14159m, converter.Convert(3.14159).To<decimal>());
            Assert.AreEqual(42L, converter.Convert("42").To<long>());
            Assert.AreEqual("AQIDBAUGBwgJCg==", converter.Convert(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }).To<string>());
            Assert.AreEqual(1, converter.Convert(true).To<int>());
            Assert.AreEqual(0, converter.Convert(false).To<int>(-42));
            Assert.IsTrue(converter.Convert(1).To<bool>());
        }
        [TestMethod]
        public void DefaultConvert_Unhappy()
        {
            Assert.IsFalse(converter.DoConversion<string, int>("42x").IsSuccessful);
            Assert.IsFalse(converter.DoConversion<int, DateTime>(37).IsSuccessful);

        }
    }
}
