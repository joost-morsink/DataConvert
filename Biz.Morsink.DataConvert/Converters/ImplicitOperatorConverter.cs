using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Biz.Morsink.DataConvert.DataConvertUtils;

namespace Biz.Morsink.DataConvert.Converters
{
    public class ImplicitOperatorConverter : IConverter
    {
        public static ImplicitOperatorConverter Instance { get; } = new ImplicitOperatorConverter();

        private MethodInfo GetImplicitOperator(Type from, Type to)
        {
            var q = from method in @from.GetTypeInfo().DeclaredMethods
                    let parameters = method.GetParameters()
                    where method.IsPublic && method.IsStatic && method.ReturnType == to && method.Name == "op_Implicit"
                        && parameters.Length == 1 && parameters[0].ParameterType == @from
                    select method;
            var q2 = from method in to.GetTypeInfo().DeclaredMethods
                     let parameters = method.GetParameters()
                     where method.IsPublic && method.IsStatic && method.ReturnType == to && method.Name == "op_Implicit"
                         && parameters.Length == 1 && parameters[0].ParameterType == @from
                     select method;
            return q.Concat(q2).FirstOrDefault();
        }

        public bool CanConvert(Type from, Type to)
            => GetImplicitOperator(from, to) != null;

        public Delegate Create(Type from, Type to)
            => CreateLambda(from, to).Compile();

        public bool SupportsLambda => true;

        public LambdaExpression CreateLambda(Type from, Type to)
        {
            var operatorMethod = GetImplicitOperator(from, to);
            var input = Expression.Parameter(from, "input");
            var block = Result(to, Expression.Call(operatorMethod, input));
            return Expression.Lambda(block, input);
        }
    }
}