using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using Stylelabs.M.Sdk.WebClient;

namespace WKSkunkWorks.Foundation.DAM
{
    public class WebClientFactory
    {
        public IWebMClient GetClient => ServiceLocator.ServiceProvider.GetRequiredService<IWebMClient>();
    }
}