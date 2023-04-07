using System;
using System.Reflection;
using Ex = System.Linq.Expressions.Expression;
using static Biz.Morsink.DataConvert.DataConvertUtils;
using System.Linq.Expressions;

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

        public bool SupportsLambda => true;

        public bool CanConvert(Type from, Type to)
            => from.GetTypeInfo().IsEnum
            && (to == typeof(int) || to == typeof(long));

        public LambdaExpression CreateLambda(Type from, Type to)
        {
            var input = Ex.Parameter(from, "input");
            var block = Result(to, Ex.Convert(input, to));
            var lambda = Ex.Lambda(block, input);
            return lambda;
        }

        public Delegate Create(Type from, Type to)
            => CreateLambda(from, to).Compile();
    }
}
