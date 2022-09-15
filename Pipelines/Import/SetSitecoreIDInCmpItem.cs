using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using Sitecore.Abstractions;
using Sitecore.Connector.CMP;
using Sitecore.Connector.CMP.Helpers;
using Sitecore.Connector.CMP.Pipelines.ImportEntity;
using Sitecore.Publishing.Pipelines.Publish;
using Stylelabs.M.Framework.Essentials.LoadConfigurations;
using Stylelabs.M.Sdk.WebClient;

namespace WKSkunkWorks.Foundation.DAM.Pipelines.Import
{
    public class SetSitecoreIDInCmpItem :ImportEntityProcessor
    {
        private readonly WebClientFactory _webClientFactory;
        private IWebMClient Client => _webClientFactory.GetClient;

        public SetSitecoreIDInCmpItem(WebClientFactory clientFactory, BaseLog logger, CmpSettings settings) : base(logger, settings)
        {
            _webClientFactory = clientFactory;
        }

        public override void Process(ImportEntityPipelineArgs args, BaseLog logger)
        {
            if (!args.Entity.Id.HasValue)
                return;
            var scID = args.Item.ID.ToString();
            args.EntityMappingItem.Children.Where(child=>child.TemplateID.Equals(Constants.IDToCHField.TemplateID)).ForEach(
                child =>
                {
                    var item = Client.Entities.GetAsync(args.Entity.Id.Value, EntityLoadConfiguration.Default).ConfigureAwait(false).GetAwaiter().GetResult();
                    logger.Debug($"Sending ID {scID} back to ch item field {child[Sitecore.Connector.CMP.Constants.FieldMappingCmpFieldNameFieldId]}");
                    item.SetPropertyValue(child[Sitecore.Connector.CMP.Constants.FieldMappingCmpFieldNameFieldId], scID);
                    Client.Entities.SaveAsync(item).ConfigureAwait(false).GetAwaiter().GetResult();
                });
        }
    }
}