using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WatchTower
{
    internal static class LogMessageGenerator
    {
        public static string CreateJsonLogMessage(string eventName, Dictionary<string, object> parameters)
        {
            var logEntry = new
            {
                EventName = eventName,
                EventData = parameters,
                Timestamp = DateTime.UtcNow
            };

            return JsonConvert.SerializeObject(logEntry);
        }

        public static string CreateJsonLogMessage(EventReport eventReport, bool excludeObjectsWithNullValues)
        {
            if (excludeObjectsWithNullValues)
                return Helpers.SerializeObjectWithoutNulls(eventReport);


            var settings = new JsonSerializerSettings
            {
                Converters = new JsonConverter[] { new StringEnumConverter() }
            };

            return JsonConvert.SerializeObject(eventReport, settings);
        }


        public static string CreateXmlLogMessage(string eventName, Dictionary<string, object> parameters)
        {
            var eventElement = new XElement("Event",
                new XAttribute("name", eventName),
                new XAttribute("timestamp", DateTime.UtcNow)
            );

            foreach (var param in parameters)
            {
                eventElement.Add(new XElement("Data",
                    new XAttribute("key", param.Key),
                    new XAttribute("value", param.Value)));
            }

            return eventElement.ToString();
        }

        public static string CreateXmlLogMessage(EventReport eventReport)
        {
            var xmlSerializer = new XmlSerializer(typeof(EventReport));
            using (var stringWriter = new StringWriter())
            {
                xmlSerializer.Serialize(stringWriter, eventReport);
                return stringWriter.ToString();
            }
        }

        public static string CreateNdJsonLogMessage(string eventName, Dictionary<string, object> parameters)
        {
            var logEntry = new
            {
                EventName = eventName,
                EventData = parameters,
                Timestamp = DateTime.UtcNow
            };

            string json = JsonConvert.SerializeObject(logEntry);
            return json + Environment.NewLine;
        }

        public static string CreateNdJsonLogMessage(EventReport eventReport, bool excludeObjectsWithNullValues)
        {
            if (excludeObjectsWithNullValues)
                return Helpers.SerializeObjectWithoutNulls(eventReport);


            var settings = new JsonSerializerSettings
            {
                Converters = new JsonConverter[] { new StringEnumConverter() }
            };

            return JsonConvert.SerializeObject(eventReport, settings);
        }
    }
}
