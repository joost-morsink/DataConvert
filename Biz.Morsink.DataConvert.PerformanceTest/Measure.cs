using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Biz.Morsink.DataConvert.PerformanceTest
{
    public struct Measure
    {
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
        }
        private readonly string _name;
        private readonly TimeSpan _span;
        private readonly bool _initial;
        private readonly Action _reference;
        private readonly Action _measurement;

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
        public Measure TimeSpan(TimeSpan span)
            => new Measure(_name, span, _initial, _reference, _measurement);
        public Measure InitialRun(bool initial = true)
            => new Measure(_name, _span, initial, _reference, _measurement);
        public Measure ReferenceAction(Action action)
            => new Measure(_name, _span, _initial, action, _measurement);
        public Measure MeasurementAction(Action action)
            => new Measure(_name, _span, _initial, _reference, action);
        public MeasurementResult Execute()
        {
            var reference = MeasureAction(_span, _reference);
            var measure = MeasureAction(_span, _measurement);

            return new MeasurementResult(_name, reference, measure);
        }
        private long MeasureAction(TimeSpan time, Action act)
        {
            var ticks = time.Ticks;
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
