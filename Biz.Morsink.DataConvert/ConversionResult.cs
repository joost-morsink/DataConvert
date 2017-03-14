using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.DataConvert
{
    /// <summary>
    /// This struct represents the result of a conversion. It can either be a successful or a failed ConversionResult. 
    /// The default (parameterless) constructor creates a failed conversion.
    /// The other constructor creates a successful conversion.
    /// </summary>
    /// <typeparam name="T">The destination type of the conversion.</typeparam>
    public struct ConversionResult<T> : IConversionResult
    {
        /// <summary>
        /// Creates a successful conversion result.
        /// </summary>
        /// <param name="res">The result of the conversion</param>
        public ConversionResult(T res)
        {
            IsSuccessful = true;
            Result = res;
        }
        object IConversionResult.Result => Result;
        /// <summary>
        /// Contains the result of the conversion <b>iff</b> the conversion is successful (IsSuccessful == true).
        /// </summary>
        public T Result { get; }
        /// <summary>
        /// Indicates whether the conversion was successful. Inherited from IConversionResult.
        /// </summary>
        public bool IsSuccessful { get; }
        public override string ToString()
            => IsSuccessful ? Result?.ToString() : "<No result>";
    }
}
