using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
namespace Biz.Morsink.DataConvert
{
    /// <summary>
    /// This interface represents a component in the conversion pipeline.
    /// </summary>
    public interface IConverter
    {
        /// <summary>
        /// Should return whether this converter can provide an implementation for the conversion from 'from' to 'to'.
        /// </summary>
        /// <param name="from">The type to convert from.</param>
        /// <param name="to">The type to convert to.</param>
        /// <returns>True if this component can provider a conversion implementation.</returns>
        bool CanConvert(Type from, Type to);
        /// <summary>
        /// Should create a Delegate that provides an implementation from the conversion.
        /// </summary>
        /// <param name="from">The type to convert from.</param>
        /// <param name="to">The type to convert to.</param>
        /// <returns>A Func&lt;T, ConversionResult&lt;U&gt;&gt; that implements conversion.</returns>
        Delegate Create(Type from, Type to);
        /// <summary>
        /// True if this converter is able to return a LambdaExpression.
        /// </summary>
        bool SupportsLambda { get; }
        /// <summary>
        /// When supported creates a lambda expression for the conversion.
        /// </summary>
        /// <param name="from">The type to convert from.</param>
        /// <param name="to">The type to convert to.</param>
        /// <returns>When supported, a LambdaExpression that implements conversion. Otherwise null.</returns>
        LambdaExpression CreateLambda(Type from, Type to);
    }
}
