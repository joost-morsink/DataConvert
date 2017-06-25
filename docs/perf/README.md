# Performance
The library has been designed so that performance loss is minimal, while also giving the developer the most generic api.
To measure the performance differences, a console application has been included to perform these measurements.
The measurement algorithm:
* tries to cancel out loop and call overhead.
* performs measurements in a configurable timespan.
* tries to include the time needed for the collection of generated garbage.

## Measurement
The tests measure a relative performance of an Action relative to a reference Action. 
The Action is defined as the conversion using DataConvert and the reference Action is defined as the same or equivalent conversion but done using the BCL methods directly.
This gives us an accurate measurement of hoe big the overhead is for using the DataConvert library in the case the from and to types are known **at compile time**.
Practically all measurements will therefore be negative, but I believe it gives us the most 'honest' statistics.
In practice, results may vary based on the specific use case.

The amount of iterations is measured within a preset timespan.
To cancel out any negative effects of checking the time, it is only checked at 'powers of 2'.
An external loop increments the power of 2, while the inner loop does that amount of iterations. 
If the timespan has elapsed after the loop, the number of actual iterations within the timespan is calculated on a linear basis. 

It is advised to have as little applications as possible running at the time of measurement, because they can all influence the measurement process.

## Loop and call overhead
On initialization a loop of 100 million iterations is done to measure how much time is in the loop overhead. 
This time is systematically subtracted from all future measurements.

## Tests
Included tests are:

| Description | Performance difference |
| ----------- | ---------------------- |
| string to int conversion | 30% slower |
| string to int? conversion | 10% slower |
| dynamic (object) string to int conversion | 210% slower |
| simple primitive numeric conversion (int to short) | 370% slower |
| complex primitive numeric conversion (double to decimal) | 100% slower | 
| dynamic (object) primitive numeric conversion (int to short) | 630% slower |
| int to string conversion | 30% slower |
| DateTime to string conversion | 5% slower |
| Failure (framework method throws exception) | 18000% faster |

Dynamic operations tend to be a lot slower than non dynamic ones, relatively. 
This is probably caused by the extra call into the converter needed after the type check is done.
Within the current design there is no way of avoiding the extra call. 

Performance differences are indicated for a single measurement run on my personal machine, so again, results may vary. 

* x% slower means it takes x% more time to perform the conversion.
* x% faster means it performs x% more conversions in the same time.

This way the **percentages** indicate the same order of difference between the reference measurement and the measurement itself.
