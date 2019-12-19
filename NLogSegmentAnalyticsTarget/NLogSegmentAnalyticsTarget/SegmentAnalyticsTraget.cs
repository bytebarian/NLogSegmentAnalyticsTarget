using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using Segment;
using Segment.Model;

namespace NLogSegmentAnalyticsTarget
{
    [Target("SegmentAnalyticsTraget")]
    public class SegmentAnalyticsTraget : TargetWithLayout
    {
        private DateTime lastLogEventTime;
        [RequiredParameter]
        public string WriteKey { get; set; }

        protected override void Dispose(bool disposing)
        {          
            Analytics.Client.Dispose();
            base.Dispose(disposing);
        }

        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            try
            {
                Analytics.Client.Flush();
                if (DateTime.UtcNow.AddSeconds(-30) > lastLogEventTime)
                {
                    // Nothing has been written, so nothing to wait for
                    asyncContinuation(null);
                }
                else
                {
                    // Documentation says it is important to wait after flush, else nothing will happen
                    // https://docs.microsoft.com/azure/application-insights/app-insights-api-custom-events-metrics#flushing-data
                    Task.Delay(TimeSpan.FromMilliseconds(500)).ContinueWith((task) => asyncContinuation(null));
                }
            }
            catch (Exception ex)
            {
                asyncContinuation(ex);
            }
        }

        protected override void InitializeTarget()
        {
            Analytics.Initialize(WriteKey);
            base.InitializeTarget();
        }

        protected override void Write(LogEventInfo logEvent)
        {
            lastLogEventTime = DateTime.UtcNow;
            var props = logEvent.Properties?.ToDictionary(k => k.Key is string ? (string)k.Key : k.Key.ToString(), k => k.Value);
            var userName = props.TryGetValue("user", out var userObj) 
                ? userObj is string ? (string)userObj : userObj.ToString()
                : "AnonymousUser";
            var eventName = props.TryGetValue("event", out var eventObj)
                ? eventObj is string ? (string)eventObj : eventObj.ToString()
                : "AnonymousEvent";
            Analytics.Client.Track(
                userName,
                eventName,
                props ?? new Dictionary<string, object>(),
                new Options().SetTimestamp(DateTime.UtcNow));
        }
    }
}
