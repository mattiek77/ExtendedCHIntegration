using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ExtendedCHIntegration.Foundation.DAM.Helpers;
using ExtendedCHIntegration.Foundation.DAM.Pipelines.Rendering;

namespace ExtendedCHIntegration.Foundation.DAM.DI
{
    public class Configurator : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IOneWebCMPSitecoreHelper, OneWebCMPSitecoreHelper>();
            serviceCollection.AddSingleton<IPublicLinkHelper, PublicLinkHelper>();
            serviceCollection.AddSingleton<ICMPExporter, CMPExporter>();
            serviceCollection.AddSingleton<ICMPWorkflowHelper, CMPWorkflowHelper>();
            serviceCollection.AddSingleton<IOneWebCMPSitecoreHelper, OneWebCMPSitecoreHelper>();
            serviceCollection.AddSingleton<APIVersionHelper>();
            serviceCollection.AddSingleton<ImportMappingService>();
            serviceCollection.AddSingleton<SitecoreFieldMappingHelper>();
            serviceCollection.AddSingleton<WebClientFactory>();
        }
    }
}