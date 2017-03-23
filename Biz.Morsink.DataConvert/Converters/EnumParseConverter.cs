using System;
using System.Linq;
using System.Reflection;
using Ex = System.Linq.Expressions.Expression;
using static Biz.Morsink.DataConvert.DataConvertUtils;
using System.Linq.Expressions;

namespace Biz.Morsink.DataConvert.Converters
{
    /// <summary>
    /// This converter uses the Enum.TryParse method to try to convert strings to enums. Works with flags as well.
    /// </summary>
    public class EnumParseConverter : IConverter
    {
        /// <summary>
        /// Gets a singleton case-sensitive EnumParseConverter 
        /// </summary>
        public static EnumParseConverter CaseSensitive { get; } = new EnumParseConverter(false);
        /// <summary>
        /// Gets a singleton case-insensitive EnumParseConverter 
        /// </summary>
        public static EnumParseConverter CaseInsensitive { get; } = new EnumParseConverter(true);
        /// <summary>
        /// Constructs the converter
        /// </summary>
        /// <param name="ignoreCase">Indicates whether casing should be ignored in parsing.</param>
        public EnumParseConverter(bool ignoreCase)
        {
            IgnoreCase = ignoreCase;
        }
        public bool CanConvert(Type from, Type to)
            => from == typeof(string) && to.GetTypeInfo().IsEnum;

        private static MethodInfo tryParse = (from m in typeof(Enum).GetTypeInfo().GetDeclaredMethods(nameof(Enum.TryParse))
                                              let ps = m.GetParameters()
                                              let gs = m.GetGenericArguments()
                                              where gs.Length == 1
                                              && ps.Length == 3
                                              && ps[0].ParameterType == typeof(string)
                                              && ps[1].ParameterType == typeof(bool)
                                              && ps[2].IsOut && ps[2].ParameterType == m.GetGenericArguments()[0].MakeByRefType()
                                              select m).Single();
        /// <summary>
        /// Gets a boolean indicating if casing should be ignored when parsing.
        /// </summary>
        public bool IgnoreCase { get; }

        private MethodInfo getTryParse(Type t)
            => tryParse.MakeGenericMethod(t);
        private Ex getParameter(ParameterInfo pi, ParameterExpression input, ParameterExpression output)
        {
            if (pi.ParameterType == typeof(string))
                return input;
            else if (pi.ParameterType == typeof(bool))
                return Ex.Constant(IgnoreCase);
            else if (pi.IsOut)
                return output;
            else
                throw new ArgumentException("Unknown parameter");
        }
        public Delegate Create(Type from, Type to)
        {
            var input = Ex.Parameter(from, "input");
            var result = Ex.Parameter(to, "result");
            var m = getTryParse(to);
            var block = Ex.Block(new[] { result },
                Ex.Condition(Ex.Call(m, m.GetParameters().Select(pi => getParameter(pi, input, result))),
                    Result(to, result),
                    NoResult(to)));
            return Ex.Lambda(block, input).Compile();
        }
    }
}
