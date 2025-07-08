using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace SpiritualGiftsSurvey.Services;

public interface IAnalyticsService
{
    void TrackEvent(string eventName, IDictionary<string, string>? properties = null);
    void TrackError(Exception exception, IDictionary<string, string>? properties = null);
}


public class AppInsightsService : IAnalyticsService
{
    readonly TelemetryClient _client;

    public AppInsightsService()
    {
        var config = TelemetryConfiguration.CreateDefault();
        config.ConnectionString = "InstrumentationKey=c88bdba7-57cc-4f02-a4a0-0a0f9446c142;IngestionEndpoint=https://eastus-8.in.applicationinsights.azure.com/;LiveEndpoint=https://eastus.livediagnostics.monitor.azure.com/;ApplicationId=0e4e0412-4693-4426-8011-9cdc0e5caa7a";

        _client = new TelemetryClient(config);
    }

    public void TrackEvent(string eventName, IDictionary<string, string>? properties = null)
        => _client.TrackEvent(eventName, properties);

    public void TrackError(Exception exception, IDictionary<string, string>? properties = null)
        => _client.TrackException(exception, properties);
}

