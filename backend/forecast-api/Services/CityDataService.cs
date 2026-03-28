using forecast_api.Models;
using HtmlAgilityPack;
using Microsoft.VisualBasic;

namespace forecast_api.Services
{
    /// <summary>
    /// An interface for fetching the extended data for a cities
    /// </summary>
    public interface ICityDataService
    {
        /// <summary
        /// Fetches the extended data for a given Geolocation
        /// </summary>
        /// <param name="geolocation">The Geolocation for which to fetch extended city data</param>
        /// <returns>The extended city data for the given Geolocation</returns>
        public Task<CityData?> GetCityDataAsync(Geolocation geolocation);
    }

    /// <summary>
    /// A CityDataService that is backed by Wikipedia
    /// </summary>
    public class WikipediaCityDataService(HttpClient client) : ICityDataService
    {
        public async Task<CityData?> GetCityDataAsync(Geolocation geolocation)
        {
            var encodedCityName = geolocation.CityName.Replace(" ", "_");
            var url = $"https://en.wikipedia.org/w/rest.php/v1/page/{encodedCityName}/html";

            client.DefaultRequestHeaders.UserAgent.ParseAdd("Test Forecast App");
            HttpResponseMessage response;

            // If we can't find the Wikipedia page for this city, return nothing
            try {
                response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
            } catch {
                return null;
            }

            var html = await response.Content.ReadAsStringAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Cleanup the first paragraph and return it as the blurb
            var blurb = GetSanitizedFirstParagraph(doc);

            // Extract Population from infobox data
            var infobox = doc.DocumentNode.SelectSingleNode("//table[contains(@class,'infobox')]");
            long? population = GetPopulationDataFromInfoBox(infobox);

            // Learn more URL
            var learnMoreUrl = $"https://en.wikipedia.org/wiki/{encodedCityName}";

            return new CityData
            {
                Blurb = blurb,
                Geolocation = geolocation,
                Population = population,
                LearnMoreUrl = learnMoreUrl
            };
        }

        private string GetSanitizedFirstParagraph(HtmlDocument document)
        {
            // Get the first paragraph tag that does not have a class set, which skips the one at the very top of the page that is used for disambiguation
            var firstParagraph = document.DocumentNode.SelectSingleNode("//p[not(@class)]");
            var blurb = firstParagraph?.InnerText ?? "";

            // Remove references like [1], [2], etc.
            blurb = RemoveCitations(blurb);
            // Strip HTML entities
            blurb = HtmlEntity.DeEntitize(blurb);

            // Remove leading pronunciation parenthetical (e.g., "Edmonton (/ˈɛdməntən/ ... ) is...")
            var pronunMatch = System.Text.RegularExpressions.Regex.Match(
                blurb,
                @"^(.*?)\s*\(([^)]*?(?:listen|IPA|/)[^)]*)\)\s*(.*)$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (pronunMatch.Success)
            {
                var before = pronunMatch.Groups[1].Value.Trim();
                var after = pronunMatch.Groups[3].Value.Trim();
                blurb = string.IsNullOrEmpty(after) ? before : $"{before} {after}".Trim();
            }

            return blurb;
        }

        private string RemoveCitations(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, @"\[\w+\]", "");         
        }

        private long? GetPopulationDataFromInfoBox(HtmlNode? infobox)
        {
            if(infobox == null){
                return null;
            }

            // Wikipedia has a bunch of different population statistics. These are some of the most common
            // We're going to return the first highest one we see
            // They'll all start off at null to start, and as we see them, we'll populate it, then take the max
            var populationTypes = new Dictionary<string, long?>(){ {"Metro", null}, {"Urban", null}, { "Total", null}, {"Municipality", null} };


            bool inPopulationSection = false;
            var rows = infobox.SelectNodes(".//tr");
            if (rows != null)
            {
                // Scan the rows in the input box
                foreach (var row in rows)
                {
                    // Get the table headers and data columns
                    var th = row.SelectSingleNode(".//th");
                    var td = row.SelectSingleNode(".//td");
                    
                    // Check if this is a section header (header with no data)
                    if (th != null && td == null)
                    {
                        // Check if this is the Population header
                        var sectionHeader = th.InnerText.Trim();
                        inPopulationSection = sectionHeader.Contains("Population");
                        continue;
                    }

                    // If we're in the Population section, and we have a header and data, look at these rows
                    if(inPopulationSection && th != null && td != null){

                        // Sometimes the population header will have citations in them, so remove them if they do
                        var populationHeader = RemoveCitations(th.InnerText.Trim());
                        var populationValue = td.InnerText.Trim();

                        // Look through the population types, in order, and see if this header string ends with that type.
                        // Note, not Contains because we'll often have entries like "Metro" and "Metro density"
                        foreach(var populationType in populationTypes){
                            if(populationHeader.EndsWith(populationType.Key)){
                                // Set this population to be the value we see (or null if it doesn't exist)
                                populationTypes[populationType.Key] = ExtractPopulationValue(populationValue);
                            }
                        }                        
                    }
                }
            }

            // If the highest value for the population we saw, or null if we couldn't find it
            return populationTypes.Max(type => type.Value);
        }

        private long? ExtractPopulationValue(string value)
        {

            // Get the first numerical value in the string (often containing commas)
            var popMatch = System.Text.RegularExpressions.Regex.Match(value, @"([\d,]+)");
            if (popMatch.Success)
            {
                // If it can be parsed as long, return it
                return long.TryParse(popMatch.Groups[0].Value.Replace(",", ""), out long output) ? output : null;
            }
            return null;
        }
    }
}