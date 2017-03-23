using System;
using System.Collections.Generic;
using System.Text;
using Ex = System.Linq.Expressions.Expression;
using Et = System.Linq.Expressions.ExpressionType;
using static Biz.Morsink.DataConvert.DataConvertUtils;

namespace Biz.Morsink.DataConvert.Converters
{
    /// <summary>
    /// A converter to take care of numerical conversions.
    /// Both the 'from' and 'to' types should be .Net framework numeric types. 
    /// They should also both be signed; unsigned types are not supported by this component.
    /// </summary>
    public class SimpleNumericConverter : IConverter
    {
        /// <summary>
        /// Indicates what kind of numeric a type is.
        /// </summary>
        private enum NumericKind { Integer = 0, Decimal = 1, Floating = 2 };
        /// <summary>
        /// A structure containing all important information (related to conversion) of a numeric type.
        /// </summary>
        private struct NumericInfo : IComparable<NumericInfo>, IEquatable<NumericInfo>
        {
            public NumericInfo(int size, NumericKind kind, object minValue, object maxValue)
            {
                Size = size;
                Kind = kind;
                MinValue = minValue;
                MaxValue = maxValue;
            }
            public int Size { get; }
            public NumericKind Kind { get; }
            public object MinValue { get; }
            public object MaxValue { get; }

            public int CompareTo(NumericInfo other)
            {
                if (Kind < other.Kind)
                    return -1;
                else if (Kind > other.Kind)
                    return 1;
                else
                    return Size.CompareTo(other.Size);
            }
            public static bool operator <(NumericInfo left, NumericInfo right)
                => left.CompareTo(right) < 0;
            public static bool operator >(NumericInfo left, NumericInfo right)
                => left.CompareTo(right) > 0;
            public static bool operator <=(NumericInfo left, NumericInfo right)
                => left.CompareTo(right) <= 0;
            public static bool operator >=(NumericInfo left, NumericInfo right)
                => left.CompareTo(right) >= 0;
            public static bool operator ==(NumericInfo left, NumericInfo right)
                => left.CompareTo(right) == 0;
            public static bool operator !=(NumericInfo left, NumericInfo right)
                => left.CompareTo(right) != 0;
            public override int GetHashCode()
                => ((int)Kind << 8) + Size;
            public override bool Equals(object obj)
                => obj is NumericInfo && Equals((NumericInfo)obj);

            public bool Equals(NumericInfo other)
                => Size == other.Size && Kind == other.Kind;
        }
        private static Dictionary<Type, NumericInfo> infos = new Dictionary<Type, NumericInfo>
        {
            [typeof(sbyte)] = new NumericInfo(1, NumericKind.Integer, sbyte.MinValue, sbyte.MaxValue),
            [typeof(short)] = new NumericInfo(2, NumericKind.Integer, short.MinValue, short.MaxValue),
            [typeof(int)] = new NumericInfo(4, NumericKind.Integer, int.MinValue, int.MaxValue),
            [typeof(long)] = new NumericInfo(8, NumericKind.Integer, long.MinValue, long.MaxValue),
            [typeof(float)] = new NumericInfo(4, NumericKind.Floating, float.MinValue, float.MaxValue),
            [typeof(double)] = new NumericInfo(8, NumericKind.Floating, double.MinValue, double.MaxValue),
            [typeof(decimal)] = new NumericInfo(16, NumericKind.Decimal, decimal.MinValue, decimal.MaxValue)
        };
        /// <summary>
        /// Gets a singleton SimpleNumericConverter.
        /// </summary>
        public static SimpleNumericConverter Instance { get; } = new SimpleNumericConverter();

        private static bool isNumeric(Type t)
            => infos.ContainsKey(t);
        public bool CanConvert(Type from, Type to)
            => isNumeric(from) && isNumeric(to);

        public Delegate Create(Type from, Type to)
        {
            var input = Ex.Parameter(from, "input");
            var fromInfo = infos[from];
            var toInfo = infos[to];
            if (fromInfo <= toInfo) // Can make use of an implicit conversion
            {
                var block = Result(to, Ex.Convert(input, to));
                var lambda = Ex.Lambda(block, input);
                return lambda.Compile();
            }
            else // Cannot make use of an implicit conversion, bounds must be checked. Precision might be lost.
            {
                var block = Ex.Condition(
                    Ex.MakeBinary(Et.AndAlso,
                        Ex.MakeBinary(Et.GreaterThanOrEqual,
                            input,
                            Ex.Convert(Ex.Constant(toInfo.MinValue), from)),
                        Ex.MakeBinary(Et.LessThanOrEqual,
                            input,
                            Ex.Convert(Ex.Constant(toInfo.MaxValue), from))),
                    Result(to, Ex.Convert(input, to)),
                    NoResult(to));
                var lambda = Ex.Lambda(block, input);
                return lambda.Compile();
            }
        }
    }
}
