using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace CoreInfrastructure.Logging
{
    public class PerformanceTracker
    {
        private readonly Stopwatch _sw;
        private DateTime _beginTime;
        private readonly Dictionary<string, object> _details;

        public string PerfName { get; set; }

        public PerformanceTracker(string name)
        {
            _sw = Stopwatch.StartNew();
            PerfName = name;
            _beginTime = DateTime.Now;
            _details = new Dictionary<string, object>();
        }

        public PerformanceTracker(string name, Dictionary<string, object> perfParams)
            : this(name)
        {
            foreach (var item in perfParams)
                _details.Add("input-" + item.Key, item.Value);
        }

        public void Stop()
        {
            _sw.Stop();

            if (!_details.ContainsKey("Started"))
            {
                _details.Add("Started", _beginTime.ToString(CultureInfo.InvariantCulture));
                _details.Add("ElapsedMilliseconds", _sw.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture));
                _details.Add("PerfName", PerfName);
            }
            else
            {
                _details["Started"] = _beginTime.ToString(CultureInfo.InvariantCulture);
                _details["ElapsedMilliseconds"] = _sw.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture);
                _details["PerfName"] = PerfName;
            }
            SuperLogger.WriteLog("Performance captured for " + PerfName, LoggingCategory.Performance, _details);
        }
    }
}
