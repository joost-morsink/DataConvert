using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.DataConvert.Converters
{
    /// <summary>
    /// Abstract base class for applying the decorator pattern to existing IConverters
    /// </summary>
    public abstract class DecoratedConverter : IConverter
    {
        protected DecoratedConverter(IConverter inner)
        {
            Inner = inner;
        }

        public IConverter Inner { get; }

        /// <summary>
        /// Implements additional type restrictions on the decorated converter.
        /// </summary>
        /// <param name="from">The type to convert from.</param>
        /// <param name="to">The type to convert to.</param>
        /// <returns>True if conversion is supported.</returns>
        public virtual bool OuterCanConvert(Type from, Type to) 
            => true;
        public bool CanConvert(Type from, Type to)
            => OuterCanConvert(from, to) && Inner.CanConvert(from, to);
        /// <summary>
        /// Decorates the resulting conversion delegate.
        /// </summary>
        /// <param name="del">The delegate from the decorated converter is passed in here.</param>
        /// <param name="from">The type to convert from.</param>
        /// <param name="to">The type to convert to.</param>
        /// <returns>A possibly decorated conversion delegate.</returns>
        public virtual Delegate DecorateDelegate(Delegate del, Type from, Type to)
            => del;
        public Delegate Create(Type from, Type to)
            => DecorateDelegate(Inner.Create(from, to), from, to);
    }
}
