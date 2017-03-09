using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.DataConvert
{
    /// <summary>
    /// Provides an interface to inject a reference into an IConverter
    /// </summary>
    public interface IDataConverterRef
    {
        /// <summary>
        /// Sets the reference to the IDataConverter.
        /// </summary>
        IDataConverter Ref { set; }
    }
}
