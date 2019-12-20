using System;
using System.Collections.Generic;
using System.Linq;
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

        [RequiredParameter]
        public string WriteKey { get; set; }

        protected override void Dispose(bool disposing)
        {
            Analytics.Client.Dispose();
            base.Dispose(disposing);
        }

        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            Analytics.Client.Flush();
            base.FlushAsync(asyncContinuation);
        }

        protected override void InitializeTarget()
        {
            Analytics.Initialize(WriteKey);
            base.InitializeTarget();
        }

        protected override void Write(LogEventInfo logEvent)
        {
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