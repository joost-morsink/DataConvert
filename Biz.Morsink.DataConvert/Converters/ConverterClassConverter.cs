using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Biz.Morsink.DataConvert.DataConvertUtils;

namespace Biz.Morsink.DataConvert.Converters
{
    public class ConverterClassConverter<T> : IConverter
    {
        public static ConverterClassConverter<T> Instance { get; } = new ConverterClassConverter<T>();

        private MethodInfo GetMethod(Type from, Type to)
        {
            var q = from method in typeof(T).GetTypeInfo().DeclaredMethods
                let parameters = method.GetParameters()
                where method.IsPublic && method.IsStatic
                                      && (method.ReturnType == typeof(ConversionResult<>).MakeGenericType(to)
                                          || method.ReturnType == to)
                                      && parameters.Length == 1 && parameters[0].ParameterType == @from
                select method;

            return q.FirstOrDefault();
        }

        public bool CanConvert(Type from, Type to)
            => GetMethod(from, to) != null;

        public Delegate Create(Type from, Type to)
        {
            var m = GetMethod(from, to);
            if (m.ReturnType == typeof(ConversionResult<>).MakeGenericType(to))
                return m.CreateDelegate(typeof(Func<,>).MakeGenericType(from,
                    typeof(ConversionResult<>).MakeGenericType(to)));

            return CreateLambda(from, to).Compile();
        }

        public bool SupportsLambda => true;

        public LambdaExpression CreateLambda(Type from, Type to)
        {
            var method = GetMethod(from, to);
            var input = Expression.Parameter(from, "input");
            var block = Expression.Call(method, input);
            return Expression.Lambda(
                method.ReturnType == typeof(ConversionResult<>).MakeGenericType(to)
                ? block : Result(to, block), input);
        }
    }
}