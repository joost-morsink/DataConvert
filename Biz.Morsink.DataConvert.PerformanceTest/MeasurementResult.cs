using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.DataConvert.PerformanceTest
{
    public class MeasurementResult
    {
        public MeasurementResult(string name, long reference, long measurement)
        {
            Name = name;
            Reference = reference;
            Measurement = measurement;
        }
        public string Name { get; }
        public long Reference { get; }
        public long Measurement { get; }
        public double Factor => (double)Measurement / Reference;
        public double Percentage
            => Measurement > Reference
                ? (Factor - 1) * 100
                : -(1 / Factor - 1) * 100;
        public override string ToString()
            => $"{Name}{Environment.NewLine}{Measurement} / {Reference} = {Factor} ({Percentage:0.0}%)";
    }
}
