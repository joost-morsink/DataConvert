using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Ex = System.Linq.Expressions.Expression;
using static Biz.Morsink.DataConvert.DataConvertUtils;
namespace Biz.Morsink.DataConvert.Converters
{
    /// <summary>
    /// This converter converts a enum value to int or long.
    /// </summary>
    public class EnumToNumericConverter : IConverter
    {
        /// <summary>
        /// Gets an EnumToNumericConverter singleton.
        /// </summary>
        public static EnumToNumericConverter Instance { get; } = new EnumToNumericConverter();

        public bool CanConvert(Type from, Type to)
            => from.GetTypeInfo().IsEnum
            && (to == typeof(int) || to == typeof(long));

        public Delegate Create(Type from, Type to)
        {
            var input = Ex.Parameter(from, "input");
            var block = Result(to, Ex.Convert(input, to));
            var lambda = Ex.Lambda(block, input);
            return lambda.Compile();
        }
    }
}
