using System;
using System.Globalization;
using System.Linq.Expressions;

namespace Biz.Morsink.DataConvert.Converters
{
    /// <summary>
    /// This class handles conversion between strings and DateTime/DateTimeOffset values.
    /// </summary>
    public class IsoDateTimeConverter : IConverter
    {
        /// <summary>
        /// Gets a singleton IsoDateTimeConverter.
        /// </summary>
        public static IsoDateTimeConverter Instance { get; } = new IsoDateTimeConverter();

        public bool SupportsLambda => false;

        public static ConversionResult<string> ToIsoDate(DateTime datetime)
            => new ConversionResult<string>(datetime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
        public static ConversionResult<string> ToIsoDateOffset(DateTimeOffset datetime)
            => new ConversionResult<string>(datetime.ToString("yyyy-MM-ddTHH:mm:ss.fffK"));

        public static ConversionResult<DateTime> FromIsoDate(string str)
        {
            if (DateTime.TryParse(str, null, DateTimeStyles.RoundtripKind, out var res))
                return new ConversionResult<DateTime>(res.ToUniversalTime());
            else
                return default(ConversionResult<DateTime>);
        }
        public static ConversionResult<DateTimeOffset> FromIsoDateOffset(string str)
        {
            if (DateTimeOffset.TryParse(str, null, DateTimeStyles.RoundtripKind, out var res))
                return new ConversionResult<DateTimeOffset>(res);
            else
                return default(ConversionResult<DateTimeOffset>);
        }

        public bool CanConvert(Type from, Type to)
            => from == typeof(string) && (to == typeof(DateTime) || to == typeof(DateTimeOffset))
            || to == typeof(string) && (from == typeof(DateTime) || from == typeof(DateTimeOffset));

        public Delegate Create(Type from, Type to)
        {
            if (from == typeof(string))
            {
                if (to == typeof(DateTime))
                    return new Func<string, ConversionResult<DateTime>>(FromIsoDate);
                else if (to == typeof(DateTimeOffset))
                    return new Func<string, ConversionResult<DateTimeOffset>>(FromIsoDateOffset);
            }
            else if (to == typeof(string))
            {
                if (from == typeof(DateTime))
                    return new Func<DateTime, ConversionResult<string>>(ToIsoDate);
                else if (from == typeof(DateTimeOffset))
                    return new Func<DateTimeOffset, ConversionResult<string>>(ToIsoDateOffset);
            }
            throw new ArgumentException("Cannot convert between types");
        }

        public LambdaExpression CreateLambda(Type from, Type to)
            => null;
    }
}
