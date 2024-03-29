﻿using System;
using System.Linq;
using System.Reflection;
using Ex = System.Linq.Expressions.Expression;
using static Biz.Morsink.DataConvert.DataConvertUtils;
using System.Linq.Expressions;
using Biz.Morsink.DataConvert.Helpers;

namespace Biz.Morsink.DataConvert.Converters
{
    /// <summary>
    /// This converter always succeeds by first trying the conversion to the non-nullable type. 
    /// When that fails, this converter succeeds with a 'null'-value (default(Nullable<>)).
    /// </summary>
    public class ToNullableConverter : IConverter, IDataConverterRef
    {
        public IDataConverter Ref { get; set; }

        public bool SupportsLambda => true;

        public bool CanConvert(Type from, Type to)
            => to.GetTypeInfo().GenericTypeArguments.Length == 1
            && to.GetTypeInfo().GetGenericTypeDefinition() == typeof(Nullable<>);

        public LambdaExpression CreateLambda(Type from, Type to)
        {
            var innerTo = to.GetTypeInfo().GenericTypeArguments[0];
            var input = Ex.Parameter(from, "input");
            var resultType = typeof(ConversionResult<>).MakeGenericType(innerTo);
            var result = Ex.Parameter(resultType, "result");

            var baseConverter = Ref.GetLambda(from, innerTo);
            var block = Ex.Block(new[] { result },
                Ex.Assign(result, baseConverter.ApplyTo(input)),
                Ex.Condition(Ex.Property(result, nameof(IConversionResult.IsSuccessful)),
                    Result(to,
                        Ex.New(typeof(Nullable<>)
                                .MakeGenericType(innerTo)
                                .GetTypeInfo()
                                .DeclaredConstructors
                                .First(ci => ci.GetParameters().Length == 1 && ci.GetParameters()[0].ParameterType == innerTo),
                            Ex.Property(result, nameof(IConversionResult.Result)))),
                    Result(to, Ex.Default(typeof(Nullable<>).MakeGenericType(innerTo)))));
            var lambda = Ex.Lambda(block, input);
            return lambda;
        }

        public Delegate Create(Type from, Type to)
            => CreateLambda(from, to).Compile();
    }
}
