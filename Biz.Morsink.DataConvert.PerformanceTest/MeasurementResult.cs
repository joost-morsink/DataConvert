using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.DataConvert.PerformanceTest
{
    /// <summary>
    /// This class represent the result of a measurement.
    /// </summary>
    public class MeasurementResult
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public MeasurementResult(string name, long reference, long measurement)
        {
            Name = name;
            Reference = reference;
            Measurement = measurement;
        }
        /// <summary>
        /// The Measure's Name
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The amount of iterations of the ReferenceAction performed in the configured TimeSpan.
        /// </summary>
        public long Reference { get; }
        /// <summary>
        /// The amount of iterations of the MeasureAction performed in the configured TimeSpan.
        /// </summary>
        public long Measurement { get; }
        /// <summary>
        /// The factor between the number of iterations of the MeasureAction and the ReferenceAction.
        /// </summary>
        public double Factor => (double)Measurement / Reference;
        /// <summary>
        /// A percentage for the factor.
        /// </summary>
        public double Percentage
            => Measurement > Reference
                ? (Factor - 1) * 100
                : -(1 / Factor - 1) * 100;
        public override string ToString()
            => $"{Name}{Environment.NewLine}{Measurement} / {Reference} = {Factor} ({Percentage:0.0}%)";
    }
}
