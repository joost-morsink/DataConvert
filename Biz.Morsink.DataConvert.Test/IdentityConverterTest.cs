using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Biz.Morsink.DataConvert.Converters;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class IdentityConverterTest
    {
        private IdentityConverter _idConverter;
        private IDataConverter _converter;
        [TestInitialize]
        public void Init()
        {
            _idConverter = IdentityConverter.Instance;
            _converter = new DataConverter(_idConverter);
        }
        [TestCleanup]
        public void Exit()
        {
            _idConverter = null;
            _converter = null;
        }
        [TestMethod]
        public void Identity_CanConvert()
        {
            Assert.IsTrue(_idConverter.CanConvert(typeof(bool), typeof(bool)), "IdentityConverter should be able to convert bool to bool");
            Assert.IsTrue(_idConverter.CanConvert(typeof(int), typeof(int)), "IdentityConverter should be able to convert int to int");
            Assert.IsTrue(_idConverter.CanConvert(typeof(string), typeof(string)), "IdentityConverter should be able to convert string to string");
            Assert.IsFalse(_idConverter.CanConvert(typeof(int), typeof(long)), "IdentityConverter should not be able to convert int to long");
            Assert.IsFalse(_idConverter.CanConvert(typeof(string), typeof(DateTime)), "IdentityConverter should be not able to convert string to DateTime");
        }

        [TestMethod]
        public void Identity_Identities()
        {
            var iconv = _converter.GetConverter<int, int>();
            Assert.IsTrue(iconv(42).IsSuccessful, "IdentityConverter should convert an int to itself");
            Assert.AreEqual(42, iconv(42).Result, "IdentityConverter should preserve the value of an int when converted to itself");
            var sconv = _converter.GetConverter<string, string>();
            Assert.IsTrue(sconv("hello").IsSuccessful, "IdentityConverter should convert a string to itself");
            Assert.AreEqual("hello", sconv("hello").Result, "IdentityConverter should preserve the value of a string when converted to itself");

            Assert.IsTrue(_converter.Convert(new Dummy()).TryTo(out Dummy result));
        }
        [TestMethod]
        public void Identity_Impossible()
        {
            var iconv = _converter.GetConverter<int, long>();
            Assert.IsFalse(iconv(42).IsSuccessful, "IdentityConverter should fail on converting int to long");

            var sconv = _converter.GetConverter<string, object>();
            Assert.IsFalse(sconv("hello").IsSuccessful, "IdentityConverter should fail on converting string to object");
        }
    }
    public class Dummy { }
}
