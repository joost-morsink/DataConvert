# Default pipeline

The default pipeline consists of the following components in order of occurence:

* [IdentityConverter](identity.md)
* [ToStringConverter](toString.md)
* [TryParseConverter](tryParse.md)
* [SimpleNumericConverter](simpleNum.md)
* [EnumParseConverter](enumParse.md)
* [ToNullableConverter](toNullable.md)
* [FromStringRepresentationConverter](fromStringRep.md) 
  * Restricted by a RestrictTypesConverter.
* [DynamicConverter](dynamic.md)


### FromStringRepresentationConverter restriction
The `FromStringRepresentationConverter` is restricted on its input type (not equal to `Version`).
The `System.Version` class, could be misinterpreted as a numeric value by the conversion pipeline if only major and minor parts are available.

* A version value of `1.1` is not equal to `1.10` (9 minors later), but when converted to a numeric type such as a `Double`, these values will be equal, although intermediate values are not equal.
* Also a value of `1.9` is smaller than `1.10`, but when converted `1.9` is larger than `1.10`.

A conversion which does not preserve ordering in some meaningful way is not desirable.
Combined with the fact that numeric conversions are a big part of this library, the `Version` class is excluded from the `FromStringRepresentationConverter` in the default pipeline.
It is technically possible to define some set of numeric types to weaken this restriction, but the set of all numeric types is open. 
For instance, .Net 4.0 introduced the `System.Numeric` namespace.
There is no guarantee that someone in the future will not introduce a new numeric type, that should be excluded from this conversion as well.
