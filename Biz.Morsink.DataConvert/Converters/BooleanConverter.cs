using System;
using System.Linq.Expressions;
using Ex = System.Linq.Expressions.Expression;
using static Biz.Morsink.DataConvert.DataConvertUtils;

namespace Biz.Morsink.DataConvert.Converters
{
    /// <summary>
    /// Converts between booleans and strings or the usual integral numeric types.
    /// </summary>
    public class BooleanConverter : IConverter
    {
        public static BooleanConverter Instance { get; } = new BooleanConverter();

        public bool SupportsLambda => true;

        private bool CompatibleType(Type t)
            => t == typeof(string) || t == typeof(int) || t == typeof(long) || t == typeof(short) || t == typeof(byte);

        public bool CanConvert(Type from, Type to)
            => from == typeof(bool) && CompatibleType(to)
            || CompatibleType(from) && to == typeof(bool);

        public Delegate Create(Type from, Type to)
            => CreateLambda(from, to)?.Compile();

        public LambdaExpression CreateLambda(Type from, Type to)
        {
            if (from == typeof(bool))
            {
                var input = Ex.Parameter(typeof(bool), "input");
                if (to == typeof(string))
                    return Ex.Lambda(Result(to, Ex.Call(input, nameof(bool.ToString), Type.EmptyTypes)), input);
                else
                    return Ex.Lambda(Result(to, Ex.Condition(input, Ex.Constant(Convert.ChangeType(1, to), to), Ex.Constant(Convert.ChangeType(0, to), to))), input);
            }
            else // to == typeof(bool)
            {
                var input = Ex.Parameter(from, "input");
                if (from == typeof(string))
                {
                    var i = Ex.Parameter(typeof(int), "i");
                    var body = Ex.Block(new[] { i },
                        Ex.Condition(
                            Ex.Call(typeof(string), nameof(string.Equals), Type.EmptyTypes,
                                input,
                                Ex.Constant("true"),
                                Ex.Constant(StringComparison.OrdinalIgnoreCase)),
                            Result(to, Ex.Constant(true)),
                            Ex.Condition(
                                Ex.Call(typeof(string), nameof(string.Equals), Type.EmptyTypes,
                                    input,
                                    Ex.Constant("false"),
                                    Ex.Constant(StringComparison.OrdinalIgnoreCase)),
                                Result(to, Ex.Constant(false)),
                                Ex.Condition(Ex.Call(typeof(int), nameof(int.TryParse), Type.EmptyTypes, input, i),
                                    Result(to, Ex.MakeBinary(ExpressionType.NotEqual, i, Ex.Constant(0))),
                                    NoResult(to)))));
                    return Ex.Lambda(body, input);
                }
                else
                    return Ex.Lambda(Result(to,
                            Ex.MakeBinary(ExpressionType.NotEqual, input, Ex.Constant(Convert.ChangeType(0, from), from))), input);

            }
        }
    }
}
