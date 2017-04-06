using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Ex = System.Linq.Expressions.Expression;
using static Biz.Morsink.DataConvert.DataConvertUtils;
using static Biz.Morsink.DataConvert.Helpers.Tuples;
using System.Collections;

namespace Biz.Morsink.DataConvert.Converters
{
    /// <summary>
    /// This components converts an enumerable of several values into an equivalent Tuple type. 
    /// Component values are converted individually.
    /// </summary>
    public class EnumerableToTupleConverter : IConverter, IDataConverterRef
    {
        /// <summary>
        /// Gets an EnumerableToTupleConverter singleton.
        /// </summary>
        public static EnumerableToTupleConverter Instance { get; } = new EnumerableToTupleConverter();
        public IDataConverter Ref { get; set; }

        public bool CanConvert(Type from, Type to)
            => from != typeof(string) && getGetEnumeratorMethod(from) != null && TupleArity(to) >= 0;

        public Delegate Create(Type from, Type to)
        {
            return createForList(from, to)
                ?? createForCollection(from, to)
                ?? createDefault(from, to);
        }
        private static MethodInfo getGetEnumeratorMethod(Type t)
        {
            var specific = (from m in t.GetTypeInfo().GetDeclaredMethods(nameof(IEnumerable.GetEnumerator))
                            where m.GetParameters().Length == 0
                            && m.ReturnType.GetTypeInfo().GetDeclaredMethods(nameof(IEnumerator.MoveNext))
                                    .Any(mn => mn.ReturnType == typeof(bool) && mn.GetParameters().Length == 0)
                            && m.ReturnType.GetTypeInfo().GetDeclaredProperty(nameof(IEnumerator.Current)) != null
                            select m).FirstOrDefault();
            if (specific != null)
                return specific;
            var eType = t.GetTypeInfo().ImplementedInterfaces
                .Where(i => i.GenericTypeArguments.Length == 1 && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Select(i => i.GenericTypeArguments[0])
                .FirstOrDefault();
            if (eType != null)
                return typeof(IEnumerable<>).MakeGenericType(eType).GetTypeInfo().GetDeclaredMethod(nameof(IEnumerable<object>.GetEnumerator));
            else if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()))
                return typeof(IEnumerable).GetTypeInfo().GetDeclaredMethod(nameof(IEnumerable.GetEnumerator));
            else
                return null;
        }

        private Delegate createDefault(Type from, Type to)
        {
            var toParameters = to.GetTypeInfo().GenericTypeArguments;
            var input = Ex.Parameter(from, "input");
            var getEnumerator = getGetEnumeratorMethod(from);
            var enumerator = Ex.Parameter(getEnumerator.ReturnType, "enumerator");
            var eType = getEnumerator.ReturnType.GetTypeInfo().GetDeclaredProperty(nameof(IEnumerator.Current)).PropertyType;
            var converters = toParameters.Select(p => Ref.GetConverter(eType, p)).ToArray();

            var res = toParameters.Select(p => Ex.Parameter(typeof(ConversionResult<>).MakeGenericType(p))).ToArray();
            var end = Ex.Label(typeof(ConversionResult<>).MakeGenericType(to), "end");
            var conversion =
                Ex.Block(res.Zip(converters, (r, con) =>
                    Ex.Block(
                        Ex.IfThenElse(
                            enumerator.Type.GetTypeInfo().GetDeclaredMethod(nameof(IEnumerator.MoveNext)) == null
                                ? Ex.Call(Ex.Convert(enumerator, typeof(IEnumerator)), nameof(IEnumerator.MoveNext), Type.EmptyTypes)
                                : Ex.Call(enumerator, enumerator.Type.GetTypeInfo().GetDeclaredMethod(nameof(IEnumerator.MoveNext))),
                            Ex.Assign(r,
                                Ex.Invoke(
                                    Ex.Constant(con, typeof(Func<,>).MakeGenericType(eType, r.Type)),
                                    Ex.Property(enumerator, nameof(IEnumerator.Current)))),
                            Ex.Goto(end, NoResult(to))),
                        Ex.IfThen(Ex.Not(Ex.Property(r, nameof(IConversionResult.IsSuccessful))),
                            Ex.Goto(end, NoResult(to))))));
            var block = Ex.Block(res.Concat(new[] { enumerator }),
                Ex.Assign(enumerator, Ex.Call(input, getEnumerator)),
                typeof(IDisposable).GetTypeInfo().IsAssignableFrom(enumerator.Type.GetTypeInfo())
                    ? (Ex)Ex.TryFinally(conversion,
                        Ex.Call(enumerator, typeof(IDisposable).GetTypeInfo().GetDeclaredMethod(nameof(IDisposable.Dispose))))
                    : conversion,
                Ex.Label(end, Result(to, Ex.Call(Creator(to), res.Select(r => Ex.Property(r, nameof(IConversionResult.Result)))))));

            var lambda = Ex.Lambda(block, input);
            return lambda.Compile();
        }

        private Delegate createForCollection(Type from, Type to)
        {
            return null; // To be implemented
        }

        private Delegate createForList(Type from, Type to)
        {
            return null; // To be implemented
        }


    }
}
