using System;
using System.Threading.Tasks;

namespace WatchTower
{
    public interface IMonitoringService
    {
        event EventHandler<NewFileIOEventAvailableEventArgs> NewFileIoEventAvailable;
        void Start();
        Task Stop();
        void Wait();
        Task WaitAsync();
    }
}
