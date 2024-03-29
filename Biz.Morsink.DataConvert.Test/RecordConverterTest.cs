﻿using Biz.Morsink.DataConvert.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class RecordConverterTest
    {
        private DataConverter converter;

        private class PersonC
        {
            public PersonC(string firstName, string lastName, int age = -1)
            {
                FirstName = firstName;
                LastName = lastName;
                Age = age;
            }
            public string FirstName { get; }
            public string LastName { get; }
            public int Age { get; }
        }
        public class PersonS
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public int Age { get; set; }
        }
        public class Unit
        {
            public static Unit Instance { get; } = new Unit();
        }
        [TestInitialize]
        public void Init()
        {
            converter = new DataConverter(
                IdentityConverter.Instance,
                new TryParseConverter(),
                new ToStringConverter(true),
                RecordConverter.ForReadOnlyDictionaries(),
                RecordConverter.ForDictionaries(),
                new DynamicConverter());
        }
        [TestMethod]
        public void Rec_HappyConstructor()
        {
            var d = new Dictionary<string, object>
            {
                ["FirstName"] = "Joost",
                ["LastName"] = "Morsink",
                ["age"] = "37"
            };
            Assert.IsTrue(converter.Convert(d).TryTo(out PersonC p), "Conforming dictionary should be convertible using parameterized constructor.");
            Assert.AreEqual("Joost", p.FirstName, "Conforming dictionary should be preserve value of FirstName");
            Assert.AreEqual("Morsink", p.LastName, "Conforming dictionary should be preserve value of LastName");
            Assert.AreEqual(37, p.Age, "Conforming dictionary should convert the value of Age");
        }
        [TestMethod]
        public void Rec_HappySetter()
        {
            var d = new Dictionary<string, string>
            {
                ["FirstName"] = "Joost",
                ["LastName"] = "Morsink",
                ["age"] = "37"
            };

            Assert.IsTrue(converter.Convert(d).TryTo(out PersonS p), "Conforming dictionary should be convertible using setters");
            Assert.AreEqual("Joost", p.FirstName, "Conforming dictionary should be preserve value of FirstName");
            Assert.AreEqual("Morsink", p.LastName, "Conforming dictionary should be preserve value of LastName");
            Assert.AreEqual(37, p.Age, "Conforming dictionary should convert the value of Age");
        }
        [TestMethod]
        public void Rec_HappyToDict()
        {
            var p = new PersonC("Joost", "Morsink", 37);
            Assert.IsTrue(converter.Convert(p).TryTo(out Dictionary<string, string> d), "An object should be convertible to a dictionary");
            Assert.AreEqual(3, d.Count, "Number of readable properties should equal the number of entries in the dictionary");
            Assert.AreEqual(p.FirstName, d["FirstName"], "Property value FirstName should be preserved in dictionary");
            Assert.AreEqual(p.LastName, d["LastName"], "Property value LastName should be preserved in dictionary");
            Assert.AreEqual(p.Age.ToString(), d["Age"], "Property value Age should be converted in dictionary");
        }
        [TestMethod]
        public void Rec_UnhappyValue()
        {
            var d = new Dictionary<string, string>
            {
                ["FirstName"] = "Joost",
                ["LastName"] = "Morsink",
                ["Age"] = "37x"
            };
            Assert.IsFalse(converter.Convert(d).TryTo(out PersonC p), "Unparseable int should fail entire conversion for parameterized constructor types");
            Assert.IsFalse(converter.Convert(d).TryTo(out PersonS q), "Unparseable int should fail entire conversion for property setter types");
        }
        [TestMethod]
        public void Rec_HappyKey()
        {
            var d = new Dictionary<string, string>
            {
                ["FirstName"] = "Joost",
                ["LastNames"] = "Morsink",
                ["Age"] = "37"
            };
            Assert.IsTrue(converter.Convert(d).TryTo(out PersonS q), "Non-existent key should be ignored for property setter types");
        }
        [TestMethod]
        public void Rec_UnhappyKey()
        {
            var d = new Dictionary<string, string>
            {
                ["FirstName"] = "Joost",
                ["LastNames"] = "Morsink",
                ["Age"] = "37"
            };
            Assert.IsFalse(converter.Convert(d).TryTo(out PersonC p), "Non-existent key should fail entire conversion for parameterized constructor types");
        }
        [TestMethod]
        public void Rec_DefaultParam()
        {
            var d = new Dictionary<string, string>
            {
                ["FirstName"] = "Joost",
                ["LastName"] = "Morsink"
            };
            Assert.IsTrue(converter.Convert(d).TryTo(out PersonC p));
            Assert.AreEqual("Joost", p.FirstName);
            Assert.AreEqual("Morsink", p.LastName);
            Assert.AreEqual(-1, p.Age);
        }
        [TestMethod]
        public void Rec_ReadonlyDict()
        {
            var dict = ImmutableDictionary<string, string>.Empty.Add("FirstName", "Joost").Add("LastName", "Morsink").Add("Age", "37");
            Assert.IsTrue(converter.Convert(dict).TryTo(out PersonC p));
            Assert.AreEqual("Joost", p.FirstName);
            Assert.AreEqual("Morsink", p.LastName);
            Assert.AreEqual(37, p.Age);
            Assert.IsFalse(converter.Convert(p).TryTo(out ImmutableDictionary<string, string> _));
            Assert.IsFalse(converter.Convert(p).TryTo(out IReadOnlyDictionary<string, string> _));
        }
        [TestMethod]
        public void Rec_StaticUnhappy()
        {
            Assert.IsFalse(RecordConverter.ForDictionaries().CanConvert(typeof(object), typeof(Dictionary<string, string>)));
            Assert.IsFalse(RecordConverter.ForDictionaries().CanConvert(typeof(Dictionary<string, string>), typeof(object)));
        }
        [TestMethod]
        public void Rec_HappyEmpty()
        {
            Assert.IsTrue(converter.Convert(new Dictionary<string, string>()).Result<Unit>().IsSuccessful);
        }
        [TestMethod]
        public void Rec_HappyExpando()
        {
            converter = new DataConverter(
               IdentityConverter.Instance,
               new TryParseConverter(),
               new ToStringConverter(true),
               RecordConverter.ForReadOnlyDictionaries(),
               RecordConverter.ForDictionaries(),
               ToObjectConverter.Instance,
               new DynamicConverter());

            var e = new ExpandoObject();
            dynamic d = e;
            d.FirstName = "Joost";
            d.LastName = "Morsink";
            d.Age = 38;
            Assert.IsTrue(converter.Convert(e).TryTo(out PersonS p));
            Assert.AreEqual("Joost", p.FirstName);
            Assert.AreEqual("Morsink", p.LastName);
            Assert.AreEqual(38, p.Age);
            Assert.IsTrue(converter.Convert(p).TryTo(out e));
            d = e;
            Assert.AreEqual("Joost", d.FirstName);
            Assert.AreEqual("Morsink", d.LastName);
            Assert.AreEqual(38, d.Age);
        }
    }
}
