using Biz.Morsink.DataConvert.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class EnumToNumericConverterTest
    {
        private DataConverter converter;

        public enum TestEnum { Abc = 1, Def = 2, Ghi = 3 }
        [TestInitialize]
        public void Init()
        {
            converter = new DataConverter(EnumToNumericConverter.Instance);
        }

        [TestMethod]
        public void EnumToNum_Happy()
        {
            Assert.AreEqual(1, converter.Convert(TestEnum.Abc).To<int>(), "Enum should convert to int");
            Assert.AreEqual(2L, converter.Convert(TestEnum.Def).To<long>(), "Enum should convert to long");
        }
    }
}
