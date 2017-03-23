using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Biz.Morsink.DataConvert.Converters;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class FromStringRepresentationConverterTest
    {
        private DataConverter converter;

        [TestInitialize]
        public void Init()
        {
            converter = new DataConverter(
                IdentityConverter.Instance,
                new FromStringRepresentationConverter(),
                new TryParseConverter());
        }
        [TestMethod]
        public void FromStringRep_HappySimple()
        {
            Assert.IsTrue(converter.DoConversion<int, string>(42).IsSuccessful);
            Assert.AreEqual(1.0, converter.Convert(new Version("1.0")).To<double>());
            Assert.AreEqual("42", converter.Convert(42).To<string>());
        }
        [TestMethod]
        public void FromStringRep_HappyNumeric()
        {
            Assert.AreEqual(42.0, converter.Convert(42).To<double>());
            Assert.AreEqual(3.14159m, converter.Convert(3.14159).To<decimal>());
            Assert.AreEqual(3.14159, converter.Convert(3.14159m).To<double>(), 0.000001);
        }
        [TestMethod]
        public void FromStringRep_Unhappy()
        {
            Assert.IsFalse(converter.DoConversion<DateTime, int>(new DateTime(2017, 3, 16, 8, 2, 0, DateTimeKind.Utc)).IsSuccessful);
            Assert.IsFalse(converter.DoConversion<Version, double>(new Version("1.0.0")).IsSuccessful);
            Assert.IsFalse(converter.DoConversion<int, Guid>(42).IsSuccessful);
        }
    }
}
