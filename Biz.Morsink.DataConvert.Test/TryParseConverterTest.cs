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
            var conv = new DataConverter(new[] { new TryParseConverter() });
            Assert.AreEqual(42, conv.Convert("42").To<int>());
            Assert.AreEqual(new DateTime(2017, 3, 12, 19, 15, 0, DateTimeKind.Utc), conv.Convert("2017-03-12T19:15:00Z").To<DateTime>().ToUniversalTime());
            Assert.AreEqual(new Guid("12345678-1234-1234-1234-1234567890ab"), conv.Convert("12345678-1234-1234-1234-1234567890ab").To<Guid>());
        }
        [TestMethod]
        public void TryParse_Unhappy()
        {
            var conv = new DataConverter(new[] { new TryParseConverter() });
            Assert.IsFalse(conv.Convert("42x").TryTo<int>(out var _));
            Assert.IsFalse(conv.Convert("42.0").TryTo<int>(out var _));
            Assert.IsFalse(conv.Convert("12345678-1234-1234-1234-1234567890abc").TryTo<Guid>(out var _));
            Assert.IsFalse(conv.Convert("12345678-1234-1234-1234-12345678g0ab").TryTo<Guid>(out var _));
        }
    }
}
