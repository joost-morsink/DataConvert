using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Ex = System.Linq.Expressions.Expression;
using static Biz.Morsink.DataConvert.DataConvertUtils;
using static Biz.Morsink.DataConvert.Helpers.Tuples;
namespace Biz.Morsink.DataConvert.Converters
{
    /// <summary>
    /// This component converts a tuple into an array of some specified basetype.
    /// </summary>
    public class TupleToArrayConverter : IConverter, IDataConverterRef
    {
        /// <summary>
        /// Gets a TupleToArrayConverter singleton.
        /// </summary>
        public static TupleToArrayConverter Instance { get; } = new TupleToArrayConverter();
        public IDataConverter Ref { get; set; }

        public bool CanConvert(Type from, Type to)
            => TupleArity(from) >= 0 && typeof(Array).GetTypeInfo().IsAssignableFrom(to.GetTypeInfo());
            
        public Delegate Create(Type from, Type to)
        {
            var input = Ex.Parameter(from, "input");
            var eType = to.GetElementType();
            var res = Ex.Parameter(typeof(ConversionResult<>).MakeGenericType(eType).MakeArrayType(), "res");
            var end = Ex.Label(typeof(ConversionResult<>).MakeGenericType(to),"end");
            var fromParameters = from.GetTypeInfo().GenericTypeArguments;
            var converters = fromParameters.Select(t => Ref.GetConverter(t, eType)).ToArray();

            var block = Ex.Block(new[] { res },
                Ex.Assign(res, Ex.NewArrayBounds(typeof(ConversionResult<>).MakeGenericType(eType), Ex.Constant(fromParameters.Length))),
                Ex.Block(converters.Select((con, i) =>
                    Ex.Block(
                        Ex.Assign(Ex.ArrayAccess(res, Ex.Constant(i)),
                            Ex.Invoke(
                                Ex.Constant(con, typeof(Func<,>).MakeGenericType(con.GetMethodInfo().GetParameters()[0].ParameterType, typeof(ConversionResult<>).MakeGenericType(eType))),
                                Ex.PropertyOrField(input, $"Item{i + 1}"))),
                        Ex.IfThen(Ex.Not(Ex.Property(Ex.ArrayIndex(res, Ex.Constant(i)), nameof(IConversionResult.IsSuccessful))),
                            Ex.Goto(end, NoResult(to)))))),
                Ex.Label(end, Result(to, 
                        Ex.NewArrayInit(eType, 
                        Enumerable.Range(0,fromParameters.Length)
                            .Select(idx => Ex.Property(Ex.ArrayIndex(res,Ex.Constant(idx)), nameof(IConversionResult.Result)))))));
            var lambda = Ex.Lambda(block, input);
            return lambda.Compile();
        }
    }
}
