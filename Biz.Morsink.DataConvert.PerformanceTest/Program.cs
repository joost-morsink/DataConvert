using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Biz.Morsink.DataConvert.PerformanceTest
{
    static class Program
    {
        static void ToConsole(this object o)
            => Console.WriteLine(o);
        static void Main(string[] args)
        {
            if (args.Length == 0 || !double.TryParse(args[0], out double secs))
                secs = 1.0;
            Console.WriteLine($"Loop overhead: {Measure.LOOP_AND_CALL_OVERHEAD}");
            var converter = DataConverter.Default;
            foreach (var m in from mi in typeof(Program).GetTypeInfo().DeclaredMethods
                              where mi.IsStatic
                              let ps = mi.GetParameters()
                              where ps.Length == 1 && ps[0].ParameterType == typeof(IDataConverter) && mi.ReturnType == typeof(Measure)
                              select (Func<IDataConverter, Measure>)mi.CreateDelegate(typeof(Func<IDataConverter, Measure>)))
                m(converter).TimeSpan(TimeSpan.FromSeconds(secs)).Execute().ToConsole();
        }
        private static Measure StringToInt(IDataConverter converter)
        {
            var c = converter.GetConverter<string, int>();
            var ic = CultureInfo.InvariantCulture;
            return Measure.Create("string to int conversion")
                .ReferenceAction(() => { var _ = Convert.ToInt32("42", ic); })
                .MeasurementAction(() => { var _ = c("42").Result; });
        }
        private static Measure StringToNullableInt(IDataConverter converter)
        {
            var c = converter.GetConverter<string, int?>();
            var ic = CultureInfo.InvariantCulture;
            return Measure.Create("string to int? conversion")
                .ReferenceAction(() => { var _ = (int?)Convert.ToInt32("42"); }) 
                .MeasurementAction(() => { var _ = c("42").Result; });
        }
        private static Measure DynamicStringToInt(IDataConverter converter)
        {
            var c = converter.GetConverter<object, int>();
            var ic = CultureInfo.InvariantCulture;
            return Measure.Create("dynamic string to int conversion")
                .ReferenceAction(() => { var _ = Convert.ToInt32((object)"42", ic); })
                .MeasurementAction(() => { var _ = c("42").Result; });
        }
        private static Measure SimplePrimitiveNumeric(IDataConverter converter)
        {
            var c = converter.GetConverter<int, short>();
            return Measure.Create("simple primitive numeric conversion")
                .ReferenceAction(() => { var _ = Convert.ToInt16(42); })
                .MeasurementAction(() => { var _ = c(42).Result; });
        }
        private static Measure ComplexPrimitiveNumeric(IDataConverter converter)
        {
            var c = converter.GetConverter<double, decimal>();
            return Measure.Create("complex primitive numeric conversion")
                .ReferenceAction(() => { var _ = Convert.ToDecimal(3.14159); })
                .MeasurementAction(() => { var _ = c(3.14159).Result; });
        }
        private static Measure DynamicPrimitiveNumeric(IDataConverter converter)
        {
            var c = converter.GetConverter<object, short>();
            return Measure.Create("dynamic primitive numeric conversion")
                .ReferenceAction(() => { var _ = Convert.ToInt16((object)42); })
                .MeasurementAction(() => { var _ = c(42).Result; });
        }
        private static Measure IntToString(IDataConverter converter)
        {
            var c = converter.GetConverter<int, string>();
            var ic = CultureInfo.InvariantCulture;
            return Measure.Create("int to string conversion")
                .ReferenceAction(() => { var _ = 42.ToString(ic); })
                .MeasurementAction(() => { var _ = c(42).Result; });
        }
        private static Measure ParseDate(IDataConverter converter)
        {
            var c = converter.GetConverter<string, DateTime>();
            return Measure.Create("DateTime to string conversion")
                .ReferenceAction(() => { var _ = DateTime.TryParse("2017-04-20T08:51:00Z", null, DateTimeStyles.RoundtripKind, out var res); })
                .MeasurementAction(() => { var _ = c("2017-04-20T08:51:00Z").Result; });
        }
        private static Measure ParseFailure(IDataConverter converter)
        {
            return Measure.Create("failure")
                .ReferenceAction(() =>
                {
                    try
                    {
                        var _ = Convert.ToInt32("X");
                    }
                    catch { }
                })
                .MeasurementAction(() => converter.Convert("X").To<int>());
        }
    }
}