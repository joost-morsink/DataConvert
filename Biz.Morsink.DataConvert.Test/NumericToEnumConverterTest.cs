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
            Assert.AreEqual(TestEnum.Ghi, converter.Convert(3).To<TestEnum>());
            Assert.AreEqual(TestFlags.Two | TestFlags.Three | TestFlags.Five, converter.Convert((short)22).To<TestFlags>());
        }
        [TestMethod]
        public void NumToEnum_UnhappyRegular()
        {
            Assert.IsFalse(converter.DoConversion<int, TestEnum>(5).IsSuccessful);
            Assert.IsFalse(converter.DoConversion<int, TestEnum>(0).IsSuccessful);
        }
        [TestMethod]
        public void NumToEnum_UnhappyFlags()
        {
            Assert.IsFalse(converter.DoConversion<int, TestFlags>(-1).IsSuccessful);
            Assert.IsFalse(converter.DoConversion<int, TestFlags>(32).IsSuccessful);
            Assert.IsFalse(converter.DoConversion<int, TestFlags>(999).IsSuccessful);

        }
    }
}
