using Biz.Morsink.DataConvert.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class DynamicConverterTest
    {
        private IDataConverter _converter;
        [TestInitialize]
        public void Init()
        {
            _converter = new DataConverter(
                IdentityConverter.Instance,
                new DynamicConverter());
        }
        [TestCleanup] 
        public void Exit()
        {
            _converter = null;
        }

        [TestMethod]
        public void Dynamic_Happy()
        {
            Assert.AreEqual(42, _converter.Convert<object>(42).To<int>(), "DynamicConverter should typecheck int and use identity converter");
            Assert.AreEqual(42m, _converter.Convert<object>(42m).To<decimal>(), "DynamicConverter should typecheck decimal and use identity converter");
            var res = _converter.DoConversion<object, int>(0);
            Assert.IsTrue(res.IsSuccessful, "DynamicConverter should typecheck int and use identity converter (default value, successful)");
            Assert.AreEqual(0, res.Result, "DynamicConverter should typecheck int and use identity converter (default value, value equal)");
        }
            
        [TestMethod]
        public void Dynamic_Unhappy()
        {
            Assert.AreEqual(0, _converter.Convert(42L).To<int>(), "DynamicConverter should typecheck int and fail because int is not long");
            Assert.IsFalse(_converter.DoConversion<object, int>(null).IsSuccessful, "DynamicConverter should fail on null");
        }
    }
}
