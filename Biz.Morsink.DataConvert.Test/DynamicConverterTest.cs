using Biz.Morsink.DataConvert.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Assert.AreEqual(42, _converter.Convert<object>(42).To<int>());
            Assert.AreEqual(42m, _converter.Convert<object>(42m).To<decimal>());
            var res = _converter.DoConversion<object, int>(0);
            Assert.IsTrue(res.IsSuccessful);
            Assert.AreEqual(0, res.Result);
        }
            
        [TestMethod]
        public void Dynamic_Unhappy()
        {
            Assert.AreEqual(0, _converter.Convert(42L).To<int>());
            Assert.IsFalse(_converter.DoConversion<object, int>(null).IsSuccessful);
        }
    }
}
