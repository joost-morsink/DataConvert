using Biz.Morsink.DataConvert.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class LosslessStringToTupleConverterTest
    {
        private DataConverter converter;

        [TestInitialize]
        public void Init()
        {
            converter = new DataConverter(IdentityConverter.Instance, new TryParseConverter(), new LosslessStringToTupleConverter('-'));
        }

        [TestMethod]
        public void LlStr2Tup_Happy()
        {
            var res = converter.Convert("1-xyz-12345").Result<(string, int)>();
            Assert.IsTrue(res.IsSuccessful, "Conversion of valid separated values should succeed");
            Assert.AreEqual(("1-xyz", 12345), res.Result, "Parts on the end should have precedence");

            var res2 = converter.Convert("1-2-3-4-5").Result<(string, int, int)>();
            Assert.IsTrue(res2.IsSuccessful, "Conversion of valid separated values should succeed");
            Assert.AreEqual(("1-2-3", 4, 5), res2.Result, "It should be possible to get multiple parts at the end.");
        }
        [TestMethod]
        public void LlStr2Tup_HappyTrivial()
        {
            var res = converter.Convert("1-2-3").Result<(int, int, int)>();
            Assert.IsTrue(res.IsSuccessful, "Separated int string should convert to integer tuple");
            Assert.AreEqual((1, 2, 3), res.Result, "Separated int string should convert to integer tuple");

        }
        [TestMethod]
        public void LlStr2Tup_UnhappyRef()
        {
            var res = converter.Convert("1-2-3").Result<(int, int)>();
            Assert.IsFalse(res.IsSuccessful, "If a part is not convertible the conversion should fail.");
        }
        [TestMethod]
        public void LlStr2Tup_UnhappySmall()
        {
            var res = converter.Convert("1-2-3").Result<(string, string, string, string)>();
            Assert.IsFalse(res.IsSuccessful, "If not enough parts are present the conversion should fail.");
        }
    }
}
