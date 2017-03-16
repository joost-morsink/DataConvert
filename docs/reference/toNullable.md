# ToNullableConverter
This converter always succeeds by first trying the conversion to the non-nullable type. 
When that fails, this converter succeeds with a 'null'-value `default(Nullable<To>)`.
It transfers the possible failure aspect into a .Net native construct.

This converter cannot be configured and has no parameters.
