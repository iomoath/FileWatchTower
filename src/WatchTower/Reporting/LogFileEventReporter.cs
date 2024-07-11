using System.Threading.Tasks;

namespace WatchTower
{
    public class LogFileEventReporter : IEventReporter
    {
        public LogOutputFormat LogFormat { get; set; } = LogOutputFormat.NdJson;
        public bool RemoveNullObjects { get; set; } = false;


        public async Task ReportAsync(EventReport eventReport)
        {
            await Task.Run(() =>
            {
                Report(eventReport);
            });
        }

        public void Report(EventReport eventReport)
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

            LogFileWriter.Instance.WriteLog(logMessage);
        }
    }
}
