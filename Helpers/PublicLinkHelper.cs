using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Stylelabs.M.Base.Querying;
using Stylelabs.M.Base.Querying.Linq;
using Stylelabs.M.Framework.Essentials.LoadConfigurations;
using Stylelabs.M.Sdk.Contracts.Base;
using Stylelabs.M.Sdk.WebClient;
using MConstants = Stylelabs.M.Sdk.Constants;

namespace ExtendedCHIntegration.Foundation.DAM.Helpers
{
    public class PublicLinkHelper : IPublicLinkHelper
    {
        private readonly  WebClientFactory _webClientFactory;
        private IWebMClient Client => _webClientFactory.GetClient;
        private static readonly Regex imageParsingRegex = new Regex(@"[^\/|?]+(?=\?)", RegexOptions.Compiled);
        private const string outputFormat = "<image stylelabs-content-id=\"{0}\" thumbnailsrc=\"{1}/api/gateway/{0}/thumbnail\" src=\"{2}\" mediaid=\"\" stylelabs-content-type=\"Image\" alt=\"{5}\" height=\"{3}\" width=\"{4}\" />";

        public PublicLinkHelper(WebClientFactory clientFactory)
        {
            _webClientFactory = clientFactory;
        }

        public async Task<string> GetPublicLink(string source, string host)
        {
            //caught in unit test.
            if (string.IsNullOrWhiteSpace(source))
                return string.Empty;

            var relativeUrl = imageParsingRegex.Match(source).Value;
            if (string.IsNullOrEmpty(relativeUrl))
                return string.Empty;
            host = host.TrimEnd('/');
            return await GetPublicLinkFomDAM(relativeUrl, host);
        }

        private async Task<string> GetPublicLinkFomDAM(string relativeUrl, string host)
        {
            var query = Query.CreateQuery(entities =>
            {
                return entities.Where(e => e.DefinitionName == "M.PublicLink")
                    .Where(e => e.Property("RelativeUrl") == relativeUrl);
            });
            var publicImageEntity = (await Client.Querying.QueryAsync(query, EntityLoadConfiguration.Full)).Items.FirstOrDefault();
            if (publicImageEntity == null)
                return string.Empty;
            var parentEntityId =
                (await publicImageEntity.GetRelationAsync<IChildToManyParentsRelation>(MConstants.PublicLink
                    .AssetToPublicLink)).Parents.FirstOrDefault();
            var parentEntity = await Client.Entities.GetAsync(parentEntityId, EntityLoadConfiguration.Full);
            var altText = await parentEntity.GetPropertyValueAsync<string>(MConstants.Asset.Title) ?? await parentEntity.GetPropertyValueAsync<string>(MConstants.Asset.FileName);
            var rawResponse = await (await Client.Raw.GetAsync($"{host}/api/entities/{publicImageEntity.Id}").ConfigureAwait(false)).Content.ReadAsStringAsync().ConfigureAwait(false);
            var response = JToken.Parse(rawResponse);
            var plToken = response.SelectToken(Constants.PublicLink.PublicLinkField);
            var publiclinkUrl = plToken?.Value<string>() ?? string.Empty;

            //get conversion from public link if it exists
            var publicLinkConversions = await publicImageEntity.GetPropertyValueAsync<JToken>(Constants.PublicLink.ConversionConfiguration);
            if (publicLinkConversions == null)
            {
                publicLinkConversions = (await parentEntity.GetPropertyValueAsync<JToken>($"{Constants.Asset.MainFile}")).SelectToken(Constants.Asset.Properties);
            }

            var height = publicLinkConversions?.SelectToken(Constants.Asset.Height)?.Value<string>();
            var width = publicLinkConversions?.SelectToken(Constants.Asset.Width)?.Value<string>();

            return string.Format(outputFormat, parentEntityId, host, publiclinkUrl, height, width, altText);
        }
    }
}