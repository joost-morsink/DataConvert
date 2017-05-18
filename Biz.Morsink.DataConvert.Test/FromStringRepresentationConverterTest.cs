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
            Assert.IsTrue(converter.DoConversion<int, string>(42).IsSuccessful, "Int should have a string representation");
            Assert.AreEqual(1.0, converter.Convert(new Version("1.0")).To<double>(), "Version string representation should convert to double in absence of third component");
            Assert.AreEqual("42", converter.Convert(42).To<string>(), "Int should convert to string");
        }
        [TestMethod]
        public void FromStringRep_HappyNumeric()
        {
            Assert.AreEqual(42.0, converter.Convert(42).To<double>(), "Int representation should be parseable to double");
            Assert.AreEqual(3.14159m, converter.Convert(3.14159).To<decimal>(), "Double representation should be parseable to decimal");
            Assert.AreEqual(3.14159, converter.Convert(3.14159m).To<double>(), 0.000001, "Decimal representation should be parseable to double");
        }
        [TestMethod]
        public void FromStringRep_Unhappy()
        {
            Assert.IsFalse(converter.DoConversion<DateTime, int>(new DateTime(2017, 3, 16, 8, 2, 0, DateTimeKind.Utc)).IsSuccessful, "DateTime representation should not be parseable to int");
            Assert.IsFalse(converter.DoConversion<Version, double>(new Version("1.0.0")).IsSuccessful, "Version string with third component should fail on numeric parse methods");
            Assert.IsFalse(converter.DoConversion<int, Guid>(42).IsSuccessful, "Int representation should not convert to Guid");
            Assert.IsFalse(converter.DoConversion<NoToStringMethod, string>(new NoToStringMethod()).IsSuccessful);
        }
        [TestMethod]
        public void FromStringRep_ValueToRefWithoutEqual()
        {
            var res = converter.DoConversion<(int, int, int), string>((1, 2, 3));
            Assert.IsTrue(res.IsSuccessful);
            Assert.AreEqual("(1, 2, 3)", res.Result);
        }
        public class NoToStringMethod
        {

        }
    }
}
