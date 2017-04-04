using System;
using System.Collections.Generic;
using System.Text;
using Ex = System.Linq.Expressions.Expression;
using Et = System.Linq.Expressions.ExpressionType;
using static Biz.Morsink.DataConvert.DataConvertUtils;
namespace Biz.Morsink.DataConvert.Converters
{
    /// <summary>
    /// This class converts a strings in separated format to something that can be converted to from an array.
    /// TODO: Have this class convert enumerable/tuple types to a separated string.
    /// </summary>
    public class SeparatedStringConverter : IConverter, IDataConverterRef
    {
        public IDataConverter Ref { get; set; }
        /// <summary>
        /// An array containing possible separator characters.
        /// </summary>
        public char[] Separators { get; }

        public SeparatedStringConverter(params char[] separators)
        {
            Separators = separators;
        }
        public bool CanConvert(Type from, Type to)
            => from == typeof(string);// || from !=typeof(string) && to==typeof(string);

        public Delegate Create(Type from, Type to)
        {
            if (from == typeof(string))
            {
                return fromStringDelegate(to);
            }
            else
                /// TODO: Have this class convert enumerable/tuple types to a separated string.
                throw new ArgumentException("Arguments do not match a case for separated strings.");
        }

        private Delegate fromStringDelegate(Type to)
        {
            var input = Ex.Parameter(typeof(string), "input");
            var split = Ex.Parameter(typeof(string[]), "split");

            var block = Ex.Condition(Ex.MakeBinary(Et.Equal, input, Ex.Default(typeof(string))),
                    NoResult(to),
                    Ex.Block(new[] { split },
                        Ex.Assign(split, Ex.Call(input, nameof(string.Split), Type.EmptyTypes, Ex.Constant(Separators))),
                        Ex.Condition(Ex.MakeBinary(Et.Equal, Ex.Property(split, nameof(Array.Length)), Ex.Constant(1)),
                            NoResult(to),
                            Ex.Invoke(
                                Ex.Convert(
                                    Ex.Call(Ex.Constant(Ref), nameof(IDataConverter.GetConverter), Type.EmptyTypes, Ex.Constant(typeof(string[])), Ex.Constant(to)),
                                    typeof(Func<,>).MakeGenericType(typeof(string[]), typeof(ConversionResult<>).MakeGenericType(to))),
                                split))));
            var lambda = Ex.Lambda(block, input);
            return lambda.Compile();
        }
    }
}
