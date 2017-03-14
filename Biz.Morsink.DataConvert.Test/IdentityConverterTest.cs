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
            _idConverter = new IdentityConverter();
            _converter = new DataConverter(new[] { _idConverter });
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
            Assert.IsTrue(_idConverter.CanConvert(typeof(bool), typeof(bool)));
            Assert.IsTrue(_idConverter.CanConvert(typeof(int), typeof(int)));
            Assert.IsTrue(_idConverter.CanConvert(typeof(string), typeof(string)));
            Assert.IsFalse(_idConverter.CanConvert(typeof(int), typeof(long)));
            Assert.IsFalse(_idConverter.CanConvert(typeof(string), typeof(DateTime)));
        }

        [TestMethod]
        public void Identity_Identities()
        {
            var iconv = _converter.GetConverter<int, int>();
            Assert.IsTrue(iconv(42).IsSuccessful);
            Assert.AreEqual(42, iconv(42).Result);
            var sconv = _converter.GetConverter<string, string>();
            Assert.IsTrue(sconv("hello").IsSuccessful);
            Assert.AreEqual("hello", sconv("hello").Result);
        }
        [TestMethod]
        public void Identity_Impossible()
        {
            var iconv = _converter.GetConverter<int, long>();
            Assert.IsFalse(iconv(42).IsSuccessful);

            var sconv = _converter.GetConverter<string, object>();
            Assert.IsFalse(sconv("hello").IsSuccessful);
        }
    }
}
