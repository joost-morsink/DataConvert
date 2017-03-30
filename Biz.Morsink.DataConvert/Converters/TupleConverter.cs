using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Ex = System.Linq.Expressions.Expression;
using Et = System.Linq.Expressions.ExpressionType;
using static Biz.Morsink.DataConvert.DataConvertUtils;
namespace Biz.Morsink.DataConvert.Converters
{
    /// <summary>
    /// This component takes care of the conversion of tuples by individually converting the component values.
    /// </summary>
    public class TupleConverter : IConverter, IDataConverterRef
    {
        public const string ValueTuple = nameof(ValueTuple);
        /// <summary>
        /// Gets a TupleConverter singleton.
        /// </summary>
        public static TupleConverter Instance { get; } = new TupleConverter();

        public IDataConverter Ref { get; set; }

        /// <summary>
        /// Gets the tuple arity for a type.
        /// </summary>
        private int tupleArity(Type t)
        {
            if (t.Namespace == nameof(System) && t.Name.StartsWith(nameof(Tuple) + "`"))
                return t.GetTypeInfo().GenericTypeArguments.Length;
            else if (t.Namespace == nameof(System) && (t.Name == ValueTuple || t.Name.StartsWith(ValueTuple+"`")))
                return t.GetTypeInfo().GenericTypeArguments.Length;
            else
                return -1;
        }
        /// <summary>
        /// Gets the type containing the static Create function for the parameter type.
        /// </summary>
        private Type creatorType(Type t)
        {
            if (t.Namespace == nameof(System) && t.Name.StartsWith(nameof(Tuple) + "`"))
                return typeof(Tuple);
            else if (t.Namespace == nameof(System) && (t.Name == ValueTuple || t.Name.StartsWith(ValueTuple+"`")))
                return t.GetTypeInfo().Assembly.GetType($"{nameof(System)}.{ValueTuple}");
            else
                return null;
        }
        /// <summary>
        /// Gets the static Create method to create the parameter type.
        /// </summary>
        private MethodInfo creator(Type t)
        {
            var generics = t.GetTypeInfo().GenericTypeArguments;
            var arity = tupleArity(t);
            return creatorType(t)?.GetTypeInfo().DeclaredMethods
                .Where(m => m.IsStatic && m.Name == nameof(Tuple.Create) && m.GetGenericArguments().Length == arity && m.GetParameters().Length == arity)
                .Select(m => m.MakeGenericMethod(generics.ToArray()))
                .First();
        }
        public bool CanConvert(Type from, Type to)
            => tupleArity(from) >= 0 && tupleArity(from) == tupleArity(to);

        public Delegate Create(Type from, Type to)
        {
            var fromParameters = from.GetTypeInfo().GenericTypeArguments;
            var toParameters = to.GetTypeInfo().GenericTypeArguments;

            var converters = fromParameters
                .Zip(toParameters, (f, t) => Ref.GetConverter(f, t))
                .ToArray();
            var input = Ex.Parameter(from, "input");

            var res = toParameters.Select(t => Ex.Parameter(typeof(ConversionResult<>).MakeGenericType(t))).ToArray();

            var conversion = res.Select((r, i) =>
                    Ex.Assign(res[i],
                        Ex.Invoke(
                            Ex.Constant(converters[i], typeof(Func<,>).MakeGenericType(fromParameters[i], typeof(ConversionResult<>).MakeGenericType(toParameters[i]))),
                            Ex.PropertyOrField(input, $"Item{i + 1}")))).ToArray(); 
            var conversionSuccesful = Enumerable.Aggregate(res, (Ex)Ex.Constant(true),
                            (c, p) => Ex.MakeBinary(Et.AndAlso, c, Ex.Property(p, nameof(IConversionResult.IsSuccessful))));

            var block = Ex.Block(res,
                    Ex.Block(conversion),
                    Ex.Condition(conversionSuccesful,
                        Result(to,
                            Ex.Call(creator(to), Enumerable.Select(res, p => Ex.Property(p, nameof(IConversionResult.Result))))),
                        NoResult(to)));
            var lambda = Ex.Lambda(block, input);
            return lambda.Compile();
        }

    }
}
