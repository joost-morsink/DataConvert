using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Biz.Morsink.DataConvert.Converters;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class TryParseConverterTest
    {
        [TestMethod]
        public void TryParse_Basics()
        {
            var tpc = new TryParseConverter();
            Assert.IsTrue(tpc.CanConvert(typeof(string), typeof(int)));
            Assert.IsTrue(tpc.CanConvert(typeof(string), typeof(Guid)));
            Assert.IsFalse(tpc.CanConvert(typeof(int), typeof(int)));
        }
        [TestMethod]
        public void TryParse_Happy()
        {
            var conv = new DataConverter(new TryParseConverter());
            Assert.AreEqual(42, conv.Convert("42").To<int>(), "Valid parseable int string should succeed conversion");
            Assert.AreEqual(new DateTime(2017, 3, 12, 19, 15, 0, DateTimeKind.Utc), conv.Convert("2017-03-12T19:15:00Z").To<DateTime>().ToUniversalTime(), "Valid parseable DateTime string should succeed conversion");
            Assert.AreEqual(new Guid("12345678-1234-1234-1234-1234567890ab"), conv.Convert("12345678-1234-1234-1234-1234567890ab").To<Guid>(), "Valid parseable Guid string should succeed conversion");
        }
        [TestMethod]
        public void TryParse_HappyDecimalSeparator()
        {
            var conv = new DataConverter(new TryParseConverter(numberStyles: System.Globalization.NumberStyles.Any));
            Assert.AreEqual(42.0, conv.Convert("42.0").To<double>(), "Valid parseable double string should convert to double (NumberStyles.Any)");
            Assert.AreEqual(420, conv.Convert("42,0").To<double>(), "Valid parseable double string should lead to strange result with NumberStyles.Any");
        }
        [TestMethod]
        public void TryParse_UnhappyDecimals()
        {
            var conv = new DataConverter(new TryParseConverter(numberStyles: System.Globalization.NumberStyles.Float));
            Assert.IsFalse(conv.DoConversion<string, double>("42,00").IsSuccessful, "Unexpected decimal separator should fail conversion (NumberStyles.Float)");
        }
        [TestMethod]
        public void TryParse_Unhappy()
        {
            var conv = new DataConverter(new TryParseConverter());
            Assert.IsFalse(conv.Convert("42x").TryTo<int>(out var _), "Unparseable int string should fail conversion");
            Assert.IsFalse(conv.Convert("42,0").TryTo<int>(out var _), "Unexpected separator character should fail conversion");
            Assert.IsFalse(conv.Convert("12345678-1234-1234-1234-1234567890abc").TryTo<Guid>(out var _), "Guid string that is too long should fail conversion");
            Assert.IsFalse(conv.Convert("12345678-1234-1234-1234-12345678g0ab").TryTo<Guid>(out var _), "Invalid characters in Guid string should fail conversion");
        }
    }
}
