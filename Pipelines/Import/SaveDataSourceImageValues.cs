using System;
using System.Linq;
using ExtendedCHIntegration.Foundation.DAM.Helpers;
using Sitecore.Abstractions;
using Sitecore.Connector.CMP;
using Sitecore.Connector.CMP.Pipelines.ImportEntity;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.SecurityModel;

namespace ExtendedCHIntegration.Foundation.DAM.Pipelines.Import
{
    public class SaveDataSourceImageValues : ImportEntityProcessor
    {
        private readonly IPublicLinkHelper _publicLinkHelper;
        
        public SaveDataSourceImageValues(IPublicLinkHelper publicLinkHelper, CmpSettings settings,
            BaseLog logger) : base(logger, settings)
        {
            _publicLinkHelper = publicLinkHelper;
        }

        public override void Process(ImportEntityPipelineArgs args, BaseLog logger)
        {
            Assert.IsNotNull((object)args.Item, "The item is null.");
            Assert.IsNotNull((object)args.Language, "The language is null.");
            using (new SecurityDisabler())
            using (new LanguageSwitcher(args.Language))
            {
                SetChildValues(args, logger);
            }
        }

        public void SetChildValues(ImportEntityPipelineArgs args, BaseLog logger)
        {
            var configItem = args.EntityMappingItem;
            var updatingItem = args.Item;

            var allUpdateSettings = from child in configItem.Children
                                    where child.TemplateID == Constants.LocalDataItemImageMappingTemplateID
                                    select child;
            foreach (var item in allUpdateSettings)
            {
                try
                {
                    var relatedItemPath = item[Constants.RelatedItemPathFieldID];
                    var cmpField = item[Sitecore.Connector.CMP.Constants.FieldMappingCmpFieldNameFieldId];
                    var sitecoreField = item[Sitecore.Connector.CMP.Constants.FieldMappingSitecoreFieldNameFieldId];
                    if (relatedItemPath.StartsWith(Constants.LocalDS))
                    {
                        relatedItemPath = relatedItemPath.Replace(Constants.LocalDS, updatingItem.Paths.FullPath);
                    }

                    var itemWeAreUpdating = updatingItem.Database.GetItem(relatedItemPath);
                    if (itemWeAreUpdating == null)
                    {
                        logger.Error(
                            $"{nameof(SaveDataSourceFieldValues)} - cant find item {relatedItemPath} for mapping {item.Paths.FullPath}",
                            this);
                        continue;
                    }

                    try
                    {
                        itemWeAreUpdating.Editing.BeginEdit();
                        var host = Constants.UriParser.Match(System.Configuration.ConfigurationManager.ConnectionStrings["CMP.ContentHub"]?.ConnectionString ?? string.Empty).Value;
                        var imgSrc = args.Entity.GetPropertyValue<string>(cmpField);
                        itemWeAreUpdating[sitecoreField] = _publicLinkHelper.GetPublicLink(imgSrc, host).Result;
                        itemWeAreUpdating.Editing.EndEdit();
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"{nameof(SaveDataSourceFieldValues)} - exception occured", ex);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"{nameof(SaveDataSourceFieldValues)} - exception occured", ex);
                }
            }
        }
    }
}