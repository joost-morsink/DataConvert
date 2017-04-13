using Biz.Morsink.DataConvert.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class EnumerableToTupleConverterTest 
    {
        class TestEnumerable
        {
            public struct Enumerator
            {
                private int x;
                public bool MoveNext()
                {
                    return ++x <= 3;
                }
                public int Current => x;
            }
            public Enumerator GetEnumerator() => new Enumerator();
        }
        class TestCollection : IReadOnlyCollection<int>
        {
            public int Count => 3;

            public IEnumerator<int> GetEnumerator()
            {
                yield return 1;
                yield return 2;
                yield return 3;
            }

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();
        }
        private DataConverter converter;

        [TestInitialize]
        public void Init()
        {
            converter = new DataConverter(IdentityConverter.Instance, EnumerableToTupleConverter.Instance, new DynamicConverter());

        }
        [TestMethod]
        public void Enumerable2Tuple_Happy()
        {
            Assert.AreEqual(Tuple.Create(1, 2), converter.Convert(new int[] { 1, 2 }).To<Tuple<int, int>>(), "Int array should convert to int Tuple");
            Assert.AreEqual(Tuple.Create(1, 2, 3), converter.Convert(new TestEnumerable()).To<Tuple<int, int, int>>(), "Foreachable class should convert to Tuple");
            Assert.AreEqual((1, 2, 3), converter.Convert(new TestCollection()).To<(int, int, int)>(), "IReadOnlyCollection should convert to Tuple");
            Assert.AreEqual((1, 2, 3), converter.Convert<IReadOnlyCollection<int>>(new TestCollection()).To<(int, int, int)>(), "Statically typed IReadOnlyCollection should convert to Tuple");
        }
        [TestMethod]
        public void Enumerable2Tuple_Unhappy()
        {
            Assert.IsFalse(converter.DoConversion<int[], Tuple<int, string>>(new[] { 1, 2 }).IsSuccessful, "EnumerableToTupleConverter should not do component conversions by itself");
            Assert.IsFalse(converter.DoConversion<int[], Tuple<int, int, int>>(new[] { 1, 2 }).IsSuccessful, "Conversion of enumerables that are too short should fail");
        }
        
    }
}
