using Biz.Morsink.DataConvert.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Ex = System.Linq.Expressions.Expression;
using static Biz.Morsink.DataConvert.DataConvertUtils;
using System.Runtime.InteropServices;
#if NET45 || STD_2_0 || STD_1_6
using System.Dynamic;
#endif

namespace Biz.Morsink.DataConvert.Converters
{
    /// <summary>
    /// The RecordConverter converts back and forth between abstract 'records' and data class supporting either the getter/setter pattern or the readonly properties with a constructor parameter for each property pattern.
    /// </summary>
    public class RecordConverter : IConverter, IDataConverterRef
    {
        #region Helper types
        /// <summary>
        /// Interface for creating a 'Record' adapter for certain types.
        /// </summary>
        public interface IRecordCreator
        {
            bool CanConvertToRecord { get; }
            bool CanConvertFromRecord { get; }
            /// <summary>
            /// Determines if the type is compatible.
            /// </summary>
            /// <param name="t">The type to check.</param>
            /// <returns>true if the type is supported for creating IRecord&lt;T&gt; instances for</returns>
            bool IsTypeCompatible(Type t);
            /// <summary>
            /// Gets the type for the generic parameter for the IRecord interface for a certain type.
            /// Only types that have IsTypeCompatible(t) == true.
            /// </summary>
            /// <param name="t">The type to check the value type for.</param>
            /// <returns>The valuetype of the record.</returns>
            Type GetValueType(Type t);
            /// <summary>
            /// Creates a LambdaExpression which creates an IRecord&lt;T&gt; instance based on some object.
            /// </summary>
            /// <param name="t">The type to create an IRecord&ltT&gt; for.</param>
            /// <returns>A LambdaExpression that can create a new IRecord&ltT&gt; instance.</returns>
            LambdaExpression Creator(Type t);
        }
        /// <summary>
        /// Interface for getting and setting value in a string keyed record structure.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public interface IRecord<T>
        {
            /// <summary>
            /// Tries to get the value corresponding to some key.
            /// </summary>
            /// <param name="key">The key to get the value for.</param>
            /// <param name="value">Outputs the value that corresponds to the key.</param>
            /// <returns>True if the lookup succeeded, false otherwise.</returns>
            bool TryGetValue(string key, out T value);
            /// <summary>
            /// Sets a value for a certain key.
            /// </summary>
            /// <param name="key">The key to set the value for.</param>
            /// <param name="value">The value to set.</param>
            void SetValue(string key, T value);
        }
        /// <summary>
        /// Implementation of IRecordCreator for (I)Dictionary&lt;string, T&gt;.
        /// </summary>
        public class DictionaryRecordCreator : IRecordCreator
        {
            /// <summary>
            /// Singleton instance.
            /// </summary>
            public static DictionaryRecordCreator Instance { get; } = new DictionaryRecordCreator();

            public bool CanConvertFromRecord => true;

            public bool CanConvertToRecord => true;

            public LambdaExpression Creator(Type t)
            {
                var valueType = GetValueType(t);
                var inner = Ex.Parameter(typeof(IDictionary<,>).MakeGenericType(typeof(string), valueType), "inner");

                return Ex.Lambda(
                    Ex.Condition(
                        Ex.Property(Ex.Convert(inner, typeof(ICollection<>).MakeGenericType(typeof(KeyValuePair<,>).MakeGenericType(typeof(string), valueType))),
                            nameof(ICollection<object>.IsReadOnly)),
                        Ex.Default(typeof(DictionaryRecord<>).MakeGenericType(valueType)),
                        Ex.New(
                            typeof(DictionaryRecord<>).MakeGenericType(valueType).GetTypeInfo().DeclaredConstructors.First(),
                            inner)), inner);
            }

            public Type GetValueType(Type t)
                => (from itf in t.GetTypeInfo().ImplementedInterfaces.Concat(new[] { t })
                    let ga = itf.GenericTypeArguments
                    where ga.Length == 2 && ga[0] == typeof(string) && itf.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                    select ga[1]).FirstOrDefault();

            public bool IsTypeCompatible(Type t)
            {
                var ti = t.GetTypeInfo();
#if NET45 || STD_2_0 || STD_1_6
                return t == typeof(ExpandoObject) 
                    || ti.GenericTypeArguments.Length == 2
                        && (ti.GetGenericTypeDefinition() == typeof(IDictionary<,>) || ti.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                        && GetValueType(t) != null;
#else
                return t.FullName == "System.Dynamic.ExpandoObject"
                    || ti.GenericTypeArguments.Length == 2
                        && (ti.GetGenericTypeDefinition() == typeof(IDictionary<,>) || ti.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                        && GetValueType(t) != null;
#endif
            }
        }
        /// <summary>
        /// Implementation of IRecord&lt;T&gt; for (I)Dictionary&lt;string, T&gt;.
        /// </summary>
        /// <typeparam name="T">The valuetype of the record.</typeparam>
        public class DictionaryRecord<T> : IRecord<T>
        {
            private readonly IDictionary<string, T> inner;
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="inner">The dictionary instance to expose the IRecord&lt;T&gt; interface for.</param>
            public DictionaryRecord(IDictionary<string, T> inner)
            {
                this.inner = inner;
            }

            public void SetValue(string key, T value)
                => inner[key] = value;

            public bool TryGetValue(string key, out T value)
                => inner.TryGetValue(key, out value);
        }
        /// <summary>
        /// Implementation of IRecordCreator for IReadOnlyDictionary&lt;string, T&gt;.
        /// </summary>
        public class ReadOnlyDictionaryRecordCreator : IRecordCreator
        {
            /// <summary>
            /// Singleton instance.
            /// </summary>
            public static ReadOnlyDictionaryRecordCreator Instance { get; } = new ReadOnlyDictionaryRecordCreator();

            public bool CanConvertFromRecord => true;

            public bool CanConvertToRecord => false;

            public LambdaExpression Creator(Type t)
            {
                var valueType = GetValueType(t);
                var inner = Ex.Parameter(typeof(IReadOnlyDictionary<,>).MakeGenericType(typeof(string), valueType), "inner");
                return Ex.Lambda(
                    Ex.New(
                        typeof(ReadOnlyDictionaryRecord<>).MakeGenericType(valueType).GetTypeInfo().DeclaredConstructors.First(),
                        inner), inner);
            }

            public Type GetValueType(Type t)
                => (from itf in t.GetTypeInfo().ImplementedInterfaces.Concat(new[] { t })
                    let ga = itf.GenericTypeArguments
                    where ga.Length == 2 && ga[0] == typeof(string) && itf.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)
                    select ga[1]).FirstOrDefault();

            public bool IsTypeCompatible(Type t)
                => GetValueType(t) != null;
        }
        /// <summary>
        /// Implementation of IRecord&lt;T&gt; for IReadOnlyDictionary&lt;string, T&gt;.
        /// </summary>
        /// <typeparam name="T">The valuetype of the record.</typeparam>
        public class ReadOnlyDictionaryRecord<T> : IRecord<T>
        {
            private readonly IReadOnlyDictionary<string, T> inner;
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="inner">The dictionary instance to expose the IRecord&lt;T&gt; interface for.</param>
            public ReadOnlyDictionaryRecord(IReadOnlyDictionary<string, T> inner)
            {
                this.inner = inner;
            }

            public void SetValue(string key, T value)
                => throw new NotSupportedException();
            public bool TryGetValue(string key, out T value)
                => inner.TryGetValue(key, out value);
        }
#endregion

        private readonly IRecordCreator recordCreator;

        // TODO: Implement case sensitive/insensitive keys for the dictionary type
        // public static DictionaryObjectConverter CaseSensitive => new DictionaryObjectConverter();
        // public static DictionaryObjectConverter CaseInsensitive = new DictionaryObjectConverter(CaseInsensitiveEqualityComparer.Instance);
        // public DictionaryObjectConverter(IEqualityComparer<string> keyComparer = null)
        // {
        //     KeyComparer = keyComparer ?? EqualityComparer<string>.Default; ;
        // }
        // public IEqualityComparer<string> KeyComparer { get; }
        /// <summary>
        /// Creates a RecordConverter for dictionary types.
        /// </summary>
        public static RecordConverter ForDictionaries() => new RecordConverter(DictionaryRecordCreator.Instance);
        /// <summary>
        /// Creates a RecordConverter for read only dictionary types.
        /// </summary>
        public static RecordConverter ForReadOnlyDictionaries() => new RecordConverter(ReadOnlyDictionaryRecordCreator.Instance);
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="recordCreator">A IRecordCreator instance that is to be used when converting.</param>
        public RecordConverter(IRecordCreator recordCreator)
        {
            this.recordCreator = recordCreator;
        }

        public bool SupportsLambda => true;

        public IDataConverter Ref { get; set; }

        public bool CanConvert(Type from, Type to)
            => recordCreator.CanConvertFromRecord && recordCreator.IsTypeCompatible(from) && IsCompatibleObjectType(to)
            || recordCreator.CanConvertToRecord && IsCompatibleObjectType(from) && recordCreator.IsTypeCompatible(to) && GetParameterlessConstructor(to) != null;

        public LambdaExpression CreateLambda(Type from, Type to)
        {
            if (recordCreator.IsTypeCompatible(from))
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
            var valueType = recordCreator.GetValueType(to);
            var recType = typeof(IRecord<>).MakeGenericType(valueType);
            var set = recType.GetTypeInfo().GetDeclaredMethod(nameof(IRecord<object>.SetValue));

            var input = Ex.Parameter(from, "input");
            var tmp = Ex.Parameter(typeof(ConversionResult<>).MakeGenericType(valueType), "tmp");
            var res = Ex.Parameter(typeof(IDictionary<,>).MakeGenericType(typeof(string),valueType), "res");
            var rec = Ex.Parameter(recType, "rec");
            var getters = GetReadablePropertiesForType(from);
            var converters = getters.Select(g => Ref.GetLambda(g.PropertyType, valueType));

            var end = Ex.Label(typeof(ConversionResult<>).MakeGenericType(to));
            var block = Ex.Block(new[] { tmp, res, rec },
                Ex.Assign(res, Ex.New(GetParameterlessConstructor(to))),
                Ex.Assign(rec, recordCreator.Creator(to).ApplyTo(res)),
                Ex.IfThen(Ex.MakeBinary(ExpressionType.Equal, rec, Ex.Default(rec.Type)), Ex.Goto(end, NoResult(to))),
                Ex.Block(getters.Zip(converters, (g, c) => new { g, c })
                    .Select(x =>
                        Ex.Block(
                            Ex.Assign(tmp, x.c.ApplyTo(Ex.Property(input, x.g))),
                            Ex.IfThenElse(Ex.Property(tmp, nameof(IConversionResult.IsSuccessful)),
                                Ex.Call(rec, set, Ex.Constant(x.g.Name), Ex.Property(tmp, nameof(IConversionResult.Result))),
                                Ex.Goto(end, NoResult(to)))))),
                Ex.Label(end, Result(to, Ex.Convert(res,to))));
            return Ex.Lambda(block, input);
        }

        private LambdaExpression setterLambda(Type from, Type to)
        {
            var valueType = recordCreator.GetValueType(from);
            var recType = typeof(IRecord<>).MakeGenericType(valueType);
            var tryGet = recType.GetTypeInfo().GetDeclaredMethod(nameof(IRecord<object>.TryGetValue));

            var input = Ex.Parameter(from, "input");
            var rec = Ex.Parameter(recType, "rec");
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
            var block = Ex.Block(new[] { tmp, res, rec }.Concat(pars.Select(x => x.Var)),
                Ex.Assign(res, Ex.New(ctor)),
                Ex.Assign(rec, recordCreator.Creator(from).ApplyTo(input)),
                Ex.Block(pars
                    .Select(x =>
                        Ex.Block(
                            Ex.IfThen(
                                Ex.MakeBinary(ExpressionType.OrElse,
                                    Ex.Call(rec, tryGet, Ex.Constant(x.Var.Name), tmp),
                                    Ex.Call(rec, tryGet, Ex.Constant(ToCamelCase(x.Var.Name)), tmp)),
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
            var valueType = recordCreator.GetValueType(from);
            var recType = typeof(IRecord<>).MakeGenericType(valueType);
            var tryGet = recType.GetTypeInfo().GetDeclaredMethod(nameof(IRecord<object>.TryGetValue));

            var input = Ex.Parameter(from, "input");
            var rec = Ex.Parameter(recType, "rec");
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
            var block = Ex.Block(new[] { rec }.Concat(pars.Select(x => x.Var)),
                Ex.Assign(rec, recordCreator.Creator(from).ApplyTo(input)),
                Ex.Block(new[] { tmp }, pars
                    .Select(x =>
                        x.Optional
                        ? Ex.Block(
                            Ex.IfThenElse(
                                Ex.MakeBinary(ExpressionType.OrElse,
                                    Ex.Call(rec, tryGet, Ex.Constant(x.Var.Name), tmp),
                                    Ex.Call(rec, tryGet, Ex.Constant(ToPascalCase(x.Var.Name)), tmp)),
                                Ex.Block(
                                    Ex.Assign(x.Var, x.Converter.ApplyTo(tmp)),
                                    Ex.IfThen(Ex.Not(Ex.Property(x.Var, nameof(IConversionResult.IsSuccessful))),
                                        Ex.Goto(end, NoResult(to)))),
                                Ex.Assign(x.Var, Result(x.Type, Ex.Constant(x.Default, x.Type)))))
                        : Ex.Block(
                            Ex.IfThen(Ex.Not(Ex.Call(rec, tryGet, Ex.Constant(x.Var.Name), tmp)),
                                Ex.IfThen(Ex.Not(Ex.Call(rec, tryGet, Ex.Constant(ToPascalCase(x.Var.Name)), tmp)),
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
        private ConstructorInfo GetConstructorForType(Type t)
        {
            var ti = t.GetTypeInfo();
            var props = ti.Iterate(x => x.BaseType?.GetTypeInfo())
                .TakeWhile(x => x != null)
                .SelectMany(x => x.DeclaredProperties)
                .Where(p => !p.GetMethod.IsStatic)
                .ToArray();
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
                .Where(pi => pi.CanWrite && !pi.SetMethod.IsStatic && pi.GetIndexParameters().Length == 0)
                .ToArray();
        private IReadOnlyCollection<PropertyInfo> GetReadablePropertiesForType(Type t)
            => t.GetTypeInfo().Iterate(x => x.BaseType?.GetTypeInfo()).TakeWhile(x => x != null)
                .SelectMany(x => x.DeclaredProperties)
                .Where(pi => pi.CanRead && !pi.GetMethod.IsStatic && pi.GetIndexParameters().Length == 0)
                .ToArray();
        private bool IsCompatibleObjectType(Type t)
            => t != typeof(object) && GetConstructorForType(t) != null || GetParameterlessConstructor(t) != null && GetWritablePropertiesForType(t).Count > 0;
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
