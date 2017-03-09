# DataConvert

This library provides a uniform interface for the conversion of values from some datatype to another.
It is extensible and flexible. 
When provided with performance-tuned implementations it should also be fast. 

The default way for type conversion in .Net is through the `System.Convert` class. 
This class is implemented on top of the `IConvertible` interface and uses a lot of typecasting, which in some use cases is unnecessary. 
It is also not flexible, nor extensible (except through the `IConvertible` interface).
The default method of indicating failure is throwing an exception, which could be a big performance hit.

The `System.Convert` allows for a lot of formatting options, which is a great idea for UI based applications. 
However, this library aims at datatype conversion at a backend level, not primarily to be consumed by users.

The repository contains:
* DataConvert solution (VS 2017)
  * DataConvert project
  * DataConvert.Test unit test project
* This [gitbook](http://www.gitbook.com) containing documentation for the project.
