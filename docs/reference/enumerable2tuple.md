# EnumerableToTupleConverter
This component converts from any enumerable type to a tuple type by converting the enumerated values one by one into component values for the tuple. 
It supports regular `System.Tuple` types as well as the new `System.ValueTuple` types. 
The enumerable should have _at least_ as many elements as the arity of the resulting tuple.

There are different levels of being 'enumerable':
* If a `IReadOnlyList<T>` is implemented, the `Count` property can be used to check if there are enough component values and the indexer can be used to directly access elements of the list.
* If a `IReadOnlyCollection<T>` is implemented, the `Count` property can be used to check if there are enough component values. 
  For the enumeration of elements, we will try some supported enumerator (specified below).
* If a enumerator pattern is implemented (A GetEnumerator() method that returns some type implementing bool MoveNext() method en Current property of any type), use that to enumerate over the values.
* If `IEnumerable<T>` is implemented, use to enumerate the elements.
* Otherwise use `IEnumerable`.

At the time of writing, the top 2 cases are not implemented and use the less efficient mechanisms of cases 3-5.

This converter is not configurable and has no constructor parameters.

This type supports the singleton pattern by implementation of a static property `Instance`.
