using Biz.Morsink.DataConvert.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class Base64IntegerConverterTest 
    {
        private DataConverter converter;

        [TestInitialize]
        public void Init()
        {
            converter = new DataConverter(IdentityConverter.Instance, new Base64IntegerConverter());
        }

        [TestMethod]
        public void Base64Int_Happy()
        {
            var f = converter.GetConverter<int, string>();
            var t = converter.GetConverter<string, int>();
            foreach(var i in Enumerable.Range(0,1000).Concat(Enumerable.Range(1000000,1000)).Concat(Enumerable.Range(-1001,1000)))
            {
                var str = f(i);
                Assert.IsTrue(str.IsSuccessful, "Base 64 Conversion to string should always succeed.");
                var back = t(str.Result);
                Assert.IsTrue(back.IsSuccessful, "A generated base 64 string should be convertible back to the integral type it was generated from.");
                Assert.AreEqual(i, back.Result, "A generated base 64 string should convert back to the integral value it was generated from.");
            }
        }
        [TestMethod]
        public void Base64Int_HappyBigInt()
        {
            var f = converter.GetConverter<BigInteger, string>();
            var t = converter.GetConverter<string, BigInteger>();
            BigInteger num = 1000;
            for (int i = 0; i < 200; i++)
            {
                num <<= 1;
                var str = f(num);
                Assert.IsTrue(str.IsSuccessful, "Base 64 Conversion to string should always succeed. (BigInteger)");
                var back = t(str.Result);
                Assert.IsTrue(back.IsSuccessful, "A generated base 64 string should be convertible back to the integral type it was generated from. (BigInteger)");
                Assert.AreEqual(num, back.Result, "A generated base 64 string should convert back to the integral value it was generated from. (BigInteger)");
            }
        }

        [TestMethod]
        public void Base64Int_HappyUlong()
        {
            var f = converter.GetConverter<ulong, string>();
            var t = converter.GetConverter<string, ulong>();
            foreach (var i in Enumerable.Range(0, 1000).Concat(Enumerable.Range(1000000, 1000)).Concat(Enumerable.Range(-1001, 1000)))
            {
                ulong l = (ulong)i * 999999UL;
                var str = f(l);
                Assert.IsTrue(str.IsSuccessful, "Base 64 Conversion to string should always succeed. (ulong)");
                var back = t(str.Result);
                Assert.IsTrue(back.IsSuccessful, "A generated base 64 string should be convertible back to the integral type it was generated from. (ulong)");
                Assert.AreEqual(l, back.Result, "A generated base 64 string should convert back to the integral value it was generated from. (ulong)");
            }
        }
        [TestMethod]
        public void Base64Int_UnhappyConstruction()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Base64IntegerConverter("ABCDEFG".ToCharArray()), "Too short char arrays should be rejected.");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Base64IntegerConverter("ABCDEFGHIJKLMNOPQRSTUVWXYZäbcdëfghïjklmnöpqrstüvwxÿz0123456789+/".ToCharArray()), "Char arrays containing non 7 bit ASCII characters should be rejected.");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Base64IntegerConverter("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789++".ToCharArray()),"Char arrays containing duplicates should be rejected.");
        }
        [TestMethod]
        public void Base64Int_UnhappyParse()
        {
            var c = converter.GetConverter<string, int>();
            Assert.IsFalse(c("A$").IsSuccessful, "Strings containing unknown characters should fail.");
        }
    }
}
