using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Biz.Morsink.DataConvert.Helpers.Tuples;
using static Biz.Morsink.DataConvert.DataConvertUtils;
using Ex = System.Linq.Expressions.Expression;
using Et = System.Linq.Expressions.ExpressionType;
using System.Reflection;
using System.Linq.Expressions;
using Biz.Morsink.DataConvert.Helpers;

namespace Biz.Morsink.DataConvert.Converters
{
    /// <summary>
    /// This component converts string to tuples, starting at the end of the string.
    /// It does not lose any information of the value during conversion.
    /// </summary>
    public class LosslessStringToTupleConverter : IConverter, IDataConverterRef
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="separator">The separator used to separate parts of the string.</param>
        public LosslessStringToTupleConverter(char separator) {
            Separator = separator;
            _separator = Ex.Constant(new[] { separator });
            _separatorString = Ex.Constant(separator.ToString());
        }
        public IDataConverter Ref { get; set; }
        /// <summary>
        /// The separator used to separate parts of the string.
        /// </summary>
        public char Separator { get; }

        public bool SupportsLambda => true;

        private readonly Ex _separator;
        private readonly Ex _separatorString;

        public bool CanConvert(Type from, Type to)
            => from == typeof(string) && TupleArity(to) > 1;

        public LambdaExpression CreateLambda(Type from, Type to)
        {
            var toParameters = to.GetTypeInfo().GenericTypeArguments;
            var tupa = toParameters.Length;
            var input = Ex.Parameter(from, "input");
            var converters = toParameters.Select(p => Ref.GetLambda(typeof(string), p)).ToArray();
            var res = toParameters.Select(p => Ex.Parameter(typeof(ConversionResult<>).MakeGenericType(p))).ToArray();
            var end = Ex.Label(typeof(ConversionResult<>).MakeGenericType(to), "end");
            var indexer = typeof(string[]).GetTypeInfo().GetDeclaredProperty("Item");

            var split = Ex.Parameter(typeof(string[]), "split");
            var conversion = Ex.Block(converters.Select((c, i) =>
                Ex.Block(
                    Ex.Assign(res[i],
                        c.ApplyTo(Ex.MakeIndex(split, indexer, new[] { Ex.MakeBinary(Et.Add, Ex.Constant(i), Ex.MakeBinary(Et.Subtract, Ex.Property(split, nameof(Array.Length)), Ex.Constant(tupa))) }))),
                    Ex.IfThen(Ex.Not(Ex.Property(res[i], nameof(IConversionResult.IsSuccessful))),
                        Ex.Goto(end, NoResult(to))))));
            var block = Ex.Block(new[] { split },
                Ex.Assign(split, Ex.Call(input, nameof(string.Split), Type.EmptyTypes, _separator)),
                Ex.Condition(Ex.MakeBinary(Et.LessThan, Ex.Property(split, nameof(Array.Length)), Ex.Constant(tupa)),
                    NoResult(to),
                    Ex.Block(res,
                        Ex.IfThen(Ex.MakeBinary(Et.GreaterThan, Ex.Property(split, nameof(Array.Length)), Ex.Constant(tupa)),
                            Ex.Assign(Ex.ArrayAccess(split, Ex.MakeBinary(Et.Subtract, Ex.Property(split, nameof(Array.Length)), Ex.Constant(tupa))),
                                Ex.Call(typeof(string), nameof(string.Join), Type.EmptyTypes, _separatorString,
                                    Ex.Call(typeof(Enumerable), nameof(Enumerable.Take), new[] { typeof(string) }, split,
                                        Ex.MakeBinary(Et.Add, Ex.Constant(1), Ex.MakeBinary(Et.Subtract, Ex.Property(split, nameof(Array.Length)), Ex.Constant(tupa))))))),
                        conversion,
                        Ex.Label(end, Result(to, Ex.Call(Creator(to), res.Select(r => Ex.Property(r, nameof(IConversionResult.Result)))))))));
            var lambda = Ex.Lambda(block, input);
            return lambda;
        }

        public Delegate Create(Type from, Type to)
            => CreateLambda(from, to).Compile();
    }
}
