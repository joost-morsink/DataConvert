using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Biz.Morsink.DataConvert.Converters;

namespace Biz.Morsink.DataConvert
{
    /// <summary>
    /// This is the main implementation of the IDataConverter interface
    /// </summary>
    public class DataConverter : IDataConverter
    {
        private readonly IConverter[] _converters;
        private readonly ConcurrentDictionary<Tuple<Type, Type>, Entry> _entries;
        private class Entry
        {
            /// <summary>
            /// Creates an Entry based on the converter functions passed as a parameter. 
            /// </summary>
            public static Entry Make<T, U>(IEnumerable<Func<T, ConversionResult<U>>> fs)
            {
                var functions = fs.ToArray();
                var specific = new Func<T, ConversionResult<U>>(i =>
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
                return new Entry(generic, specific);
            }
            /// <summary>
            /// Uses reflection to call the generic Make method.
            /// </summary>
            public static Entry Create(Type from, Type to, IEnumerable<Delegate> delegates)
            {
                var ofType = typeof(Enumerable).GetTypeInfo().GetDeclaredMethod(nameof(Enumerable.OfType))
                                           .MakeGenericMethod(typeof(Func<,>).MakeGenericType(from, typeof(ConversionResult<>).MakeGenericType(to)));
                return (Entry)typeof(Entry).GetTypeInfo()
                                            .GetDeclaredMethod(nameof(Make))
                                            .MakeGenericMethod(from, to)
                                           .Invoke(null, new object[] { ofType.Invoke(null, new[] { delegates }) });
            }
            private Entry(Func<object, IConversionResult> generic, Delegate specific)
            {
                General = generic;
                Specific = specific;
            }
            public Delegate Specific { get; }
            public Func<object, IConversionResult> General { get; }
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

        private Entry getEntry(Type from, Type to)
            => _entries.GetOrAdd(Tuple.Create(from, to), tup => 
            {
                var converters = from converter in _converters
                                 where converter.CanConvert(tup.Item1, tup.Item2)
                                 select converter.Create(tup.Item1, tup.Item2);
                return Entry.Create(tup.Item1, tup.Item2, converters);
            });

        /// <summary>
        /// Gets a delegate that can try conversion from 'from' to 'to'
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
        /// Provides a way of getting/setting a default DataConverter, when that is needed for projects that don't actually want to use the dependency injection pattern.
        /// </summary>
        public static IDataConverter Default { get; set; } = new DataConverter(new IConverter[]
        {
            new IdentityConverter(),
            new ToStringConverter(true),
            new TryParseConverter(),
            new FromStringRepresentationConverter(),
            new DynamicConverter()
        });
    }
}
