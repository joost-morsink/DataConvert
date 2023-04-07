using System;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using Ex = System.Linq.Expressions.Expression;
using static Biz.Morsink.DataConvert.DataConvertUtils;
namespace Biz.Morsink.DataConvert.Converters
{
    /// <summary>
    /// This class supports the conversion of numeric integral types from and to string.
    /// </summary>
    public class Base64IntegerConverter : IConverter
    {
        /// <summary>
        /// The default set of characters for BASE64 encoding
        /// </summary>
        public static readonly char[] BASE64_STANDARD = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".ToCharArray();
        /// <summary>
        /// A default set of URL-safe characters for BASE64 encoding (see RFC 4648 §5)
        /// </summary>
        public static readonly char[] BASE64_URL = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_".ToCharArray();

        private char[] _mapChars;
        private byte[] _invMap;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mapChars">The encoding characters to use.</param>
        public Base64IntegerConverter(char[] mapChars = null)
        {
            mapChars = mapChars ?? BASE64_STANDARD;
            if (mapChars.Length != 64)
                throw new ArgumentOutOfRangeException(nameof(mapChars), "mapChars should have length 64.");
            if (mapChars.Any(c => c > 0x7f))
                throw new ArgumentOutOfRangeException(nameof(mapChars), "mapChars should only contain 7-bit ASCII characters.");
            if (mapChars.Distinct().Count() != 64)
                throw new ArgumentOutOfRangeException(nameof(mapChars), "mapChars should consist of unique characters.");
            _mapChars = mapChars;
            _invMap = new byte[128];
            for (int i = 0; i < _invMap.Length; i++)
                _invMap[i] = 0xff;
            for (int i = 0; i < mapChars.Length; i++)
                _invMap[mapChars[i]] = (byte)i;
        }
        public bool SupportsLambda => true;
        private bool isNumeric(Type t)
            => t == typeof(short) || t == typeof(ushort) 
            || t == typeof(int) ||  t == typeof(uint)
            || t == typeof(long) || t==typeof(ulong)
            || t == typeof(BigInteger);

        public bool CanConvert(Type from, Type to)
            => isNumeric(from) && to == typeof(string) || from == typeof(string) && isNumeric(to);

        public Delegate Create(Type from, Type to)
            => CreateLambda(from, to).Compile();
        
        private Type workingType(Type from)
        {
            switch (from.Name)
            {
                case "Int16":
                    return typeof(ushort);
                case "Int32":
                    return typeof(uint);
                case "Int64":
                    return typeof(ulong);
                default:
                    return from;
            }
        }
        private int charbound(Type from)
        {
            switch (from.Name)
            {
                case "Int16":
                case "Uint16":
                    return 3;
                case "Int32":
                case "Uint32":
                    return 6;
                case "Int64":
                case "Uint64":
                    return 11;
                default:
                    return 128;
            }
        }

        // Pseudocode:
        //I input => 
        //{
        //  var i = (UI)input;
        //  var result = new char[base64sizeof(UI)];
        //  for (int j = 0; j == 0 || i > 0; j++)
        //  {
        //      result[j] = _mapChar[i & 0x3f];
        //      i >>= 6;
        //  }
        //}
        private LambdaExpression toLambda(Type from)
        {
            var input = Ex.Parameter(from, "input");
            var result = Ex.Parameter(typeof(char[]), "result");
            var i = workingType(from) == from ? input : Ex.Parameter(workingType(from), "i");
            var j = Ex.Parameter(typeof(int), "j");
            var loopstart = Ex.Label("loopstart");
            var loopend = Ex.Label("loopend");
            
            var loop = Ex.Block(
                Ex.Label(loopstart),
                Ex.IfThen(Ex.MakeBinary(ExpressionType.AndAlso,
                    Ex.MakeBinary(ExpressionType.GreaterThan, j, Ex.Constant(0)),
                    i.Type == typeof(BigInteger)
                        ? (Ex)Ex.Call(i, nameof(BigInteger.Equals), Type.EmptyTypes, Ex.Constant(BigInteger.Zero))
                        : Ex.MakeBinary(ExpressionType.Equal, i,  Ex.Convert(Ex.Constant(0), i.Type))),
                    Ex.Goto(loopend)),
                Ex.Assign(
                    Ex.ArrayAccess(result, j),
                    Ex.ArrayIndex(Ex.Constant(_mapChars),
                        Ex.Convert(Ex.MakeBinary(ExpressionType.And, i, Ex.Convert(Ex.Constant(0x3f), i.Type)), typeof(int)))),
                Ex.RightShiftAssign(i, Ex.Constant(6)),
                Ex.PostIncrementAssign(j),
                Ex.Goto(loopstart));
            var ret = Result(typeof(string),
                    Ex.New(typeof(string).GetTypeInfo().DeclaredConstructors
                            .Select(c => new { c, p = c.GetParameters() })
                            .First(c => c.p.Length == 3 && c.p[0].ParameterType == typeof(char[]) && c.p[1].ParameterType == typeof(int) && c.p[2].ParameterType == typeof(int)).c,
                        result, Ex.Constant(0), j));
            var block = Ex.Block(Ex.Assign(j, Ex.Constant(0)),
                    Ex.Assign(result, Ex.NewArrayBounds(typeof(char), Ex.Constant(charbound(from)))),
                    loop,
                    Ex.Label(loopend),
                    ret);
            block = input == i
                ? Ex.Block(new[] { j, result },
                    block)
                : Ex.Block(new[] { i, j, result },
                    Ex.Assign(i, Ex.Convert(input, i.Type)),
                    block);
            return Ex.Lambda(block, input);
        }
        // Pseudocode:
        //string input =>
        //{
        //    R result = 0;
        //    for (int i = input.Length - 1; i >= 0; i--)
        //    {
        //        result <<= 6;
        //        var m = _invMap[input[i]];
        //        if (m == 0xff)
        //            return default(ConversionResult<R>);
        //        result += m;
        //    }
        //    return new ConversionResult<R>(result);
        //}
        private LambdaExpression fromLambda(Type to)
        {
            var stringthis = typeof(string).GetTypeInfo().DeclaredProperties.First(p => p.GetIndexParameters().Length == 1 && p.GetIndexParameters()[0].ParameterType == typeof(int));
            var input = Ex.Parameter(typeof(string), "input");
            var result = Ex.Parameter(to, "result");
            var i = Ex.Parameter(typeof(int), "i");
            var m = Ex.Parameter(typeof(byte), "m");
            var loopstart = Ex.Label("loopstart");
            var end = Ex.Label(typeof(ConversionResult<>).MakeGenericType(to), "end");
            var loop = Ex.Block(
                Ex.Label(loopstart),
                Ex.IfThen(Ex.MakeBinary(ExpressionType.LessThan, i, Ex.Constant(0)),
                    Ex.Goto(end, Result(to, result))),
                Ex.LeftShiftAssign(result, Ex.Constant(6)),
                Ex.Assign(m, Ex.ArrayIndex(Ex.Constant(_invMap), Ex.Convert(Ex.MakeIndex(input, stringthis, new[] { i }),typeof(int)))),
                Ex.IfThen(Ex.MakeBinary(ExpressionType.Equal, m, Ex.Constant((byte)0xff)),
                    Ex.Goto(end, NoResult(to))),
                Ex.AddAssign(result, Ex.Convert(m, result.Type)),
                Ex.PostDecrementAssign(i),
                Ex.Goto(loopstart));
            var block = Ex.Block(new[] { result, i, m },
                Ex.Assign(result, Ex.Convert(Ex.Constant(0), to)),
                Ex.Assign(i, Ex.MakeBinary(ExpressionType.Subtract, Ex.Property(input, nameof(string.Length)), Ex.Constant(1))),
                loop,
                Ex.Label(end, NoResult(to)));
            return Ex.Lambda(block, input);
        }

        
        public LambdaExpression CreateLambda(Type from, Type to)
        {
            if (from == typeof(string))
                return fromLambda(to);
            else
                return toLambda(from);
        }
    }
}
