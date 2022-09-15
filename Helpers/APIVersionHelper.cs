using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Sitecore.Connector.CMP;
using Sitecore.Shell.Applications.WebEdit.Commands;
using Stylelabs.M.Sdk.WebClient;

namespace WKSkunkWorks.Foundation.DAM.Helpers
{
    public class APIVersionHelper
    {
        private readonly WebClientFactory _webClientFactory;
        private IWebMClient Client => _webClientFactory.GetClient;
        private readonly CmpSettings _cmpSettings;
        public APIVersionHelper(WebClientFactory clientFactory, CmpSettings cmpSettings)
        {
            _webClientFactory = clientFactory;
            _cmpSettings = cmpSettings;
        }

        public int GetMajorVersion()
        {
            var fullVersion = GetVersion();
            if (string.IsNullOrEmpty(fullVersion))
            {
                return 0;
            }

            var versionstring = fullVersion.Split('.').First();
            if (int.TryParse(versionstring, out var version))
            {
                return version;
            }
            return 0;
        }

        private string RootUrl
        {
            get
            {
                var connection = _cmpSettings.ContentHubConnectionString;
                var root = connection.Split(';').FirstOrDefault(val => val.StartsWith("uri", StringComparison.OrdinalIgnoreCase)).Split('=')
                    .LastOrDefault();
                var fullUrl = new Uri(root);

                return fullUrl.Scheme + "://" + fullUrl.Host;
            }
        }

        public string GetVersion()
        {
            var response = Client.Raw.GetAsync(RootUrl+ "/api/status").ConfigureAwait(false).GetAwaiter().GetResult();
            var data = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            dynamic d = JsonConvert.DeserializeObject(data);
            var version = d?.file_version;
            return version;
        }
    }
}