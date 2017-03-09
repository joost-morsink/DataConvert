using System;
using System.Collections.Generic;
using System.Text;

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
        /// <returns>A Func&gt;T, ConversionResult&gt;U&lt;&lt; that implements conversion.</returns>
        Delegate Create(Type from, Type to);
    }
}
