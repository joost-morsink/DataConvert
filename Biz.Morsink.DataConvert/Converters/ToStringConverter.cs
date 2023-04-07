using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Ex = System.Linq.Expressions.Expression;
using Et = System.Linq.Expressions.ExpressionType;
namespace Biz.Morsink.DataConvert.Converters
{
    using System.Linq.Expressions;
    using static DataConvertUtils;
    /// <summary>
    /// This converter handles all conversions to string using either the object.ToString() or a specific ToString(IFormatProvider) method. 
    /// However it should not accept objects in general, as these need to be handled by a type checking converter. 
    /// </summary>
    public class ToStringConverter : IConverter
    {
        /// <summary>
        /// Constructs a new ToStringConverter.
        /// </summary>
        /// <param name="succeedOnNull">
        ///     Indicates whether a null input value should succeed (with a result empty string).
        /// </param>
        /// <param name="requireDeclaredMethod">
        ///     Indicates whether a parameterless ToString method is required for conversion, or if the default object.ToString may be used.
        /// </param>
        /// <param name="formatProvider">
        ///     Defines the IFormatProvider to be optionally used in the ToString method.
        ///     Default value is CultureInfo.InvariantCulture
        /// </param>
        public ToStringConverter(bool succeedOnNull, bool requireDeclaredMethod = true, IFormatProvider formatProvider = null)
        {
            SucceedOnNull = succeedOnNull;
            RequireDeclaredMethod = requireDeclaredMethod;
            FormatProvider = formatProvider ?? CultureInfo.InvariantCulture;
        }
        /// <summary>
        /// Indicates whether a null input will lead to a successful conversion (empty string) or not.
        /// </summary>
        public bool SucceedOnNull { get; }
        /// <summary>
        /// Gets the IFormatProvider used for ToString conversions
        /// </summary>
        public IFormatProvider FormatProvider { get; }
        /// <summary>
        /// Indicates whether a declared parameterless ToString methods is required for conversion.
        /// </summary>
        public bool RequireDeclaredMethod { get; }

        public bool SupportsLambda => throw new NotImplementedException();

        public bool CanConvert(Type from, Type to)
            => from != typeof(object) && to == typeof(string) && GetToString(from) != null;

        public LambdaExpression CreateLambda(Type from, Type to)
        {
            var toString = GetToString(from);
            var input = Ex.Parameter(from, "input");

            // Conversion from value types does not need to deal with null values.
            if (from.GetTypeInfo().IsValueType)
            {
                var block = getResult(to, toString, input);
                var lambda = Ex.Lambda(block, input);
                return lambda;
            }
            else
            {
                var inner = getResult(to, toString, input);
                var block = Ex.Condition(Ex.MakeBinary(Et.Equal, input, Ex.Default(from)),
                    SucceedOnNull ? Result(to, Ex.Constant("")) : NoResult(to),
                    inner);
                var lambda = Ex.Lambda(block, input);
                return lambda;
            }
        }

        private MethodInfo GetToString(Type from)
        {
            return (from method in @from.GetTypeInfo().DeclaredMethods
                    where method.IsPublic && !method.IsStatic && method.Name == nameof(object.ToString)
                    let parameters = method.GetParameters()
                    where parameters.Length == 1 && parameters[0].ParameterType == typeof(IFormatProvider)
                    select method).Concat(
                    from method in (RequireDeclaredMethod ? @from : typeof(object)).GetTypeInfo().DeclaredMethods
                    where method.IsPublic && !method.IsStatic && method.Name == nameof(object.ToString)
                        && method.GetParameters().Length == 0
                    select method).FirstOrDefault();
        }

        private Ex getResult(Type to, MethodInfo toString, System.Linq.Expressions.ParameterExpression input)
        {
            return toString.GetParameters().Length == 0
                ? Result(to, Ex.Call(input, toString))
                : Result(to, Ex.Call(input, toString, Ex.Constant(FormatProvider)));
        }

        public Delegate Create(Type from, Type to)
            => CreateLambda(from, to).Compile();
    }
}
