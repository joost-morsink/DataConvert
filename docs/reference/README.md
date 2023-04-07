# Component Reference

The following `IConverter` components have been developed:

* [Base64Converter](base64.md)
* [Base64IntegerConverter](base64int.md)
* [Base85Converter](base85.md)
* [BooleanConverter](bool.md)
* [ConverterClassConverter<T>](convClass.md)
* [DefaultConvertConverter](default.md)
* [DynamicConverter](dynamic.md)
* [EnumerableToTupleConverter](enumerable2tuple.md)
* [EnumParseConverter](enumParse.md)
* [EnumToNumericConverter](enumToNum.md)
* [FromStringRepresentationConverter](fromStringRep.md)
* [IdentityConverter](identity.md)
* [ImplicitOperatorConverter](implOper.md)
* [IsoDateTimeConverter](isoDate.md)
* [LosslessStringToTupleConverter](llStr2Tup.md)
* [NumericToEnumConverter](numToEnum.md)
* [RecordConverter](rec.md)
* [SeparatedStringConverter](sepString.md)
* [SimpleNumericConverter](simpleNum.md)
* [StaticMethodConverter](staticMeth.md)
* [ToNullableConverter](toNullable.md)
* [ToObjectConverter](toObj.md)
* [ToStringConverter](toString.md)
* [TryParseConverter](tryParse.md)
* [TupleConverter](tuple.md)
* [TupleToArrayConverter](tuple2arr.md)


### Default pipeline
These components may or may not have been configured in the [default pipeline](pipeline.md). If another configuration is desired the `DataConverter` constructor can be called with a custom sequence of converters.

