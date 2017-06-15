using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Ex = System.Linq.Expressions.Expression;
using static Biz.Morsink.DataConvert.DataConvertUtils;
using System.Linq.Expressions;

namespace Biz.Morsink.DataConvert.Converters
{
    /// <summary>
    /// This converter uses the System.Convert class to do conversion. 
    /// It is a fallback mechanism if other methods of conversion fail. 
    /// </summary>
    public class DefaultConvertConverter : IConverter
    {
        public DefaultConvertConverter(IFormatProvider formatProvider = null)
        {
            FormatProvider = formatProvider;
        }
        /// <summary>
        /// Gets the default IFormatProvider used, when such a parameter is expected.
        /// </summary>
        public IFormatProvider FormatProvider { get; }

        public bool SupportsLambda => true;

        /// <summary>
        /// Finds the 'best' conversion method on the Convert class.
        /// </summary>
        /// <param name="from">The input type.</param>
        /// <param name="to">TRhe output type.</param>
        /// <returns></returns>
        public static MethodInfo ConvertMethod(Type from, Type to)
        {
            var q = from method in typeof(Convert).GetTypeInfo().DeclaredMethods
                    let parameters = method.GetParameters()
                    where method.IsPublic && method.IsStatic && method.ReturnType == to && method.Name.StartsWith("To") 
                        && (parameters.Length == 1 && parameters[0].ParameterType == @from
                           || parameters.Length == 2 && parameters[0].ParameterType == @from && parameters[1].ParameterType == typeof(IFormatProvider))
                    select method;
            return q.FirstOrDefault();
        }
        private Ex getParameter(ParameterInfo pi, ParameterExpression input)
        {
            if (pi.ParameterType == input.Type)
                return input;
            else if (pi.ParameterType == typeof(IFormatProvider))
                return Ex.Constant(FormatProvider);
            else
                throw new ArgumentException("Unknown paramter type");
        }

        public bool CanConvert(Type from, Type to)
            => ConvertMethod(from, to) != null;

        public LambdaExpression CreateLambda(Type from, Type to)
        {
            var input = Ex.Parameter(from, "input");
            var convert = ConvertMethod(from, to);
            var exception = Ex.Parameter(typeof(Exception), "exception");
            var block = Ex.TryCatch(
                Result(to, Ex.Call(convert, convert.GetParameters().Select(pi => getParameter(pi, input)))),
                Ex.Catch(exception, NoResult(to)));
            var lambda = Ex.Lambda(block, input);
            return lambda;
        }
        public Delegate Create(Type from, Type to)
            => CreateLambda(from, to).Compile();
    }
}
