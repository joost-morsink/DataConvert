using System;
using System.Collections.Generic;
using Ex = System.Linq.Expressions.Expression;
using System.Text;

namespace Biz.Morsink.DataConvert.Converters
{
    using static DataConvertUtils;
    /// <summary>
    /// This is the trivial converter: 
    /// if the types for source an destination types are the same, the converter should just return the input.
    /// </summary>
    public class IdentityConverter : IConverter
    {
        public bool CanConvert(Type from, Type to)
            => from == to;

        public Delegate Create(Type from, Type to)
        {
            var input = Ex.Parameter(from, "input");
            var block = Result(to, input);
            return Ex.Lambda(block, input).Compile();
        }
    }
}
