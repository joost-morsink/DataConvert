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
            converter = new DataConverter(Base64Converter.Instance);
        }
        [TestMethod]
        public void Base64_Happy()
        {
            Assert.AreEqual("AAECAwQFBgcICQ==", converter.Convert(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }).To<string>(), "Byte array should convert to Base64 string");
            var res = converter.Convert("AAECAwQFBgcICQ==").To<byte[]>();
            Assert.AreEqual(10, res.Length, "Base64 string should convert to byte array (array length)");
            for (int i = 0; i < 10; i++)
                Assert.AreEqual(i, res[i], "Base64 string should convert to byte array (array content)");
        }
        [TestMethod]
        public void Base64_Unhappy()
        {
            Assert.IsFalse(converter.Convert("AAECA$QFBgcICQ==").TryTo(out byte[] _), "Invalid base64 character should fail conversion");
            Assert.IsFalse(converter.Convert("AAECAwQFBgcICQ===").TryTo(out byte[] _), "Incorrect base64 terminator should fail conversion");
            Assert.IsFalse(converter.Convert("AAECAwQFBgcICQ").TryTo(out byte[] _), "Incorrect base64 length should fail conversion");
        }
    }
}
