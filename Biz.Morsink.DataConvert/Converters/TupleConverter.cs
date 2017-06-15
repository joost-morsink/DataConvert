using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Ex = System.Linq.Expressions.Expression;
using Et = System.Linq.Expressions.ExpressionType;
using static Biz.Morsink.DataConvert.DataConvertUtils;
using static Biz.Morsink.DataConvert.Helpers.Tuples;
using System.Linq.Expressions;
using Biz.Morsink.DataConvert.Helpers;

namespace Biz.Morsink.DataConvert.Converters
{
    /// <summary>
    /// This component takes care of the conversion of tuples by individually converting the component values.
    /// </summary>
    public class TupleConverter : IConverter, IDataConverterRef
    {
        /// <summary>
        /// Gets a TupleConverter singleton.
        /// </summary>
        public static TupleConverter Instance { get; } = new TupleConverter();

        public IDataConverter Ref { get; set; }

        public bool SupportsLambda => true;

        public bool CanConvert(Type from, Type to)
            => TupleArity(from) >= 0 && TupleArity(from) == TupleArity(to);

        public LambdaExpression CreateLambda(Type from, Type to)
        {
            var fromParameters = from.GetTypeInfo().GenericTypeArguments;
            var toParameters = to.GetTypeInfo().GenericTypeArguments;

            var converters = fromParameters
                .Zip(toParameters, (f, t) => Ref.GetLambda(f, t))
                .ToArray();
            var input = Ex.Parameter(from, "input");

            var res = toParameters.Select(t => Ex.Parameter(typeof(ConversionResult<>).MakeGenericType(t))).ToArray();

            var conversion = res.Select((r, i) =>
                    Ex.Assign(res[i], converters[i].ApplyTo(Ex.PropertyOrField(input, $"Item{i + 1}")))).ToArray();
            var conversionSuccesful = Enumerable.Aggregate(res, (Ex)Ex.Constant(true),
                            (c, p) => Ex.MakeBinary(Et.AndAlso, c, Ex.Property(p, nameof(IConversionResult.IsSuccessful))));

            var block = Ex.Block(res,
                    Ex.Block(conversion),
                    Ex.Condition(conversionSuccesful,
                        Result(to,
                            Ex.Call(Creator(to), Enumerable.Select(res, p => Ex.Property(p, nameof(IConversionResult.Result))))),
                        NoResult(to)));
            var lambda = Ex.Lambda(block, input);
            return lambda;
        }

        public Delegate Create(Type from, Type to)
            => CreateLambda(from, to).Compile();

    }
}
