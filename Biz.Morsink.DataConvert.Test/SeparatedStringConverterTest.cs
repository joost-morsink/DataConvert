using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Biz.Morsink.DataConvert.Converters;
using System.Collections.Generic;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class SeparatedStringConverterTest
    {
        private DataConverter converter;

        [TestInitialize]
        public void Init()
        {
            converter = new DataConverter(IdentityConverter.Instance, new TryParseConverter(), new SeparatedStringConverter('-'), EnumerableToTupleConverter.Instance, TupleToArrayConverter.Instance);
        }
        [TestMethod]
        public void SeparatedString_Happy()
        {
            var (x, y, z) = converter.Convert("1-xyz-456789").To<(int, string, int)>();
            Assert.AreEqual(1, x);
            Assert.AreEqual("xyz", y);
            Assert.AreEqual(456789, z);
        }
        [TestMethod]
        public void SeparatedString_HappyFromStringArray()
        {
            Assert.AreEqual("1-xyz-234567", converter.Convert(new[] { "1", "xyz", "234567" }).To<string>());
        }
        [TestMethod]
        public void SeparatedString_HappyFromObjectArray()
        {
            Assert.AreEqual("1-xyz-234567", converter.Convert(new object[] { 1, "xyz", 234567 }).To<string>());
        }
        [TestMethod]
        public void SeparatedString_HappyFromIntEnumerable()
        {
            Assert.AreEqual("1-234-567890", converter.Convert(new int[] { 1, 234, 567890 }).To<string>());
        }
        [TestMethod]
        public void SeparatedString_HappyFromTuple()
        {
            Assert.AreEqual("1-xyz-234567", converter.Convert(("1", "xyz", "234567")).To<string>());
        }
        [TestMethod]
        public void SeparatedString_Unhappy()
        {
            Assert.IsFalse(converter.DoConversion<string, (int, int, int)>("1-xyz-456789").IsSuccessful);
            Assert.IsFalse(converter.DoConversion<string, (int, int)>("123").IsSuccessful);
            Assert.IsFalse(converter.DoConversion<string, (int, int)>("").IsSuccessful);
            Assert.IsFalse(converter.DoConversion<string[], string>(new string[0]).IsSuccessful);
            Assert.IsFalse(converter.DoConversion<IEnumerable<string>, string>(new string[0]).IsSuccessful);
        }
    }
}
