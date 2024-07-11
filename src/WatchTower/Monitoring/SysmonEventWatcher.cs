using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;
using Serilog;

namespace WatchTower
{
    public class SysmonEventWatcher : IMonitoringService
    {
        #region Members
        public event EventHandler<NewFileIOEventAvailableEventArgs> NewFileIoEventAvailable;
        private readonly string _computerName;
        private volatile bool _running;
        private readonly CancellationToken _cancellationToken;
        private readonly ConcurrentBag<Task> _tasks;

        private readonly Queue<TraceEventData> _eventJobQueue;
        private readonly object _eventJobQueueLock = new object();

        // Sysmon
        private const string SysmonProviderGuid = "5770385F-C22A-43E0-BF4C-06F5698FFBD9";
        private const string TraceEventSessionName = "FWT_SysmonEventWatcher";

      


        #endregion

        #region Public

        public SysmonEventWatcher()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = cancellationTokenSource.Token;
            _tasks = new ConcurrentBag<Task>();
            _eventJobQueue = new Queue<TraceEventData>();
            _computerName = Helpers.GetComputerName();
        }


        public void Start()
        {
            try
            {
                _running = true;

                // Max 3 workers
                for (int i = 0; i < 2; i++)
                {
                    var t1 = new Task<Task>(TraceEventJobConsumer, _cancellationToken, TaskCreationOptions.LongRunning);
                    t1.Start();
                    _tasks.Add(t1);
                }

                var t2 = new Task(Watcher, _cancellationToken, TaskCreationOptions.LongRunning);
                t2.Start();
                _tasks.Add(t2);
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
        }


        public async Task Stop()
        {
            try
            {
                _running = false;
                ClearRawEventJobQueue();

                await Task.Run(() =>
                {
                    foreach (var task in _tasks)
                    {
                        try
                        {
                            task?.Wait();
                        }
                        catch (AggregateException aex)
                        {
                            if (aex.InnerException != null)
                                Log.Error(aex.InnerException, aex.InnerException.Message);

                            aex.Handle(x => true);
                        }
                        catch (Exception e)
                        {
                            Log.Error(e, e.Message);
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
        }



        public void Wait()
        {
            Task.Run(() =>
            {
                while (_running)
                {
                    Thread.Sleep(50);
                }
            }).Wait();
        }

        public async Task WaitAsync()
        {
            while (_running)
            {
                await Task.Delay(50);
            }
        }

        #endregion

        #region Trace Event Job Queue

        private void EnqueueRawEventJob(TraceEventData e)
        {
            lock (_eventJobQueueLock)
            {
                _eventJobQueue.Enqueue(e);
            }
        }

        private TraceEventData DequeueRawEventJob()
        {
            lock (_eventJobQueueLock)
            {
                if (_eventJobQueue.Count > 0)
                    return _eventJobQueue.Dequeue();

                return null;

            }
        }

        private void ClearRawEventJobQueue()
        {
            lock (_eventJobQueueLock)
            {
                _eventJobQueue?.Clear();
            }
        }

        #endregion


        #region Tarce Event Watcher

        private void Watcher()
        {
            using (var session = new TraceEventSession(TraceEventSessionName))
            {
                session.EnableProvider(new Guid(SysmonProviderGuid));

                session.Source.Dynamic.All += delegate (TraceEvent e)
                {
                    if (!ShouldProcess(e.ID))
                        return;
                    
                    // Minimum delay; capture now and process later.
                    EnqueueRawEventJob(CreateTraceEventData(e));
                };

                session.Source.Process();
            }
        }

        private TraceEventData CreateTraceEventData(TraceEvent e)
        {
            return new TraceEventData
            {
                Id = e.ID,
                TimeStamp = e.TimeStamp,
                EventName = e.EventName,
                ProviderGuid = e.ProviderGuid,
                Payload = ExtractRelevantPayloadData(e)
            };
        }

        private Dictionary<string, object> ExtractRelevantPayloadData(TraceEvent e)
        {
            return e.PayloadNames
                .Select(name => new { name, value = e.PayloadByName(name) })
                .ToDictionary(p => p.name, p => p.value);
        }

        #endregion


        #region Trace Event Processor

        private async Task TraceEventJobConsumer()
        {
            while (_running)
            {
                await Task.Delay(5, _cancellationToken);

                try
                {
                    var eventData = DequeueRawEventJob();
                    if (eventData == null)
                        continue;

                  

                    ProcessEvent(eventData);
                }
                catch (Exception e)
                {
                    Log.Error(e, e.Message);
                }
            }
        }

        private void ProcessEvent(TraceEventData traceEvent)
        {
            var eventRaw = ConstructBaseEventDetailsObject(traceEvent);
            OnNewFileIOEventAvailable(this, eventRaw);
        }

        private RawEventDetails ConstructBaseEventDetailsObject(TraceEventData data)
        {
            var eventRaw = RawEventDetails.ParseFromTraceEventData(data);
            eventRaw.ComputerName = _computerName;
            return eventRaw;
        }


        private bool ShouldProcess(TraceEventID id)
        {
            var i = (int)id;

            if (i == 11)
                return true;

            if (i == 29)
                return true;

            //if (i == 2)
            //    return true;

            return false;
        }

        private void OnNewFileIOEventAvailable(object sender, RawEventDetails e)
        {
            EventHandler<NewFileIOEventAvailableEventArgs> raiseEvent = NewFileIoEventAvailable;
            if (raiseEvent == null)
                return;

            foreach (var handler in raiseEvent.GetInvocationList())
            {
                try
                {
                    ((EventHandler<NewFileIOEventAvailableEventArgs>)handler)(sender, new NewFileIOEventAvailableEventArgs(e));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, ex.Message);
                }
            }
        }


        #endregion




    }
}
