using System;
using System.Linq.Expressions;
using Ex = System.Linq.Expressions.Expression;
using static Biz.Morsink.DataConvert.DataConvertUtils;

namespace Biz.Morsink.DataConvert.Converters
{
    /// <summary>
    /// Converts reference types to a generic object reference and boxes value types.
    /// </summary>
    public class ToObjectConverter : IConverter
    {
        public static ToObjectConverter Instance { get; } = new ToObjectConverter();

        public ToObjectConverter() { }

        public bool SupportsLambda => true;

        public bool CanConvert(Type from, Type to)
            => to == typeof(object);

        public Delegate Create(Type from, Type to)
            => CreateLambda(from, to).Compile();

        public LambdaExpression CreateLambda(Type from, Type to)
        {
            var input = Ex.Parameter(from, "input");
            return Ex.Lambda(Result(typeof(object),Ex.Convert(input, typeof(object))), input);
        }
    }
}
