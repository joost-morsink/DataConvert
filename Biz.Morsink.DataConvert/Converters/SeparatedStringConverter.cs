using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ex = System.Linq.Expressions.Expression;
using Et = System.Linq.Expressions.ExpressionType;
using static Biz.Morsink.DataConvert.DataConvertUtils;
using System.Reflection;

namespace Biz.Morsink.DataConvert.Converters
{
    /// <summary>
    /// This class converts a strings in separated format to something that can be converted to from a string array.
    /// This class also converts enumerable/arrays or something that can be converted to a string array to a string using a separator character.
    /// </summary>
    public class SeparatedStringConverter : IConverter, IDataConverterRef
    {
        public IDataConverter Ref { get; set; }
        /// <summary>
        /// An array containing possible separator characters. 
        /// The first one can be used to convert back to separated format.
        /// </summary>
        public char[] Separators { get; }

        public SeparatedStringConverter(params char[] separators)
        {
            Separators = separators;
        }
        public bool CanConvert(Type from, Type to)
            => from == typeof(string) || from != typeof(string) && to == typeof(string);

        public Delegate Create(Type from, Type to)
        {
            if (from == typeof(string))
            {
                return fromStringDelegate(to);
            }
            else
                return toStringDelegate(from);
        }

        private Delegate fromStringDelegate(Type to)
        {
            var input = Ex.Parameter(typeof(string), "input");
            var split = Ex.Parameter(typeof(string[]), "split");

            var block = Ex.Condition(Ex.MakeBinary(Et.Equal, input, Ex.Default(typeof(string))),
                    NoResult(to),
                    Ex.Block(new[] { split },
                        Ex.Assign(split, Ex.Call(input, nameof(string.Split), Type.EmptyTypes, Ex.Constant(Separators))),
                        Ex.Condition(Ex.MakeBinary(Et.Equal, Ex.Property(split, nameof(Array.Length)), Ex.Constant(1)),
                            NoResult(to),
                            Ex.Invoke(
                                Ex.Convert(
                                    Ex.Call(Ex.Constant(Ref), nameof(IDataConverter.GetConverter), Type.EmptyTypes, Ex.Constant(typeof(string[])), Ex.Constant(to)),
                                    typeof(Func<,>).MakeGenericType(typeof(string[]), typeof(ConversionResult<>).MakeGenericType(to))),
                                split))));
            var lambda = Ex.Lambda(block, input);
            return lambda.Compile();
        }
        private Delegate toStringDelegate(Type from)
        {
            if (from == typeof(string[]))
                return fromStringArrayDelegate(from);
            else if (typeof(object[]).GetTypeInfo().IsAssignableFrom(from.GetTypeInfo()))
                return fromArrayDelegate(from);
            else if (from.GetTypeInfo().GenericTypeArguments.Length == 1 && from.GetTypeInfo().GetGenericTypeDefinition() == typeof(IEnumerable<>)
                || from.GetTypeInfo().ImplementedInterfaces.Any(i => i.GenericTypeArguments.Length == 1 && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                return fromEnumerableDelegate(from);
            else
                return fromConvertDelegate(from);
        }

        private Delegate fromStringArrayDelegate(Type from)
        {
            var input = Ex.Parameter(from, "input");
            var result = Ex.Call((from mi in typeof(string).GetTypeInfo().GetDeclaredMethods(nameof(string.Join))
                                  let par = mi.GetParameters()
                                  where par.Length == 2 && par[0].ParameterType == typeof(string) && par[1].ParameterType == typeof(string[])
                                  select mi).Single(),
                            Ex.Constant(Separators[0].ToString()), input);
            var block = conditional(from, input, result);
            var lambda = Ex.Lambda(block, input);
            return lambda.Compile();
        }

        private static Ex conditional(Type from, Ex input, Ex result)
        {
            return Ex.Condition(
                            Ex.MakeBinary(Et.OrElse,
                                Ex.MakeBinary(Et.Equal, input, Ex.Default(from)),
                                Ex.MakeBinary(Et.Equal, Ex.Property(input, nameof(Array.Length)), Ex.Constant(0))),
                            NoResult(typeof(string)),
                            Result(typeof(string), result));
        }

        private Delegate fromArrayDelegate(Type from)
        {
            var input = Ex.Parameter(from, "input");
            var result = Ex.Call((from mi in typeof(string).GetTypeInfo().GetDeclaredMethods(nameof(string.Join))
                                  let par = mi.GetParameters()
                                  where par.Length == 2 && par[0].ParameterType == typeof(string) && par[1].ParameterType == typeof(object[])
                                  select mi).Single(),
                            Ex.Constant(Separators[0].ToString()), input);
            var block = conditional(from, Ex.Convert(input, typeof(object[])), result);
            var lambda = Ex.Lambda(block, input);
            return lambda.Compile();
        }
        private Delegate fromEnumerableDelegate(Type from)
        {
            var input = Ex.Parameter(from, "input");
            var eType = from.GetTypeInfo().ImplementedInterfaces
                .Where(i => i.GenericTypeArguments.Length == 1 && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Select(i => i.GenericTypeArguments[0]).SingleOrDefault()
                ?? from.GetTypeInfo().GenericTypeArguments[0];
            var res = Ex.Parameter(typeof(string), "res");
            var result = Ex.Block(new[] { res },
                Ex.Assign(res, Ex.Call((from mi in typeof(string).GetTypeInfo().GetDeclaredMethods(nameof(string.Join))
                                        where mi.GetGenericArguments().Length == 1
                                        let par = mi.GetParameters()
                                        where par.Length == 2
                                          && par[0].ParameterType == typeof(string)
                                          && par[1].ParameterType == typeof(IEnumerable<>).MakeGenericType(mi.GetGenericArguments()[0])
                                        select mi).Single().MakeGenericMethod(eType),
                            Ex.Constant(Separators[0].ToString()), input)),
                Ex.Condition(Ex.MakeBinary(Et.Equal, Ex.Property(res, nameof(string.Length)), Ex.Constant(0)),
                    NoResult(typeof(string)),
                    Result(typeof(string), res)));

            var block = Ex.Condition(Ex.MakeBinary(Et.Equal, input, Ex.Default(from)),
                NoResult(typeof(string)),
                result);
            var lambda = Ex.Lambda(block, input);
            return lambda.Compile();
        }
        private Delegate fromConvertDelegate(Type from)
        {
            var input = Ex.Parameter(from, "input");
            var converted = Ex.Parameter(typeof(ConversionResult<string[]>), "converted");
            var converter = Ref.GetConverter(from, typeof(string[]));
            var result = Ex.Call((from mi in typeof(string).GetTypeInfo().GetDeclaredMethods(nameof(string.Join))
                                  let par = mi.GetParameters()
                                  where par.Length == 2 && par[0].ParameterType == typeof(string) && par[1].ParameterType == typeof(string[])
                                  select mi).Single(),
                Ex.Constant(Separators[0].ToString()), Ex.Property(converted, nameof(IConversionResult.Result)));
            var block = Ex.Block(new[] { converted },
                Ex.Assign(converted,
                    Ex.Invoke(Ex.Constant(converter, typeof(Func<,>).MakeGenericType(from, typeof(ConversionResult<string[]>))), input)),
                Ex.Condition(
                    Ex.MakeBinary(Et.AndAlso,
                        Ex.Property(converted, nameof(IConversionResult.IsSuccessful)),
                        Ex.MakeBinary(Et.GreaterThan,
                            Ex.Property(Ex.Property(converted, nameof(IConversionResult.Result)), nameof(Array.Length)),
                            Ex.Constant(0))),
                    Result(typeof(string), result),
                    NoResult(typeof(string))));
            var lambda = Ex.Lambda(block, input);
            return lambda.Compile();
        }
    }
}
