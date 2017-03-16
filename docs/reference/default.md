# DefaultConvertConverter
This converter exposes the functionality of `System.Convert` to the conversion pipeline. 
Because exceptions are used by this class to indicate failure, it has a signifcant performance impact on unsuccesful conversions.
It is advised to either not use this component, or to configure it at the end of the pipeline.

The following criteria apply when searching for an applicable method:
* The method should be public and static
* The method's name should start with 'To'
* It should have either 1 or 2 parameters
  * The first should be the input type
  * The optional second should be an `IFormatProvider`
* The return type should correspond to the type to be converted to.

### Parameters
The DefaultConvertConverter class is configurable through the following constructor parameter:

| Parameter | Property | Type | Description | Default |
| --------- | -------- | ---- | ----------- | ------- |
| formatProvider | FormatProvider | IFormatProvider | The `IFormatProvider` to use as a method parameter | CultureInfo.InvariantCulture | 

The FormatProvider is only used for methods that take the parameter, like the methods dealing with `String`s and `Object`s.
