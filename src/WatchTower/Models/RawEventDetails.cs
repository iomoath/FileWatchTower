using System;
using Microsoft.Diagnostics.Tracing;

namespace WatchTower
{
    public class RawEventDetails
    {
        public EventName EventName { get; set; }
        public string EventNameStr { get; set; }
        public int EventId { get; set; }
        public DateTime TimeStamp { get; set; }

        public string RuleName { get; set; }
        public DateTime UtcTime { get; set; }
        public string ProcessGuid { get; set; }
        public string ProcessId { get; set; }
        public string Image { get; set; }
        public string TargetFilename { get; set; }
        public DateTime CreationUtcTime { get; set; }
        public string User { get; set; }
        public string Hashes { get; set; }
        public string ComputerName { get; set; }


        public static RawEventDetails ParseFromTraceEvent(TraceEvent traceEvent)
        {
            var payload = new RawEventDetails();

            foreach (var name in traceEvent.PayloadNames)
            {
                var value = traceEvent.PayloadByName(name);
                switch (name)
                {
                    case "RuleName":
                        payload.RuleName = Convert.ToString(value);
                        break;
                    case "UtcTime":
                        payload.UtcTime = ParseDateTime(value);
                        break;
                    case "ProcessGuid":
                        payload.ProcessGuid = Convert.ToString(value);
                        break;
                    case "ProcessId":
                        payload.ProcessId = Convert.ToString(value);
                        break;
                    case "Image":
                        payload.Image = Convert.ToString(value);
                        break;
                    case "TargetFilename":
                        payload.TargetFilename = Convert.ToString(value);
                        break;
                    case "CreationUtcTime":
                        payload.CreationUtcTime = ParseDateTime(value);
                        break;
                    case "User":
                        payload.User = Convert.ToString(value);
                        break;
                    case "Hashes":
                        payload.Hashes = Convert.ToString(value);
                        break;
                }
            }

            payload.EventId = (int)traceEvent.ID;
            payload.EventNameStr = traceEvent.EventName;
            payload.TimeStamp = traceEvent.TimeStamp;
            payload.EventName = GetEventName(payload.EventId);


            return payload;
        }
        public static RawEventDetails ParseFromTraceEventData(TraceEventData traceEventData)
        {
            var payload = new RawEventDetails();

            if (traceEventData.Payload != null)
            {
                foreach (var kvp in traceEventData.Payload)
                {
                    switch (kvp.Key)
                    {
                        case "RuleName":
                            payload.RuleName = Convert.ToString(kvp.Value);
                            break;
                        case "UtcTime":
                            payload.UtcTime = ParseDateTime(kvp.Value);
                            break;
                        case "ProcessGuid":
                            payload.ProcessGuid = Convert.ToString(kvp.Value);
                            break;
                        case "ProcessId":
                            payload.ProcessId = Convert.ToString(kvp.Value);
                            break;
                        case "Image":
                            payload.Image = Convert.ToString(kvp.Value);
                            break;
                        case "TargetFilename":
                            payload.TargetFilename = Convert.ToString(kvp.Value);
                            break;
                        case "CreationUtcTime":
                            payload.CreationUtcTime = ParseDateTime(kvp.Value);
                            break;
                        case "User":
                            payload.User = Convert.ToString(kvp.Value);
                            break;
                        case "Hashes":
                            payload.Hashes = Convert.ToString(kvp.Value);
                            break;
                    }
                }
            }

            payload.EventId = (int)traceEventData.Id;
            payload.EventNameStr = traceEventData.EventName;
            payload.TimeStamp = traceEventData.TimeStamp;
            payload.EventName = GetEventName(payload.EventId);
            return payload;
        }

        private static EventName GetEventName(int eventId)
        {
            if (eventId == 11)
            {
                return EventName.FileCreate;
            }

            if (eventId == 29)
            {
                return EventName.FileExecutableDetected;
            }

            if (eventId == 2)
            {
                return EventName.FileCreateTime;
            }


            return EventName.Unsupported;

        }

        private static DateTime ParseDateTime(object dateTimeValue)
        {
            if (DateTime.TryParse(Convert.ToString(dateTimeValue), out DateTime parsedDateTime))
            {
                return parsedDateTime;
            }
            return default(DateTime);
        }
    }
}
