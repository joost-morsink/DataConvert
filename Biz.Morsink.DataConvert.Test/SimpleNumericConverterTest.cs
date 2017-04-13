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
            converter = new DataConverter(SimpleNumericConverter.Instance);
        }

        [TestMethod]
        public void SimpleNumeric_HappyToBigger()
        {
            Assert.AreEqual((short)42, converter.Convert<sbyte>(42).To<short>(), "Sbyte should convert to short");
            Assert.AreEqual(42, converter.Convert<short>(42).To<int>(), "Short should convert to int");
            Assert.AreEqual(42L, converter.Convert(42).To<long>(), "Int should convert to long");
            Assert.AreEqual(42m, converter.Convert(42).To<decimal>(), "Int should convert to decimal");
            Assert.AreEqual(3.14159, converter.Convert(3.14159f).To<double>(), 1e-6, "Float should convert to double");
            Assert.AreEqual(3.14159, converter.Convert(3.14159m).To<double>(), 1e-6, "Decimal should convert to double");
        }

        [TestMethod]
        public void SimpleNumeric_HappyToSmaller()
        {
            Assert.AreEqual((sbyte)42, converter.Convert(42).To<sbyte>(), "Int within range should convert to sbyte");
            Assert.AreEqual((short)42, converter.Convert(42).To<short>(), "Int within range should convert to short");
            Assert.AreEqual(42, converter.Convert(42L).To<int>(), "Long within range should convert to int");
            Assert.AreEqual(3.14159f, converter.Convert(3.14159).To<float>(), 1e-6, "Double within range should convert to float");
            Assert.AreEqual(3, converter.Convert(3.14159m).To<int>(), "Decimal within range should convert to int");
        }

        [TestMethod]
        public void SimpleNumeric_Unhappy()
        {
            Assert.IsFalse(converter.Convert(100000).TryTo(out short _),"Int out of range should fail conversion to short");
            Assert.IsFalse(converter.Convert(1e99).TryTo(out float _), "Double out of range should fail conversion to float");
            Assert.IsFalse(converter.Convert(1e38f).TryTo(out decimal _), "Float out of range should fail conversion to decimal");
        }
    }
}