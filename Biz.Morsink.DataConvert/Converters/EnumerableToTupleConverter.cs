using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ex = System.Linq.Expressions.Expression;
using Et = System.Linq.Expressions.ExpressionType;
using static Biz.Morsink.DataConvert.DataConvertUtils;
using static Biz.Morsink.DataConvert.Helpers.Tuples;
using System.Collections;
using System.Linq.Expressions;
using Biz.Morsink.DataConvert.Helpers;

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

        public bool SupportsLambda => true;

        public bool CanConvert(Type from, Type to)
            => from != typeof(string) && getGetEnumeratorMethod(from) != null && TupleArity(to) >= 0;

        public LambdaExpression CreateLambda(Type from, Type to)
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

        private LambdaExpression createDefault(Type from, Type to)
        {
            var toParameters = to.GetTypeInfo().GenericTypeArguments;
            var input = Ex.Parameter(from, "input");
            var getEnumerator = getGetEnumeratorMethod(from);
            var enumerator = Ex.Parameter(getEnumerator.ReturnType, "enumerator");
            var eType = getEnumerator.ReturnType.GetTypeInfo().GetDeclaredProperty(nameof(IEnumerator.Current)).PropertyType;
            var converters = toParameters.Select(p => Ref.GetLambda(eType, p)).ToArray();

            var res = toParameters.Select(p => Ex.Parameter(typeof(ConversionResult<>).MakeGenericType(p))).ToArray();
            var end = Ex.Label(typeof(ConversionResult<>).MakeGenericType(to), "end");
            var conversion = enumeratorConversion(to, enumerator, eType, converters, res, end);
            var block = Ex.Block(res.Concat(new[] { enumerator }),
                Ex.Assign(enumerator, Ex.Call(input, getEnumerator)),
                typeof(IDisposable).GetTypeInfo().IsAssignableFrom(enumerator.Type.GetTypeInfo())
                    ? (Ex)Ex.TryFinally(conversion,
                        Ex.Call(enumerator, typeof(IDisposable).GetTypeInfo().GetDeclaredMethod(nameof(IDisposable.Dispose))))
                    : conversion,
                Ex.Label(end, Result(to, Ex.Call(Creator(to), res.Select(r => Ex.Property(r, nameof(IConversionResult.Result)))))));

            var lambda = Ex.Lambda(block, input);
            return lambda;
        }

        private LambdaExpression createForCollection(Type from, Type to)
        {
            var cType = (from i in @from.GetTypeInfo().ImplementedInterfaces.Concat(new[] { @from })
                         where i.GetTypeInfo().IsInterface 
                            && i.GenericTypeArguments.Length == 1 
                            && i.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>)
                         select i.GenericTypeArguments[0]).SingleOrDefault();
            if (cType == null)
                return null;
            var toParameters = to.GetTypeInfo().GenericTypeArguments;
            var input = Ex.Parameter(from, "input");
            var getEnumerator = getGetEnumeratorMethod(from);
            var enumerator = Ex.Parameter(getEnumerator.ReturnType, "enumerator");
            var eType = getEnumerator.ReturnType.GetTypeInfo().GetDeclaredProperty(nameof(IEnumerator.Current)).PropertyType;
            var converters = toParameters.Select(p => Ref.GetLambda(eType, p)).ToArray();

            var res = toParameters.Select(p => Ex.Parameter(typeof(ConversionResult<>).MakeGenericType(p))).ToArray();
            var end = Ex.Label(typeof(ConversionResult<>).MakeGenericType(to), "end");
            Ex conversion = enumeratorConversion(to, enumerator, eType, converters, res, end);
            var block = Ex.Block(res.Concat(new[] { enumerator }),
                Ex.IfThen(
                    Ex.MakeBinary(Et.LessThan,
                        Ex.Property(input, typeof(IReadOnlyCollection<>).MakeGenericType(cType).GetTypeInfo()
                                                .GetDeclaredProperty(nameof(IReadOnlyCollection<object>.Count))),
                        Ex.Constant(TupleArity(to))),
                    Ex.Goto(end, NoResult(to))),
                Ex.Assign(enumerator, Ex.Call(input, getEnumerator)),
                typeof(IDisposable).GetTypeInfo().IsAssignableFrom(enumerator.Type.GetTypeInfo())
                    ? (Ex)Ex.TryFinally(conversion,
                        Ex.Call(enumerator, typeof(IDisposable).GetTypeInfo().GetDeclaredMethod(nameof(IDisposable.Dispose))))
                    : conversion,
                Ex.Label(end, Result(to, Ex.Call(Creator(to), res.Select(r => Ex.Property(r, nameof(IConversionResult.Result)))))));

            var lambda = Ex.Lambda(block, input);
            return lambda;
        }

        private static Ex enumeratorConversion(Type to, Ex enumerator, Type eType, LambdaExpression[] converters, Ex[] res, System.Linq.Expressions.LabelTarget end)
        {
            return Ex.Block(res.Zip(converters, (r, con) =>
                   Ex.Block(
                        Ex.IfThenElse(
                            enumerator.Type.GetTypeInfo().GetDeclaredMethod(nameof(IEnumerator.MoveNext)) == null
                                ? Ex.Call(Ex.Convert(enumerator, typeof(IEnumerator)), nameof(IEnumerator.MoveNext), Type.EmptyTypes)
                                : Ex.Call(enumerator, enumerator.Type.GetTypeInfo().GetDeclaredMethod(nameof(IEnumerator.MoveNext))),
                            Ex.Assign(r, con.ApplyTo(Ex.Property(enumerator, nameof(IEnumerator.Current)))),
                            Ex.Goto(end, NoResult(to))),
                        Ex.IfThen(Ex.Not(Ex.Property(r, nameof(IConversionResult.IsSuccessful))),
                            Ex.Goto(end, NoResult(to))))));
        }

        private LambdaExpression createForList(Type from, Type to)
        {
            var cType = (from i in @from.GetTypeInfo().ImplementedInterfaces.Concat(new[] { @from })
                         where i.GetTypeInfo().IsInterface
                             && i.GenericTypeArguments.Length == 1
                             && i.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)
                         select i.GenericTypeArguments[0]).SingleOrDefault();
            if (cType == null)
                return null;
            var indexer = typeof(IReadOnlyList<>).MakeGenericType(cType).GetTypeInfo().GetDeclaredProperty("Item");
            var count = typeof(IReadOnlyCollection<>).MakeGenericType(cType).GetTypeInfo().GetDeclaredProperty(nameof(IReadOnlyCollection<object>.Count));
            var toParameters = to.GetTypeInfo().GenericTypeArguments;
            var input = Ex.Parameter(from, "input");
            var converters = toParameters.Select(p => Ref.GetLambda(cType, p)).ToArray();
            var res = toParameters.Select(p => Ex.Parameter(typeof(ConversionResult<>).MakeGenericType(p))).ToArray();
            var end = Ex.Label(typeof(ConversionResult<>).MakeGenericType(to), "end");

            var conversion = Ex.Block(converters.Select((c, i) =>
                Ex.Block(
                    Ex.Assign(res[i],
                        c.ApplyTo(Ex.MakeIndex(input, indexer, new[] { Ex.Constant(i) }))),
                    Ex.IfThen(Ex.Not(Ex.Property(res[i], nameof(IConversionResult.IsSuccessful))),
                        Ex.Goto(end, NoResult(to))))));
            var block = Ex.Block(res,
                Ex.IfThen(
                    Ex.MakeBinary(Et.LessThan,
                        Ex.Property(input, count),
                        Ex.Constant(toParameters.Length)),
                    Ex.Goto(end, NoResult(to))),
                conversion,
                Ex.Label(end, Result(to, Ex.Call(Creator(to), res.Select(r => Ex.Property(r, nameof(IConversionResult.Result)))))));
            var lambda = Ex.Lambda(block, input);
            return lambda;
        }
        public Delegate Create(Type from, Type to)
            => CreateLambda(from, to).Compile();
    }
}
