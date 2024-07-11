using System.Threading.Tasks;

namespace WatchTower
{
    public interface IEventReporter
    {
        Task ReportAsync(EventReport eventReport);
        void Report(EventReport eventReport);
        LogOutputFormat LogFormat { get; set; }
        bool RemoveNullObjects { get; set; }
    }
}
