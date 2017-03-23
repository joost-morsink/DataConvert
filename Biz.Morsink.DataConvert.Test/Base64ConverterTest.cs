using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Biz.Morsink.DataConvert.Converters;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class Base64ConverterTest
    {
        private DataConverter converter;
        [TestInitialize]
        public void Init()
        {
            converter = new DataConverter(new[] { new Base64Converter() });
        }
        [TestMethod]
        public void Base64_Happy()
        {
            Assert.AreEqual("AAECAwQFBgcICQ==", converter.Convert(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }).To<string>());
            var res = converter.Convert("AAECAwQFBgcICQ==").To<byte[]>();
            Assert.AreEqual(10, res.Length);
            for (int i = 0; i < 10; i++)
                Assert.AreEqual(i, res[i]);
        }
        [TestMethod]
        public void Base64_Unhappy()
        {
            Assert.IsFalse(converter.Convert("AAECA$QFBgcICQ==").TryTo(out byte[] _));
            Assert.IsFalse(converter.Convert("AAECAwQFBgcICQ===").TryTo(out byte[] _));
            Assert.IsFalse(converter.Convert("AAECAwQFBgcICQ").TryTo(out byte[] _));
        }
    }
}
