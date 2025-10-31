using Google.Analytics.Data.V1Beta;
using System;
using System.Threading.Tasks;
namespace KILVRA.Area.Admin.AdminServices
{
    public class GoogleAnalyticsService
    {
        private readonly string _propertyId;
        private readonly string _credentialsPath;

        public GoogleAnalyticsService(IConfiguration config)
        {
            _propertyId = config["GoogleAnalytics:PropertyId"];
            _credentialsPath = Path.Combine(Directory.GetCurrentDirectory(), config["GoogleAnalytics:CredentialsPath"]);
        }
        public async Task<(long Users, long Sessions, double Revenue)> GetAnalyticsSummaryAsync()
        {
            var client = new BetaAnalyticsDataClientBuilder
            {
                CredentialsPath = _credentialsPath
            }.Build();

            var request = new RunReportRequest
            {
                Property = $"properties/{_propertyId}",
                DateRanges = { new DateRange { StartDate = "7daysAgo", EndDate = "today" } },
                Metrics =
                {
                    new Metric { Name = "totalUsers" },
                    new Metric { Name = "sessions" },
                    new Metric { Name = "purchaseRevenue" }
                }
            };

            var response = await client.RunReportAsync(request);

            long users = 0;
            long sessions = 0;
            double revenue = 0;

            if (response.Rows.Count > 0)
            {
                users = long.Parse(response.Rows[0].MetricValues[0].Value);
                sessions = long.Parse(response.Rows[0].MetricValues[1].Value);
                double.TryParse(response.Rows[0].MetricValues[2].Value, out revenue);
            }

            return (users, sessions, revenue);
        }
    }
}

