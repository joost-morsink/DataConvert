using System;
using System.Linq.Expressions;

namespace Biz.Morsink.DataConvert
{
    /// <summary>
    /// This interface specifies the implementation needed for a generic data converter.
    /// </summary>
    public interface IDataConverter
    {
        /// <summary>
        /// Gets a delegate that can try conversion from 'from' to 'to'
        /// </summary>
        /// <param name="from">The type that is converted from</param>
        /// <param name="to">The type that is converted to</param>
        /// <returns>A Func&gt;T, ConversionResult&gt;U&lt;&lt; where typeof(T) == from and typeof(U) == to</returns>
        Delegate GetConverter(Type from, Type to);
        /// <summary>
        /// Gets a delegate that can try conversion from 'from' to 'to'.
        /// </summary>
        /// <param name="from">The type that is converted from</param>
        /// <param name="to">The type that is converted to</param>
        /// <returns>A Func&gt;object, IConversionResult&lt; that <b>will cast</b> the input object to the type 'from'.</returns>
        Func<object, IConversionResult> GetGeneralConverter(Type from, Type to);
        /// <summary>
        /// Gets a lambda expression that can try conversion from 'from' to 'to'.
        /// </summary>
        /// <param name="from">The type that is converted from.</param>
        /// <param name="to">The type that is converted to.</param>
        /// <returns>A LambdaExpression that contains the conversion implementation.</returns>
        LambdaExpression GetLambda(Type from, Type to);
    }
}
