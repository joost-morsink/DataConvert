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
    public class NumericToEnumConverterTest
    {
        public enum TestEnum { Abc = 1, Def = 2, Ghi = 3 , Jkl = 4}
        [Flags]
        public enum TestFlags { One = 1, Two = 2, Three = 4, Four = 8, Five = 16 }
        private DataConverter converter;

        [TestInitialize]
        public void Init()
        {
            converter = new DataConverter(SimpleNumericConverter.Instance, new NumericToEnumConverter());

        }
        [TestMethod]
        public void NumToEnum_Happy()
        {
            Assert.AreEqual(TestEnum.Ghi, converter.Convert(3).To<TestEnum>(), "Int value should convert to enum value");
            Assert.AreEqual(TestFlags.Two | TestFlags.Three | TestFlags.Five, converter.Convert((short)22).To<TestFlags>(), "Short value should convert to flags enum value");
        }
        [TestMethod]
        public void NumToEnum_UnhappyRegular()
        {
            Assert.IsFalse(converter.DoConversion<int, TestEnum>(5).IsSuccessful, "Conversion to enum should fail if value is too high");
            Assert.IsFalse(converter.DoConversion<int, TestEnum>(0).IsSuccessful, "Conversion to enum should fail if value is too low");
        }
        [TestMethod]
        public void NumToEnum_UnhappyFlags()
        {
            Assert.IsFalse(converter.DoConversion<int, TestFlags>(-1).IsSuccessful, "Conversion to flags enum should fail if there are more bits set than flags are present");
            Assert.IsFalse(converter.DoConversion<int, TestFlags>(32).IsSuccessful," Conversion to flags enum should fail if some set bit cannot be mapped to a flag");
            Assert.IsFalse(converter.DoConversion<int, TestFlags>(999).IsSuccessful,"Conversion to flags enum should fail for combinations of set flags and set unknown bits");
        }
    }
}
