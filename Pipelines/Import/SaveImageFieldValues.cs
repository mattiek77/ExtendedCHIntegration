using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using WKSkunkWorks.Foundation.DAM.Helpers;
using Sitecore.Abstractions;
using Sitecore.Connector.CMP;
using Sitecore.Connector.CMP.Conversion;
using Sitecore.Connector.CMP.Pipelines.ImportEntity;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.SecurityModel;
using Sitecore.Data.Items;
using SaveFieldValues = Sitecore.Connector.CMP.Pipelines.ImportEntity.SaveFieldValues;

namespace WKSkunkWorks.Foundation.DAM.Pipelines.Import
{
    public class SaveImageFieldValues : ImportEntityProcessor
    {
        private readonly IPublicLinkHelper _publicLinkHelper;
        
        public SaveImageFieldValues(BaseLog logger, IPublicLinkHelper publicLinkHelper, CmpSettings settings) : base(logger, settings)
        {
            _publicLinkHelper = publicLinkHelper;
        }

        public override void Process(ImportEntityPipelineArgs args, BaseLog logger)
        {
            using (new LanguageSwitcher(args.Language))
            using (new SecurityDisabler())
            {
                UpdateImageFields(args, logger);
            }
        }

        private void UpdateImageFields(ImportEntityPipelineArgs args, BaseLog logger)
        {
            var configurationRoot = args.EntityMappingItem;
            var host = Constants.UriParser.Match(System.Configuration.ConfigurationManager.ConnectionStrings["CMP.ContentHub"]?.ConnectionString ?? string.Empty).Value;
            try
            {
                using (new EditContext(args.Item))
                {
                    configurationRoot.Children.Where(child => child.TemplateID.Equals(Constants.ImageImportTemplateID))
                        .ForEach(
                            importMapping =>
                            {
                                if (string.IsNullOrEmpty(host))
                                {
                                    throw new MissingFieldException(
                                        $"Unable to parse host from connection string  {System.Configuration.ConfigurationManager.ConnectionStrings["CMP.ContentHub"]?.ConnectionString ?? "No CMP.ContentHub conn str present"}");
                                }
                                var cmpField = importMapping[Sitecore.Connector.CMP.Constants.FieldMappingCmpFieldNameFieldId];
                                var sitecoreField = importMapping[Sitecore.Connector.CMP.Constants.FieldMappingSitecoreFieldNameFieldId];
                                var cmpdata = args.Entity.GetPropertyValue<string>(cmpField);
                                var scField = _publicLinkHelper.GetPublicLink(cmpdata, host).Result;
                                args.Item[sitecoreField] = scField;
                            });
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error occured getting the image data", ex, typeof(SaveFieldValues));
            }
        }
    }
}