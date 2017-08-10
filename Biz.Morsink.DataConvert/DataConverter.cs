using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Ex = System.Linq.Expressions.Expression;
using System.Reflection;
using Biz.Morsink.DataConvert.Converters;
using Biz.Morsink.DataConvert.Helpers;
using static Biz.Morsink.DataConvert.DataConvertUtils;
namespace Biz.Morsink.DataConvert
{
    /// <summary>
    /// This is the main implementation of the IDataConverter interface
    /// </summary>
    public class DataConverter : IDataConverter, IConverter
    {
        private readonly IConverter[] _converters;
        private readonly ConcurrentDictionary<Tuple<Type, Type>, Entry> _entries;
        private class Entry
        {
            /// <summary>
            /// Creates an Entry based on the converter functions passed as a parameter. 
            /// </summary>
            public static Entry Make<T, U>(IEnumerable<Tuple<LambdaExpression, Delegate>> fs)
            {
                var subs = fs.Select(t => Tuple.Create(t.Item1, (Func<T, ConversionResult<U>>)t.Item2)).ToArray();
                var functions = subs.Select(f => f.Item2).ToArray();
                var specific = functions.Length == 0
                    ? _ => new ConversionResult<U>()
                    : functions.Length == 1
                        ? functions[0]
                        : new Func<T, ConversionResult<U>>(i =>
                        {
                            ConversionResult<U> res;
                            for (int j = 0; j < functions.Length; j++)
                            {
                                res = functions[j](i);
                                if (res.IsSuccessful)
                                    return res;
                            }
                            return new ConversionResult<U>();
                        });
                var generic = new Func<object, IConversionResult>(i => specific((T)i));
                var input = Ex.Parameter(typeof(T), "input");
                var result = Ex.Parameter(typeof(ConversionResult<U>), "result");
                var end = Ex.Label(typeof(ConversionResult<U>), "end");
                var lambda = subs.Length == 0
                    ? (Expression<Func<T, ConversionResult<U>>>)(_ => default(ConversionResult<U>))
                    : subs.Length == 1
                        ? subs[0].Item1 ?? funcToLambda(subs[0].Item2)
                        : Ex.Lambda(Ex.Block(new[] { result },
                            Ex.Block(subs.Select(sub =>
                                Ex.Block(
                                    Ex.Assign(result, (sub.Item1 ?? funcToLambda(sub.Item2)).ApplyTo(input)),
                                    Ex.IfThen(
                                        Ex.Property(result, nameof(IConversionResult.IsSuccessful)),
                                        Ex.Goto(end, result))))),
                            Ex.Label(end, NoResult(typeof(U)))), input);
                return new Entry(generic, specific, lambda);
                LambdaExpression funcToLambda(Func<T, ConversionResult<U>> f)
                    => (Expression<Func<T, ConversionResult<U>>>)(inp => f(inp));
            }
            /// <summary>
            /// Uses reflection to call the generic Make method.
            /// </summary>
            public static Entry Create(Type from, Type to, IEnumerable<Tuple<LambdaExpression, Delegate>> delegates)
            {
                return (Entry)typeof(Entry).GetTypeInfo()
                                            .GetDeclaredMethod(nameof(Make))
                                            .MakeGenericMethod(from, to)
                                           .Invoke(null, new object[] { delegates });
            }
            private Entry(Func<object, IConversionResult> generic, Delegate specific, LambdaExpression lambda)
            {
                General = generic;
                Specific = specific;
                Lambda = lambda;
            }
            public Delegate Specific { get; }
            public Func<object, IConversionResult> General { get; }
            public LambdaExpression Lambda { get; }
        }

        /// <summary>
        /// Constructs a DataConverter based on the provided IConverters
        /// </summary>
        /// <param name="converters">A sequence of converters that will make up the conversion pipeline.</param>
        public DataConverter(IEnumerable<IConverter> converters)
        {
            _converters = converters.ToArray();
            foreach (var r in _converters.OfType<IDataConverterRef>())
                r.Ref = this;
            _entries = new ConcurrentDictionary<Tuple<Type, Type>, Entry>();
        }
        public DataConverter(params IConverter[] converters) : this(converters.AsEnumerable())
        {
        }

        private Entry getEntry(Type from, Type to)
            => _entries.GetOrAdd(Tuple.Create(from, to), tup =>
            {
                var converters = from converter in _converters
                                 where converter.CanConvert(tup.Item1, tup.Item2)
                                 select Tuple.Create(converter.CreateLambda(tup.Item1, tup.Item2), converter.Create(tup.Item1, tup.Item2));
                return Entry.Create(tup.Item1, tup.Item2, converters);
            });

        /// <summary>
        /// Gets a delegate that can try conversion from 'from' to 'to'.
        /// </summary>
        /// <param name="from">The type that is converted from</param>
        /// <param name="to">The type that is converted to</param>
        /// <returns>A Func&gt;T, ConversionResult&gt;U&lt;&lt; where typeof(T) == from and typeof(U) == to</returns>
        public Delegate GetConverter(Type from, Type to)
        {
            var entry = getEntry(from, to);
            return entry.Specific;
        }
        /// <summary>
        /// Gets a delegate that can try conversion from 'from' to 'to'.
        /// </summary>
        /// <param name="from">The type that is converted from</param>
        /// <param name="to">The type that is converted to</param>
        /// <returns>A Func&gt;object, IConversionResult&lt; that <b>will cast</b> the input object to the type 'from'.</returns>
        public Func<object, IConversionResult> GetGeneralConverter(Type from, Type to)
        {
            var entry = getEntry(from, to);
            return entry.General;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public LambdaExpression GetLambda(Type from, Type to)
        {
            var entry = getEntry(from, to);
            return entry.Lambda;
        }
        /// <summary>
        /// Provides a way of getting/setting a default DataConverter, when that is needed for projects that don't actually want to use the dependency injection pattern.
        /// </summary>
        public static IDataConverter Default { get; set; } = CreateDefault();

        public bool SupportsLambda => true; // _converters.All(c => c.SupportsLambda);

        public static IDataConverter CreateDefault() =>
            new DataConverter(
                IdentityConverter.Instance,
                IsoDateTimeConverter.Instance,
                Base64Converter.Instance,
                new ToStringConverter(true),
                new TryParseConverter().Restrict((from, to) => to != typeof(bool)), // bool parsing has a custom converter in pipeline
                EnumToNumericConverter.Instance,
                SimpleNumericConverter.Instance,
                BooleanConverter.Instance,
                new NumericToEnumConverter(),
                EnumParseConverter.CaseInsensitive,
                new ToNullableConverter(),
                TupleConverter.Instance,
                ToObjectConverter.Instance,
                new FromStringRepresentationConverter().Restrict((from, to) => from != typeof(Version)), // Version could conflict with numeric types' syntaxes.
                new DynamicConverter()
            );

        bool IConverter.CanConvert(Type from, Type to)
            => _converters.Any(c => c.CanConvert(from, to));

        Delegate IConverter.Create(Type from, Type to)
            => GetConverter(from, to);

        LambdaExpression IConverter.CreateLambda(Type from, Type to)
            => GetLambda(from, to);


    }
}
