using System;
using Ex = System.Linq.Expressions.Expression;

namespace Biz.Morsink.DataConvert.Converters
{
    using System.Linq.Expressions;
    using static DataConvertUtils;
    /// <summary>
    /// This is the trivial converter: 
    /// if the types for source an destination types are the same, the converter should just return the input.
    /// </summary>
    public class IdentityConverter : IConverter
    {
        /// <summary>
        /// Gets a singleton IdentityConverter.
        /// </summary>
        public static IdentityConverter Instance { get; } = new IdentityConverter();

        public bool SupportsLambda => true;

        public bool CanConvert(Type from, Type to)
            => from == to;

        public LambdaExpression CreateLambda(Type from, Type to)
        {
            var input = Ex.Parameter(from, "input");
            var block = Result(to, input);
            return Ex.Lambda(block, input);
        }

        public Delegate Create(Type from, Type to)
            => CreateLambda(from, to).Compile();
    }
}
