using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Biz.Morsink.DataConvert.Converters
{
    /// <summary>
    /// This converter uses the Base-85 encoding to convert back and forth between strings and byte arrays.
    /// </summary>
    public class Base85Converter : IConverter
    {
        public bool SupportsLambda => true;
        /// <summary>
        /// Contains a Base85Converter using the 'standard' set of ASCII characters.
        /// </summary>
        public static Base85Converter Default { get; } = new Base85Converter(Enumerable.Range(33, 85).Select(i => (char)i).ToArray());

        private char[] characters;
        private uint[] values;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="characters">An array of characters of length 85 containing the characters to use for the encoding.</param>
        public Base85Converter(char[] characters)
        {
            if (characters.Length != 85)
                throw new ArgumentOutOfRangeException("Length of array should be 85.");
            if (characters.Any(c => c >= 128))
                throw new ArgumentOutOfRangeException("All characters should be (7-bit) ASCII.");
            if (characters.Distinct().Count() < 85)
                throw new ArgumentOutOfRangeException("All characters must be unique.");
            this.characters = characters;
            values = new uint[128];
            for (int i = 0; i < 128; i++)
                values[i] = 0xffffffff;
            for (uint i = 0; i < 85; i++)
                values[characters[i]] = i;
        }

        public bool CanConvert(Type from, Type to)
            => from == typeof(string) && to == typeof(byte[])
            || from == typeof(byte[]) && to == typeof(string);

        public Delegate Create(Type from, Type to)
            => CreateLambda(from, to).Compile();

        public LambdaExpression CreateLambda(Type from, Type to)
            => from == typeof(string)
            ? (LambdaExpression)(Expression<Func<string, ConversionResult<byte[]>>>)(input => Parse(input))
            : (Expression<Func<byte[], ConversionResult<string>>>)(input => Encode(input));

        private ConversionResult<byte[]> Parse(string str)
        {
            var q = str.Length / 5; // Number of 5 character blocks
            var r = str.Length % 5; // Last blocklength
            var result = new byte[q * 4 + (r == 0 ? 0 : r - 1)]; // 4 bytes per 5 character block. 1 byte per character more than a single one in the last block.
            var respos = 0;

            for (var pos = 0; pos < str.Length; pos += 5, respos += 4)
            {
                var len = Math.Min(str.Length - pos, 5);
                if (len == 1)
                    return new ConversionResult<byte[]>(); // A partial 32bit block never encodes to a single character.
                uint val = 0;
                for (var i = 0; i < len; i++)
                {
                    var add = values[str[pos + i]];
                    if (add > 85)
                        return new ConversionResult<byte[]>(); // An invalid character was encountered.
                    val *= 85;
                    val += add;
                }
                for (var i = 0; i < len - 1; i++)
                    result[respos + len - i - 2] = (byte)(val >> (i * 8));  // In reverse to preserve byte/character order.
            }
            return new ConversionResult<byte[]>(result);
        }
        private ConversionResult<string> Encode(byte[] arr)
        {
            var sb = new StringBuilder();
            var res = new char[5];
            for (var pos = 0; pos < arr.Length; pos += 4)
            {
                var len = Math.Min(arr.Length - pos, 4);
                uint val = 0;
                for (int i = 0; i < len; i++)
                {
                    val <<= 8;
                    val += arr[pos + i];
                }

                for (int i = 0; i < len + 1; i++)
                {
                    var rem = val % 85;
                    res[4 - i] = characters[rem]; // In reverse to preserve byte/character order.
                    val /= 85;
                }
                sb.Append(res, 4 - len, len + 1);
            }
            return new ConversionResult<string>(sb.ToString());
        }
    }
}
