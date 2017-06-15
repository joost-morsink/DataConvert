using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Biz.Morsink.DataConvert.Converters
{
    /// <summary>
    /// This converter uses the Base-64 encoding to convert back and forth between strings and byte arrays.
    /// </summary>
    public class Base64Converter : IConverter
    {
        /// <summary>
        /// Gets a singleton Base64Converter.
        /// </summary>
        public static Base64Converter Instance { get; } = new Base64Converter();

        public bool SupportsLambda => false;

        public bool CanConvert(Type from, Type to)
            => from == typeof(string) && to == typeof(byte[])
            || from == typeof(byte[]) && to == typeof(string);

        public Delegate Create(Type from, Type to)
        {
            if (from == typeof(string))
                return new Func<string, ConversionResult<byte[]>>(input =>
                 {
                     try
                     {
                         return new ConversionResult<byte[]>(Convert.FromBase64String(input));
                     }
                     catch
                     {
                         return default(ConversionResult<byte[]>);
                     }
                 });
            else
                return new Func<byte[], ConversionResult<string>>(input => new ConversionResult<string>(Convert.ToBase64String(input)));
        }

        public LambdaExpression CreateLambda(Type from, Type to)
            => null;
    }
}
