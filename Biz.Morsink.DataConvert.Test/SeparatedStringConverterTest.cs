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
            Assert.AreEqual(1, x, "First component of separated string should convert to int");
            Assert.AreEqual("xyz", y, "Second component of separated string should be the substring between separators");
            Assert.AreEqual(456789, z, "Third component of separated string should convert to int");
        }
        [TestMethod]
        public void SeparatedString_HappyFromStringArray()
        {
            Assert.AreEqual("1-xyz-234567", converter.Convert(new[] { "1", "xyz", "234567" }).To<string>(), "String array should convert to separated string");
        }
        [TestMethod]
        public void SeparatedString_HappyFromObjectArray()
        {
            Assert.AreEqual("1-xyz-234567", converter.Convert(new object[] { 1, "xyz", 234567 }).To<string>(), "Mixed type object array should convert to separated string");
        }
        [TestMethod]
        public void SeparatedString_HappyFromIntEnumerable()
        {
            Assert.AreEqual("1-234-567890", converter.Convert(new int[] { 1, 234, 567890 }).To<string>(), "Int array should convert to separated string");
        }
        [TestMethod]
        public void SeparatedString_HappyFromTuple()
        {
            Assert.AreEqual("1-xyz-234567", converter.Convert(("1", "xyz", "234567")).To<string>(), "ValueTuple should convert to separated string");
        }
        [TestMethod]
        public void SeparatedString_Unhappy()
        {
            Assert.IsFalse(converter.DoConversion<string, (int, int, int)>("1-xyz-456789").IsSuccessful, "SeparatedStringCoverter should fail if one component fails");
            Assert.IsFalse(converter.DoConversion<string, (int, int)>("123").IsSuccessful, "SeparatedStringConverter should fail if now enough parts are in the string");
            Assert.IsFalse(converter.DoConversion<string, (int, int)>("").IsSuccessful, "SeparatedStringConverter should fail on empty string if arity>1");
            Assert.IsFalse(converter.DoConversion<string[], string>(new string[0]).IsSuccessful, "SeparatedStringConverter should fail on empty arrays");
            Assert.IsFalse(converter.DoConversion<IEnumerable<string>, string>(new string[0]).IsSuccessful, "SeparatedStringConverter should fail on empty enumerable");
        }
    }
}
