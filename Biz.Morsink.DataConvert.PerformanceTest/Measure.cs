using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Biz.Morsink.DataConvert.PerformanceTest
{
    /// <summary>
    /// Helper struct to perform a relative measurement between two Actions.
    /// The two actions are called the ReferenceAction and the MeasureAction, performance is measured of the MeasureAction relative to the ReferenceAction.
    /// </summary>
    public struct Measure
    {
        /// <summary>
        /// Factor between Stopwatch Tick duration and TimeSpan Tick duration. 
        /// These need not be the same on all platforms.
        /// </summary>
        public static readonly double TICKFACTOR;
        /// <summary>
        /// Measurement of an empty loop to cancel out the looping and Action calling time.
        /// Only looping is measured.
        /// </summary>
        public static readonly double LOOP_AND_CALL_OVERHEAD;
        static Measure()
        {
            const int LEN = 100_000_000;
            Action act = () => { };
            var sw = new Stopwatch();

            sw.Start();
            for (int i = 0; i < LEN; i++)
                ;
            sw.Stop();

            LOOP_AND_CALL_OVERHEAD = (double)sw.ElapsedTicks / LEN;
            TICKFACTOR = (double)new TimeSpan(0,0,1).Ticks / Stopwatch.Frequency;
        }
        private readonly string _name;
        private readonly TimeSpan _span;
        private readonly bool _initial;
        private readonly Action _reference;
        private readonly Action _measurement;

        /// <summary>
        /// Creates a Measure.
        /// </summary>
        /// <param name="name">The name for the Measure.</param>
        /// <returns>A new Measure.</returns>
        public static Measure Create(string name)
            => new Measure(name, new TimeSpan(0, 0, 1), true, null, null);
        private Measure(string name, TimeSpan span, bool initial, Action referenceAction, Action measurementAction)
        {
            _name = name;
            _span = span;
            _initial = initial;
            _reference = referenceAction;
            _measurement = measurementAction;
        }
        /// <summary>
        /// Sets the time to spend on measuring an Action.
        /// </summary>
        public Measure TimeSpan(TimeSpan span)
            => new Measure(_name, span, _initial, _reference, _measurement);
        /// <summary>
        /// Does nothing yet.
        /// </summary>
        public Measure InitialRun(bool initial = true)
            => new Measure(_name, _span, initial, _reference, _measurement);
        /// <summary>
        /// Sets the ReferenceAction
        /// </summary>
        public Measure ReferenceAction(Action action)
            => new Measure(_name, _span, _initial, action, _measurement);
        /// <summary>
        /// Sets the MeasurementAction
        /// </summary>
        public Measure MeasurementAction(Action action)
            => new Measure(_name, _span, _initial, _reference, action);
        /// <summary>
        /// Executes the measurement configured in this Measure.
        /// </summary>
        /// <returns>A MeasurementResult</returns>
        public MeasurementResult Execute()
        {
            var reference = MeasureAction(_span, _reference);
            var measure = MeasureAction(_span, _measurement);

            return new MeasurementResult(_name, reference, measure);
        }
        private long MeasureAction(TimeSpan time, Action act)
        {
            var ticks = (long)(time.Ticks / TICKFACTOR);
            var sw = new Stopwatch();
            long count = 0;
            long lap;
            sw.Start();
            for (int x = 0; (lap = sw.ElapsedTicks) < ticks; x++)
            {
                lap = sw.ElapsedTicks;
                var len = 1 << x;
                for (int y = 0; y < len; y++)
                    act();
                count += len;
            }
            GC.Collect(2);
            sw.Stop();
            var laco = (long)(LOOP_AND_CALL_OVERHEAD * count);
            var factor = (double)sw.ElapsedTicks / (sw.ElapsedTicks - laco);
#if DEBUG
            Console.WriteLine($"LACO-Factor: {factor}");
#endif
            return (long)factor * (count * ticks) / (sw.ElapsedTicks);
        }
    }
}
