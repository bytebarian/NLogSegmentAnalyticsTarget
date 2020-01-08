using NLog;
using NLog.Config;
using NLogSegmentAnalyticsTarget;
using System;

namespace NetFrameworkCodeConfigurationTest
{
    public class Tests
    {
        private static Logger _logger;

        static void Main(string[] args)
        {
            Init();
            TraceTest();

            Console.ReadKey();
        }

        public static void TraceTest()
        {
            var logEvent = new LogEventInfo(LogLevel.Trace, "Example", "NetFrameworkCodeConfigurationTest");
            logEvent.Properties["user"] = "j.doe";
            logEvent.Properties["session"] = "xyz";
            logEvent.Properties["event"] = "NetFrameworkCodeConfigurationTest";
            logEvent.Properties["token"] = "qwerty";

            _logger.Log(logEvent);
        }

        public static void Init()
        {
            var config = new LoggingConfiguration();

            var target = new SegmentAnalyticsTraget();
            target.WriteKey = "BdD0txpWecLRbW33DXssjC91Oo28G9Mv";

            LoggingRule rule = new LoggingRule("*", LogLevel.Trace, target);
            config.LoggingRules.Add(rule);

            LogManager.Configuration = config;

            _logger = LogManager.GetLogger("Example");
        }
    }
}
