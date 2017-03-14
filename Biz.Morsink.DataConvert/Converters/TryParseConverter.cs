using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Ex = System.Linq.Expressions.Expression;

namespace Biz.Morsink.DataConvert.Converters
{
    using static Biz.Morsink.DataConvert.DataConvertUtils;
    /// <summary>
    /// Converts a string to some type using a TryParse static method on the result type.
    /// </summary>
    public class TryParseConverter : IConverter
    {
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
                    select m;
            return q.FirstOrDefault();
        }
        public bool CanConvert(Type from, Type to)
            => from == typeof(string) && GetMethod(to) != null;

        public Delegate Create(Type from, Type to)
        {
            
            var input = Ex.Parameter(from, "input");
            var result = Ex.Parameter(to, "result");
            var block = Ex.Block(new[] { result },
                Ex.Condition(Ex.Call(GetMethod(to), input, result),
                    Result(to, result),
                    NoResult(to)));

            return Ex.Lambda(block, input).Compile();
        }
    }
}
