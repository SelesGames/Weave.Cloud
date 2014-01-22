using SelesGames.HttpClient;
using System.Linq;

namespace System.Net.Http.Formatting
{
    public static class SmartHttpClientExtensions
    {
        public static void AddFormatter(this SmartHttpClient client, MediaTypeFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");

            if (!client.Formatters.Any(o => o.Equals(formatter)))
                client.Formatters.Add(formatter);
        }

        public static void RemoveFormatters(this SmartHttpClient client, Func<MediaTypeFormatter, bool> condition)
        {
            if (condition == null) throw new ArgumentNullException("condition");

            var matchingFormatters = client.Formatters.Where(condition).ToList();

            foreach (var formatter in matchingFormatters)
                client.Formatters.Remove(formatter);
        }
    }
}
