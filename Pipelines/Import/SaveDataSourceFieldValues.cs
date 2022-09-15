using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using WKSkunkWorks.Foundation.DAM.Helpers;
using Sitecore.Abstractions;
using Sitecore.Connector.CMP;
using Sitecore.Connector.CMP.Conversion;
using Sitecore.Connector.CMP.Pipelines.ImportEntity;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.SecurityModel;
using Sitecore.Data.Items;

namespace WKSkunkWorks.Foundation.DAM.Pipelines.Import
{
    public class SaveDataSourceFieldValues : ImportEntityProcessor
    {
        private readonly ICmpConverterMapper _mapper;
        private readonly SitecoreFieldMappingHelper _sitecoreFieldMappingHelper;
        public SaveDataSourceFieldValues(ICmpConverterMapper mapper, CmpSettings settings,
            BaseLog logger, SitecoreFieldMappingHelper sitecoreFieldHelper) : base(logger, settings)
        {
            _mapper = mapper;
            _sitecoreFieldMappingHelper = sitecoreFieldHelper;
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
                                    where child.TemplateID == Constants.DataSourceTemplateID
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
                        var baseValue = _mapper.Convert(args.EntityDefinition, cmpField,
                            args.Entity.GetPropertyValue(cmpField));
                        itemWeAreUpdating[sitecoreField] =
                            _sitecoreFieldMappingHelper.PerformFieldMapAlteration(baseValue,
                                itemWeAreUpdating.Fields[sitecoreField]);
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