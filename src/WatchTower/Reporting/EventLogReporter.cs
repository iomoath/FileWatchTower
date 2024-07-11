using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Serilog;

namespace WatchTower
{
    public class EventLogReporter : IEventReporter
    {
        public LogOutputFormat LogFormat { get; set; } = LogOutputFormat.Xml;
        public bool RemoveNullObjects { get; set; } = true;

        public async Task ReportAsync(EventReport eventReport)
        {
            await Task.Run(() =>
            {
                Report(eventReport);
            });
        }

        public void Report(EventReport eventReport)
        {
            try
            {
                string logMessage;
                if (LogFormat == LogOutputFormat.Xml)
                    logMessage = LogMessageGenerator.CreateXmlLogMessage(eventReport);
                else if (LogFormat == LogOutputFormat.Json || LogFormat == LogOutputFormat.NdJson)
                    logMessage = LogMessageGenerator.CreateNdJsonLogMessage(eventReport, RemoveNullObjects);
                else
                {
                    return;
                }

                WriteToEventLog(logMessage, EventLogEntryType.Information, eventReport.EventId);
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
        }


        private void WriteToEventLog(string message, EventLogEntryType type, int eventId)
        {
            using (var eventLog = new EventLog(GlobalConfigs.WinLogName))
            {
                eventLog.Source = GlobalConfigs.WinLogSourceName;
                eventLog.WriteEntry(message, type, eventId);
            }
        }
    }
}
