using System;
using System.Linq;
using Ex = System.Linq.Expressions.Expression;
using System.Reflection;

namespace Biz.Morsink.DataConvert
{
    static class DataConvertUtils
    {
        public static Ex NoResult(Type t)
            => Ex.Default(typeof(ConversionResult<>).MakeGenericType(t));
        public static Ex Result(Type t, Ex expr)
            => Ex.New(typeof(ConversionResult<>).MakeGenericType(t).GetTypeInfo().DeclaredConstructors
                .Single(ci => ci.GetParameters().Length == 1 && ci.GetParameters()[0].ParameterType == t), expr);
    }
}
