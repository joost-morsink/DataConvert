using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Biz.Morsink.DataConvert.Converters;
namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class SeparatedStringConverterTest
    {
        private DataConverter converter;

        [TestInitialize]
        public void Init()
        {
            converter = new DataConverter(IdentityConverter.Instance, new TryParseConverter(), new SeparatedStringConverter('-'), EnumerableToTupleConverter.Instance);
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
        public void SeparatedString_Unhappy()
        {
            Assert.IsFalse(converter.DoConversion<string, (int, int, int)>("1-xyz-456789").IsSuccessful);
        }
    }
}
