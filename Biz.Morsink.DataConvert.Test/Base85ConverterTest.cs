using Biz.Morsink.DataConvert.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class Base85ConverterTest
    {
        private DataConverter converter;
        [TestInitialize]
        public void Init()
        {
            converter = new DataConverter(Base85Converter.Default);
        }
        [TestMethod]
        public void Base85_Happy()
        {
            Assert.AreEqual("!!*-'\"9eu7!92", converter.Convert(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }).To<string>(), "Byte array should convert to Base64 string");
            var res = converter.Convert("!!*-'\"9eu7!92").To<byte[]>();
            Assert.AreEqual(10, res.Length, "Base85 string should convert to byte array (array length)");
            for (int i = 0; i < 10; i++)
                Assert.AreEqual(i, res[i], "Base85 string should convert to byte array (array content)");
        }
    }
}
