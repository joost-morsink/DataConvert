# TupleToArrayConverter
This component converts from some tuple typs to an array of the component values by converting them one by one.
It supports regular `System.Tuple` types as well as the new `System.ValueTuple` types. 

This converter is not configurable and has no constructor parameters.

This type supports the singleton pattern by implementation of a static property `Instance`.
