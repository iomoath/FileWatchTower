using System.Threading.Tasks;

namespace WatchTower
{
    public class ApiEventReporter : IEventReporter
    {
        public LogOutputFormat LogFormat { get; set; } = LogOutputFormat.Json;
        public bool RemoveNullObjects { get; set; } = true;

        public Task ReportAsync(EventReport eventReport)
        {
            throw new System.NotImplementedException();
        }

        public void Report(EventReport eventReport)
        {
            throw new System.NotImplementedException();
        }



        public async Task ReportAsync(EventReport eventReport, IConfiguration configuration)
        {
            //if (string.IsNullOrEmpty(configuration.ApiLogEndpointUrl))
            //    return;

            //Log.Debug($"Sending event report as JSON to {configuration.ApiLogEndpointUrl}");
            //var json = JsonSerializer.Serialize(eventReport);
            //var result = await Web.WebHelper.PostJsonAsync(configuration.ApiLogEndpointUrl, json, configuration.ApiAuthorizationToken);

            //Log.Debug(result.StatusCode == HttpStatusCode.OK
            //    ? $"Sent event report as JSON to '{configuration.ApiLogEndpointUrl}'. Response code: {result.StatusCode}"
            //    : $"Failed to send event report as JSON to '{configuration.ApiLogEndpointUrl}'. Response code: {result.StatusCode}");
        }

        public void Report(EventReport report, IConfiguration configuration)
        {
            //if (string.IsNullOrEmpty(configuration.ApiLogEndpointUrl))
            //    return;

            //Log.Debug($"Sending event report as JSON to {configuration.ApiLogEndpointUrl}");
            //var json = JsonSerializer.Serialize(report);
            //var result = Web.WebHelper.PostJson(configuration.ApiLogEndpointUrl, json, configuration.ApiAuthorizationToken);

            //Log.Debug(result.StatusCode == HttpStatusCode.OK
            //    ? $"Sent event report as JSON to '{configuration.ApiLogEndpointUrl}'. Response code: {result.StatusCode}"
            //    : $"Failed to send event report as JSON to '{configuration.ApiLogEndpointUrl}'. Response code: {result.StatusCode}");
        }

    }
}
