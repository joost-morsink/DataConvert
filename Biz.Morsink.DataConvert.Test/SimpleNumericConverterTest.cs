using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Biz.Morsink.DataConvert.Converters;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class SimpleNumericConverterTest
    {
        private DataConverter converter;

        [TestInitialize]
        public void Init()
        {
            converter = new DataConverter(new[] { new SimpleNumericConverter() });
        }

        [TestMethod]
        public void SimpleNumeric_HappyToBigger()
        {
            Assert.AreEqual((short)42, converter.Convert<sbyte>(42).To<short>());
            Assert.AreEqual(42, converter.Convert<short>(42).To<int>());
            Assert.AreEqual(42L, converter.Convert(42).To<long>());
            Assert.AreEqual(42m, converter.Convert(42).To<decimal>());
            Assert.AreEqual(3.14159, converter.Convert(3.14159f).To<double>(), 1e-6);
            Assert.AreEqual(3.14159, converter.Convert(3.14159m).To<double>(), 1e-6);
        }

        [TestMethod]
        public void SimpleNumeric_HappyToSmaller()
        {
            Assert.AreEqual((sbyte)42, converter.Convert(42).To<sbyte>());
            Assert.AreEqual((short)42, converter.Convert(42).To<short>());
            Assert.AreEqual(42, converter.Convert(42L).To<int>());
            Assert.AreEqual(3.14159f, converter.Convert(3.14159).To<float>(), 1e-6);
            Assert.AreEqual(3, converter.Convert(3.14159m).To<int>());
        }
        
        [TestMethod]
        public void SimpleNumeric_Unhappy()
        {
            Assert.IsFalse(converter.Convert(100000).TryTo(out short _));
            Assert.IsFalse(converter.Convert(1e99).TryTo(out float _));
            Assert.IsFalse(converter.Convert(1e38f).TryTo(out decimal _));
        }
    }
}
