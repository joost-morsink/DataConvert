using System;
using System.Reflection;
using System.Linq.Expressions;
using Ex = System.Linq.Expressions.Expression;
using Et = System.Linq.Expressions.ExpressionType;
namespace Biz.Morsink.DataConvert.Converters
{
    using static Biz.Morsink.DataConvert.DataConvertUtils;
    /// <summary>
    /// Does a type check and delegates conversion back to the containing IDataConverter. 
    /// If the input value is null, conversion fails.
    /// </summary>
    public class DynamicConverter : IConverter, IDataConverterRef
    {
        public IDataConverter Ref { get; set; }

        public bool SupportsLambda => true;

        public bool CanConvert(Type from, Type to)
            => from == typeof(object);

        public LambdaExpression CreateLambda(Type from, Type to)
        {
            var input = Ex.Parameter(from, "input");
            var type = Ex.Parameter(typeof(Type), "type");

            var block = 
                Ex.Condition(
                    Ex.MakeBinary(Et.Equal, input, Ex.Default(typeof(object))),
                    NoResult(to),
                    Ex.Block(new[] { type },
                        Ex.Assign(type, Ex.Call(input, typeof(object).GetTypeInfo().GetDeclaredMethod(nameof(object.GetType)))),
                        Ex.Convert(Ex.Invoke(
                                    Ex.Call(Ex.Constant(Ref), nameof(IDataConverter.GetGeneralConverter), Type.EmptyTypes, type, Ex.Constant(to)),
                                    input),
                                typeof(ConversionResult<>).MakeGenericType(to))));
            
            var lambda = Ex.Lambda(block, input);
            return lambda;
        }
        public Delegate Create(Type from, Type to)
            => CreateLambda(from, to).Compile();
    }
}
