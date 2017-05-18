using Biz.Morsink.DataConvert.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.DataConvert
{
    /// <summary>
    /// Utility extension methods.
    /// </summary>
    public static class DataConverterExt
    {
        /// <summary>
        /// Gets a strongly typed converter function
        /// </summary>
        /// <typeparam name="T">The type to be converted from.</typeparam>
        /// <typeparam name="U">The type to be converted to.</typeparam>
        /// <param name="converter">The IDataConverter object that provides the non-generic converter.</param>
        /// <returns>A strongly typed converter function.</returns>
        public static Func<T, ConversionResult<U>> GetConverter<T, U>(this IDataConverter converter)
            => (Func<T, ConversionResult<U>>)converter.GetConverter(typeof(T), typeof(U));
        /// <summary>
        /// This method performs an actual conversion from a value of type T to type U.
        /// </summary>
        /// <typeparam name="T">The type to be converted from.</typeparam>
        /// <typeparam name="U">The type to be converted to.</typeparam>
        /// <param name="converter">The IDataConverter object.</param>
        /// <param name="value">The value to be converted.</param>
        /// <returns>The result of the conversion of the value to type U.</returns>
        public static ConversionResult<U> DoConversion<T, U>(this IDataConverter converter, T value)
            => converter.GetConverter<T, U>()(value);
        /// <summary>
        /// Gets the result from a ConversionResult. When it is successful it returns the converted value, otherwise some default value.
        /// </summary>
        /// <typeparam name="T">The type of the converted value.</typeparam>
        /// <param name="result">The conversion result.</param>
        /// <param name="default">A default that is returned when the conversion result is not successful.</param>
        /// <returns>The converted value, or a default value.</returns>
        public static T GetOrDefault<T>(this ConversionResult<T> result, T @default = default(T))
            => result.IsSuccessful ? result.Result : @default;
        /// <summary>
        /// This method performs a conversion and exposes it as a 'Try' method.
        /// </summary>
        /// <typeparam name="T">The type to be converted from.</typeparam>
        /// <typeparam name="U">The type to be converted to.</typeparam>
        /// <param name="converter">The IDataConverter object.</param>
        /// <param name="value">The value to be converted.</param>
        /// <param name="result">Out result parameter for the conversion result.</param>
        /// <returns>True if the conversion was successful.</returns>
        public static bool TryConvert<T, U>(this IDataConverter converter, T value, out U result)
        {
            var conversionResult = converter.DoConversion<T, U>(value);
            result = conversionResult.Result;
            return conversionResult.IsSuccessful;
        }
        /// <summary>
        /// A helper struct to support a 'fluent api' for conversion.
        /// </summary>
        /// <typeparam name="T">The type of the value to be converted.</typeparam>
        public struct Convertible<T>
        {
            private readonly IDataConverter _converter;
            private readonly T _value;

            /// <summary>
            /// Constructor for the Convertible&gt;T&lt; type.
            /// </summary>
            /// <param name="converter">The IDataConverter that will perform the conversion.</param>
            /// <param name="value">The value to be converted.</param>
            public Convertible(IDataConverter converter, T value)
            {
                _converter = converter;
                _value = value;
            }
            /// <summary>
            /// Converts the value to type U, or a default value if conversion fails.
            /// </summary>
            /// <typeparam name="U">The type to be converted to.</typeparam>
            /// <param name="default">A default value to return in case of failure.</param>
            /// <returns>The conversion result, or some default value.</returns>
            public U To<U>(U @default = default(U)) 
                => _converter.DoConversion<T, U>(_value).GetOrDefault(@default);
            /// <summary>
            /// Converts the value to type U, using the 'Try' method pattern.
            /// </summary>
            /// <typeparam name="U">The type to be converted to.</typeparam>
            /// <param name="result">Out parameter the result of the conversion will be written to.</param>
            /// <returns>True if the conversion is successful.</returns>
            public bool TryTo<U>(out U result)
                => _converter.TryConvert(_value, out result);
            /// <summary>
            /// Converts the value to a U, returns the ConversionResult.
            /// </summary>
            /// <typeparam name="U">The type to be converted to.</typeparam>
            /// <returns>The conversion result, or some default value.</returns>
            public ConversionResult<U> Result<U>()
                => _converter.DoConversion<T, U>(_value);
        }
        /// <summary>
        /// 'Fluent api' to indicate some value is to be converted.
        /// </summary>
        /// <typeparam name="T">The type of value to convert.</typeparam>
        /// <param name="converter">The IDataConverter that will perform the conversion.</param>
        /// <param name="value">The value to convert.</param>
        /// <returns>A placeholder struct that can take a conversion call.</returns>
        public static Convertible<T> Convert<T>(this IDataConverter converter, T value)
            => new Convertible<T>(converter, value);
        /// <summary>
        /// Does a conversion without strong typed generics
        /// </summary>
        /// <param name="converter">The converter to use for conversion</param>
        /// <param name="val">The value to be converted</param>
        /// <param name="destination">The type to be converted to</param>
        /// <returns></returns>
        public static IConversionResult DoGeneralConversion(this IDataConverter converter, object val, Type destination)
        {
            if (val == null)
                return (IConversionResult)Activator.CreateInstance(typeof(ConversionResult<>).MakeGenericType(destination));
            var src = val.GetType();
            return converter.GetGeneralConverter(src, destination)(val);
        }
        /// <summary>
        /// Restricts the type on which a converter can operate. 
        /// </summary>
        /// <param name="converter">The converter to restrict.</param>
        /// <param name="typeFilter">A type filter expressing the restriction.</param>
        /// <returns>A restricted converter.</returns>
        public static IConverter Restrict(this IConverter converter, Func<Type, Type, bool> typeFilter)
            => new RestrictTypesConverter(converter, typeFilter);
    }
}
