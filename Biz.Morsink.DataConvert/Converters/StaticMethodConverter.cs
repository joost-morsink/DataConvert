using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Ex = System.Linq.Expressions.Expression;
using static Biz.Morsink.DataConvert.DataConvertUtils;

namespace Biz.Morsink.DataConvert.Converters
{
    public class StaticMethodConverter : IConverter
    {
        public static StaticMethodConverter Instance { get; } = new StaticMethodConverter("TryConvert");
        
        private readonly string _methodName;

        public StaticMethodConverter(string methodName)
        {
            _methodName = methodName;
        }
        private MethodInfo GetMethod(Type from, Type to)
        {
            return to.GetTypeInfo().DeclaredMethods
                .FirstOrDefault(m => m.IsStatic && m.GetParameters().Length == 2
                                                && m.GetParameters()[0].ParameterType == from 
                                                && m.GetParameters()[1].ParameterType == to.MakeByRefType()
                                                && m.ReturnType == typeof(bool));
        }

        public bool CanConvert(Type from, Type to)
            => GetMethod(from, to) != null;

        public Delegate Create(Type from, Type to)
            => CreateLambda(from, to).Compile();

        public bool SupportsLambda => true;
        public LambdaExpression CreateLambda(Type from, Type to)
        {
            var input = Ex.Parameter(from, "input");
            var result = Ex.Parameter(to, "result");
            var m = GetMethod(from, to);
            var block = Ex.Block(new[] { result },
                Ex.Condition(Ex.Call(m, input, result),
                    Result(to, result),
                    NoResult(to)));

            return Ex.Lambda(block, input);
        }
    }
}