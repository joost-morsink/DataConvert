using System;

namespace Biz.Morsink.DataConvert.Converters
{
    /// <summary>
    /// Decorator converter.
    /// Restricts the type on which a converter can operate. 
    /// </summary>
    public class RestrictTypesConverter: DecoratedConverter 
    {
        public RestrictTypesConverter(IConverter inner, Func<Type,Type,bool> typeFilter) : base(inner)
        {
            TypeFilter = typeFilter;
        }

        public Func<Type, Type, bool> TypeFilter { get; }

        public override bool OuterCanConvert(Type from, Type to)
            => TypeFilter(from, to);
    }
}
