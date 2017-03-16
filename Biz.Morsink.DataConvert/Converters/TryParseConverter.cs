using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Ex = System.Linq.Expressions.Expression;
using static Biz.Morsink.DataConvert.DataConvertUtils;

namespace Biz.Morsink.DataConvert.Converters
{
    /// <summary>
    /// Converts a string to some type using a TryParse static method on the result type.
    /// </summary>
    public class TryParseConverter : IConverter
    {
        public TryParseConverter(IFormatProvider formatProvider = null, NumberStyles numberStyles = NumberStyles.Float)
        {
            FormatProvider = formatProvider ?? CultureInfo.InvariantCulture;
            NumberStyles = numberStyles;
        }
        /// <summary>
        /// Gets the IFormatProvider instance to be used on TryParse methods.
        /// </summary>
        public IFormatProvider FormatProvider { get; }
        /// <summary>
        /// Gets the NumberStyles value to be used on TryParse methods.
        /// </summary>
        public NumberStyles NumberStyles { get; }
        /// <summary>
        /// Gets a 'TryParse' method. This method is selected using the following criteria:
        /// <list type="bullet">
        /// <item>A public static method called 'TryParse'</item>
        /// <item>First parameter a string</item>
        /// <item>Second parameter an out T, where T is the destination type</item>
        /// <item>Its return type is Boolean</item>
        /// </list>
        /// </summary>
        /// <param name="type">The type on which to find the TryParse method</param>
        /// <returns>The MethodInfo of the TryParse method if one can be found, null otherwise.</returns>
        public static MethodInfo GetMethod(Type type)
        {
            var q = from m in type.GetTypeInfo().DeclaredMethods
                    where m.IsStatic && m.IsPublic && m.ReturnType == typeof(bool)
                    let ps = m.GetParameters()
                    where ps.Length == 2 && ps[0].ParameterType == typeof(string)
                          && ps[1].IsOut && ps[1].ParameterType == type.MakeByRefType()
                      || ps.Length == 3 && ps[0].ParameterType == typeof(string)
                          && ps[1].ParameterType == typeof(IFormatProvider)
                          && ps[2].IsOut && ps[2].ParameterType == type.MakeByRefType()
                      || ps.Length == 4 && ps[0].ParameterType == typeof(string)
                          && ps[1].ParameterType == typeof(NumberStyles)
                          && ps[2].ParameterType == typeof(IFormatProvider)
                          && ps[3].IsOut && ps[3].ParameterType == type.MakeByRefType()
                    orderby ps.Length descending
                    select m;
            return q.FirstOrDefault();
        }
        private Ex getParameter(ParameterInfo pi, ParameterExpression input, ParameterExpression output)
        {
            if (pi.ParameterType == typeof(string))
                return input;
            else if (pi.ParameterType == typeof(NumberStyles))
                return Ex.Constant(NumberStyles);
            else if (pi.ParameterType == typeof(IFormatProvider))
                return Ex.Constant(FormatProvider);
            else if (pi.IsOut)
                return output;
            else
                throw new ArgumentException("Unknown parameter");
        }

        public bool CanConvert(Type from, Type to)
            => from == typeof(string) && GetMethod(to) != null;

        public Delegate Create(Type from, Type to)
        {
            var input = Ex.Parameter(from, "input");
            var result = Ex.Parameter(to, "result");
            var m = GetMethod(to);
            var block = Ex.Block(new[] { result },
                Ex.Condition(Ex.Call(m, m.GetParameters().Select(pi => getParameter(pi, input, result))),
                    Result(to, result),
                    NoResult(to)));

            return Ex.Lambda(block, input).Compile();
        }
    }
}
