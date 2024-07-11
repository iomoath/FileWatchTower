using System;
using System.Collections.Generic;
using Microsoft.Diagnostics.Tracing;

namespace WatchTower
{
    public class TraceEventData
    {
        public TraceEventID Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public Dictionary<string, object> Payload { get; set; }
        public Guid ProviderGuid { get; set; }
        public string EventName { get; set; }
    }
}
