﻿using System;
using System.Linq;
using Ex = System.Linq.Expressions.Expression;
using Et = System.Linq.Expressions.ExpressionType;
using System.Reflection;

namespace Biz.Morsink.DataConvert.Converters
{
    using Biz.Morsink.DataConvert.Helpers;
    using System.Globalization;
    using System.Linq.Expressions;
    using static Biz.Morsink.DataConvert.DataConvertUtils;
    /// <summary>
    /// Converts a value by first converting it to string and feeding that string back into the pipeline.
    /// </summary>
    public class FromStringRepresentationConverter : IConverter, IDataConverterRef
    {
        public IDataConverter Ref { get; set; }
        public IFormatProvider FormatProvider { get; }
        public bool RequireDeclaredMethod { get; }

        public bool SupportsLambda => true;

        public FromStringRepresentationConverter(bool requireDeclaredMethod = true, IFormatProvider formatProvider = null)
        {
            RequireDeclaredMethod = requireDeclaredMethod;
            FormatProvider = formatProvider ?? CultureInfo.InvariantCulture;
        }
        /// <summary>
        /// Gets the ToString method to use in a conversion. A ToString with an IFormatProvider takes precedence over a parameterless method.
        /// </summary>
        /// <param name="type">The type containing the ToString method.</param>
        /// <returns>A MethodInfo of a ToString method on the specified type.</returns>
        public MethodInfo GetToString(Type type)
        {
            var q = from m in type.GetTypeInfo().GetDeclaredMethods("ToString")
                    where m.IsPublic && !m.IsStatic && m.ReturnType == typeof(string)
                    let ps = m.GetParameters()
                    where ps.Length == 0
                    || ps.Length == 1 && ps[0].ParameterType == typeof(IFormatProvider)
                    orderby ps.Length descending
                    select m;
            return q.FirstOrDefault() ?? (RequireDeclaredMethod ? null : typeof(object).GetTypeInfo().GetDeclaredMethod(nameof(object.ToString)));
        }
        private Ex getParameter(ParameterInfo parameter, Ex input)
        {
            if (parameter.ParameterType == typeof(IFormatProvider))
                return Ex.Constant(FormatProvider);
            else
                throw new ArgumentException("Unknown parameter type");
        }
        public bool CanConvert(Type from, Type to)
            => from != typeof(string) && from != typeof(object) && GetToString(from) != null;

        public LambdaExpression CreateLambda(Type from, Type to)
        {
            var input = Ex.Parameter(from, "input");
            var toString = GetToString(from);
            var converter = Ref.GetLambda(typeof(string), to);
            var str = Ex.Parameter(typeof(string), "str");
            Ex block = Ex.Block(new[] { str },
                Ex.Assign(str, Ex.Call(input, toString, toString.GetParameters().Select(pi => getParameter(pi, input)))),
                converter.ApplyTo(str));

            if (!from.GetTypeInfo().IsValueType)
                block = Ex.Condition(Ex.MakeBinary(Et.Equal, input, Ex.Default(from)),
                    NoResult(to),
                    block);
            var lambda = Ex.Lambda(block, input);
            return lambda;
        }

        public Delegate Create(Type from, Type to)
            => CreateLambda(from, to).Compile();
    }
}
