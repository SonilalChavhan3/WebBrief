using HtmlAgilityPack;
using System.Net;
using System.Xml;

namespace WebBrief.Services
{
    public interface IWebsiteScraper
    {
        Task<string> FetchWebsiteContentsAsync(string url);
    }

    public class WebsiteScraper : IWebsiteScraper
    {
        private readonly HttpClient _httpClient;

        public WebsiteScraper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> FetchWebsiteContentsAsync(string url)
        {
            var html = await _httpClient.GetStringAsync(url);

            var document = new HtmlDocument();
            document.LoadHtml(html);

            // Remove unwanted elements
            var unwantedNodes = document.DocumentNode.SelectNodes(
                "//script | //style | //nav | //header | //footer"
            );

            if (unwantedNodes != null)
            {
                foreach (var node in unwantedNodes)
                    node.Remove();
            }

            var text = document.DocumentNode.InnerText;

            return WebUtility.HtmlDecode(text);
        }
    }
}
