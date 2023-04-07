namespace Biz.Morsink.DataConvert
{
    /// <summary>
    /// Represents the interface of conversion results.
    /// </summary>
    public interface IConversionResult
    {
        /// <summary>
        /// Contains the result <b>iff</b> the conversion was successful, otherwise this value is not determined.
        /// </summary>
        object Result { get; }

        /// <summary>
        /// Indicates whether the conversion was successful.
        /// </summary>
        bool IsSuccessful { get; }
    }
}
