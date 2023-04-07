using System;
using System.Linq;
using System.Reflection;

namespace Biz.Morsink.DataConvert.Helpers
{
    static class Tuples
    {
        public const string ValueTuple = nameof(ValueTuple);

        /// <summary>
        /// Gets the tuple arity for a type.
        /// </summary>
        public static int TupleArity(Type t)
        {
            if (t.Namespace == nameof(System) && t.Name.StartsWith(nameof(Tuple) + "`"))
                return t.GetTypeInfo().GenericTypeArguments.Length;
            else if (t.Namespace == nameof(System) && (t.Name == ValueTuple || t.Name.StartsWith(ValueTuple + "`")))
                return t.GetTypeInfo().GenericTypeArguments.Length;
            else
                return -1;
        }
        /// <summary>
        /// Gets the type containing the static Create function for the parameter type.
        /// </summary>
        public static Type CreatorType(Type t)
        {
            if (t.Namespace == nameof(System) && t.Name.StartsWith(nameof(Tuple) + "`"))
                return typeof(Tuple);
            else if (t.Namespace == nameof(System) && (t.Name == ValueTuple || t.Name.StartsWith(ValueTuple + "`")))
                return t.GetTypeInfo().Assembly.GetType($"{nameof(System)}.{ValueTuple}");
            else
                return null;
        }
        /// <summary>
        /// Gets the static Create method to create the parameter type.
        /// </summary>
        public static MethodInfo Creator(Type t)
        {
            var generics = t.GetTypeInfo().GenericTypeArguments;
            var arity = TupleArity(t);
            return CreatorType(t)?.GetTypeInfo().DeclaredMethods
                .Where(m => m.IsStatic && m.Name == nameof(Tuple.Create) && m.GetGenericArguments().Length == arity && m.GetParameters().Length == arity)
                .Select(m => m.MakeGenericMethod(generics.ToArray()))
                .First();
        }
    }
}
