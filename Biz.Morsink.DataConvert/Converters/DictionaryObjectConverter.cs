using Biz.Morsink.DataConvert.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Ex = System.Linq.Expressions.Expression;
using static Biz.Morsink.DataConvert.DataConvertUtils;
using System.Runtime.InteropServices;

namespace Biz.Morsink.DataConvert.Converters
{
    /// <summary>
    /// The DictionaryObjectConverter converts back and forth between Dictionary&lt;string, V&gt;, IDictionary&lt;string, V&gt; and data class supporting either the getter/setter pattern or the readonly properties with a constructor parameter for each property pattern.
    /// </summary>
    public class DictionaryObjectConverter : IConverter, IDataConverterRef
    {
        // TODO: Implement case sensitive/insensitive keys for the dictionary type
        // public static DictionaryObjectConverter CaseSensitive => new DictionaryObjectConverter();
        // public static DictionaryObjectConverter CaseInsensitive = new DictionaryObjectConverter(CaseInsensitiveEqualityComparer.Instance);
        // public DictionaryObjectConverter(IEqualityComparer<string> keyComparer = null)
        // {
        //     KeyComparer = keyComparer ?? EqualityComparer<string>.Default; ;
        // }
        // public IEqualityComparer<string> KeyComparer { get; }

        public bool SupportsLambda => true;

        public IDataConverter Ref { get; set; }

        public bool CanConvert(Type from, Type to)
            => IsCompatibleDictionaryType(from) && IsCompatibleObjectType(to)
            || IsCompatibleObjectType(from) && IsCompatibleDictionaryType(to);

        public LambdaExpression CreateLambda(Type from, Type to)
        {
            if (IsCompatibleDictionaryType(from))
            {
                if (GetConstructorForType(to) != null)
                    return constructionLambda(from, to);
                else
                    return setterLambda(from, to);
            }
            else
            {
                return toDictLambda(from, to);
            }
        }

        private LambdaExpression toDictLambda(Type from, Type to)
        {
            var valueType = GetDictionaryValueType(to);
            var dictType = typeof(Dictionary<,>).MakeGenericType(typeof(string), valueType);
            var add = dictType.GetTypeInfo().GetDeclaredMethod(nameof(Dictionary<string, object>.Add));

            var input = Ex.Parameter(from, "input");
            var tmp = Ex.Parameter(typeof(ConversionResult<>).MakeGenericType(valueType), "tmp");
            var res = Ex.Parameter(dictType, "res");
            var getters = GetReadablePropertiesForType(from);
            var converters = getters.Select(g => Ref.GetLambda(g.PropertyType, valueType));

            var end = Ex.Label(typeof(ConversionResult<>).MakeGenericType(to));
            var block = Ex.Block(new[] { tmp, res },
                Ex.Assign(res, Ex.New(GetParameterlessConstructor(dictType))),
                Ex.Block(getters.Zip(converters, (g, c) => new { g, c })
                    .Select(x =>
                        Ex.Block(
                            Ex.Assign(tmp, x.c.ApplyTo(Ex.Property(input, x.g))),
                            Ex.IfThenElse(Ex.Property(tmp, nameof(IConversionResult.IsSuccessful)),
                                Ex.Call(res, add, Ex.Constant(x.g.Name), Ex.Property(tmp, nameof(IConversionResult.Result))),
                                Ex.Goto(end, NoResult(to)))))),
                Ex.Label(end, Result(to, res)));
            return Ex.Lambda(block, input);
        }

        private LambdaExpression setterLambda(Type from, Type to)
        {
            var valueType = GetDictionaryValueType(from);
            var dictType = typeof(IDictionary<,>).MakeGenericType(typeof(string), valueType);
            var tryGet = dictType.GetTypeInfo().GetDeclaredMethod(nameof(IDictionary<string, object>.TryGetValue));

            var input = Ex.Parameter(from, "input");
            var tmp = Ex.Parameter(valueType, "tmp");
            var ctor = GetParameterlessConstructor(to);
            var setters = GetWritablePropertiesForType(to);
            var res = Ex.Parameter(to, "res");
            var pars = setters.Select((p, i) => new
            {
                Converter = Ref.GetLambda(valueType, p.PropertyType),
                Var = Ex.Parameter(typeof(ConversionResult<>).MakeGenericType(p.PropertyType), p.Name)
            }).ToArray();
            var end = Ex.Label(typeof(ConversionResult<>).MakeGenericType(to), "end");
            var block = Ex.Block(new[] { tmp, res }.Concat(pars.Select(x => x.Var)),
                Ex.Assign(res, Ex.New(ctor)),
                Ex.Block(pars
                    .Select(x =>
                        Ex.Block(
                            Ex.IfThen(
                                Ex.MakeBinary(ExpressionType.OrElse,
                                    Ex.Call(input, tryGet, Ex.Constant(x.Var.Name), tmp),
                                    Ex.Call(input, tryGet, Ex.Constant(ToCamelCase(x.Var.Name)), tmp)),
                                Ex.Block(
                                    Ex.Assign(x.Var, x.Converter.ApplyTo(tmp)),
                                    Ex.IfThenElse(Ex.Property(x.Var, nameof(IConversionResult.IsSuccessful)),
                                        Ex.Assign(Ex.Property(res, x.Var.Name), Ex.Property(x.Var, nameof(IConversionResult.Result))),
                                        Ex.Goto(end, NoResult(to)))))))),
                Ex.Label(end, Result(to, res)));
            return Ex.Lambda(block, input);
        }

        private LambdaExpression constructionLambda(Type from, Type to)
        {
            var valueType = GetDictionaryValueType(from);
            var dictType = typeof(IDictionary<,>).MakeGenericType(typeof(string), valueType);
            var tryGet = dictType.GetTypeInfo().GetDeclaredMethod(nameof(IDictionary<string, object>.TryGetValue));

            var input = Ex.Parameter(from, "input");
            var ctor = GetConstructorForType(to);
            var tmp = Ex.Parameter(valueType, "tmp");
            var pars = ctor.GetParameters().Select((p, i) => new
            {
                Converter = Ref.GetLambda(valueType, p.ParameterType),
                Var = Ex.Parameter(typeof(ConversionResult<>).MakeGenericType(p.ParameterType), p.Name),
                Type = p.ParameterType,
                Optional = p.GetCustomAttributes<OptionalAttribute>().Any(),
                Default = p.HasDefaultValue ? p.DefaultValue : p.ParameterType.GetTypeInfo().IsValueType ? Activator.CreateInstance(p.ParameterType) : null
            }).ToArray();
            var end = Ex.Label(typeof(ConversionResult<>).MakeGenericType(to), "end");
            var block = Ex.Block(pars.Select(x => x.Var),
                Ex.Block(new[] { tmp }, pars
                    .Select(x =>
                        x.Optional
                        ? Ex.Block(
                            Ex.IfThenElse(
                                Ex.MakeBinary(ExpressionType.OrElse,
                                    Ex.Call(input, tryGet, Ex.Constant(x.Var.Name), tmp),
                                    Ex.Call(input, tryGet, Ex.Constant(ToPascalCase(x.Var.Name)), tmp)),
                                Ex.Block(
                                    Ex.Assign(x.Var, x.Converter.ApplyTo(tmp)),
                                    Ex.IfThen(Ex.Not(Ex.Property(x.Var, nameof(IConversionResult.IsSuccessful))),
                                        Ex.Goto(end, NoResult(to)))),
                                Ex.Assign(x.Var, Result(x.Type, Ex.Constant(x.Default, x.Type)))))
                        : Ex.Block(
                            Ex.IfThen(Ex.Not(Ex.Call(input, tryGet, Ex.Constant(x.Var.Name), tmp)),
                                Ex.IfThen(Ex.Not(Ex.Call(input, tryGet, Ex.Constant(ToPascalCase(x.Var.Name)), tmp)),
                                    Ex.Goto(end, NoResult(to)))),
                            Ex.Assign(x.Var, x.Converter.ApplyTo(tmp)),
                            Ex.IfThen(Ex.Not(Ex.Property(x.Var, nameof(IConversionResult.IsSuccessful))),
                                Ex.Goto(end, NoResult(to)))))),
                    Ex.Label(end, Result(to, Ex.New(ctor, pars.Select(p => Ex.Property(p.Var, nameof(IConversionResult.Result)))))));
            return Ex.Lambda(block, input);
        }

        public Delegate Create(Type from, Type to)
            => CreateLambda(from, to).Compile();

        #region Helper methods
        private Type GetDictionaryValueType(Type t)
            => (from itf in t.GetTypeInfo().ImplementedInterfaces.Concat(new[] { t })
                let ga = itf.GenericTypeArguments
                where ga.Length == 2 && ga[0] == typeof(string) && itf.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                select ga[1]).FirstOrDefault();
        private bool IsCompatibleDictionaryType(Type t)
        {
            var ti = t.GetTypeInfo();
            return ti.GenericTypeArguments.Length == 2
                && (ti.GetGenericTypeDefinition() == typeof(IDictionary<,>) || ti.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                && GetDictionaryValueType(t) != null;
        }
        private ConstructorInfo GetConstructorForType(Type t)
        {
            var ti = t.GetTypeInfo();
            var props = ti.Iterate(x => x.BaseType?.GetTypeInfo()).TakeWhile(x => x != null).SelectMany(x => x.DeclaredProperties).ToArray();
            if (!props.All(pi => pi.CanRead && !pi.CanWrite))
                return null;
            var ctor = from ci in ti.DeclaredConstructors
                       let ps = ci.GetParameters()
                       where ps.Length == props.Count()
                        && ps.Join(props, p => p.Name, p => p.Name, (_, __) => 1, CaseInsensitiveEqualityComparer.Instance).Count() == ps.Length
                       select ci;
            return ctor.FirstOrDefault();
        }
        private ConstructorInfo GetParameterlessConstructor(Type t)
            => t.GetTypeInfo().DeclaredConstructors.Where(ci => ci.GetParameters().Length == 0).FirstOrDefault();

        private IReadOnlyCollection<PropertyInfo> GetWritablePropertiesForType(Type t)
            => t.GetTypeInfo().Iterate(x => x.BaseType?.GetTypeInfo()).TakeWhile(x => x != null)
                .SelectMany(x => x.DeclaredProperties)
                .Where(pi => pi.CanWrite)
                .ToArray();
        private IReadOnlyCollection<PropertyInfo> GetReadablePropertiesForType(Type t)
            => t.GetTypeInfo().Iterate(x => x.BaseType?.GetTypeInfo()).TakeWhile(x => x != null)
                .SelectMany(x => x.DeclaredProperties)
                .Where(pi => pi.CanRead)
                .ToArray();
        private bool IsCompatibleObjectType(Type t)
            => GetConstructorForType(t) != null || GetParameterlessConstructor(t) != null && GetWritablePropertiesForType(t).Count > 0;
        private string ToPascalCase(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;
            var firstUpper = str[0].ToString().ToUpperInvariant()[0];
            if (firstUpper == str[0])
                return str;
            var ca = str.ToCharArray();
            ca[0] = firstUpper;
            return new string(ca);
        }

        private string ToCamelCase(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;
            var firstUpper = str[0].ToString().ToLowerInvariant()[0];
            if (firstUpper == str[0])
                return str;
            var ca = str.ToCharArray();
            ca[0] = firstUpper;
            return new string(ca);
        }
        #endregion
    }
}
