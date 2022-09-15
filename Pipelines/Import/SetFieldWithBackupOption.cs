using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DocumentFormat.OpenXml.Math;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using Sitecore.Abstractions;
using Sitecore.Connector.CMP;
using Sitecore.Connector.CMP.Helpers;
using Sitecore.Connector.CMP.Pipelines.ImportEntity;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.SecurityModel;
using Stylelabs.M.Sdk.Contracts.Base;
using WKSkunkWorks.Foundation.DAM.Helpers;
using Sitecore.Connector.CMP.Conversion;


namespace WKSkunkWorks.Foundation.DAM.Pipelines.Import
{
    public class SetFieldWithBackupOption : ImportEntityProcessor
    {
        private readonly ICmpConverterMapper _mapper;
        private readonly SitecoreFieldMappingHelper _sitecoreFieldMappingHelper;

        public SetFieldWithBackupOption(ICmpConverterMapper mapper, BaseLog logger, CmpSettings settings, CmpHelper helper, SitecoreFieldMappingHelper sitecoreFieldHelper) : base(logger, settings)
        {
            _sitecoreFieldMappingHelper = sitecoreFieldHelper;
            _mapper = mapper;
        }

        public override void Process(ImportEntityPipelineArgs args, BaseLog logger)
        {
            logger.Debug($"Setting item from CH item {args.Entity.Identifier}");
            args.EntityMappingItem.Children.Where(child =>
                child.TemplateID.Equals(Constants.MappingWithBackup.TemplateID)).ForEach(settingItem=>SetValue(args, settingItem));
        }

        public void SetValue(ImportEntityPipelineArgs args, Item settingItem)
        {
            using (new SecurityDisabler())
            using (new LanguageSwitcher(args.Language))
            using (new EditContext(args.Item))
            {
                var mainCHField = settingItem[Sitecore.Connector.CMP.Constants.FieldMappingCmpFieldNameFieldId];
                var backupCHField = settingItem[Constants.MappingWithBackup.Fields.BackupFieldID];
                var mainPropertyValue = _mapper.Convert(args.EntityDefinition, mainCHField,
                           args.Entity.GetPropertyValue(mainCHField));
                var backupPropertyValue = _mapper.Convert(args.EntityDefinition, backupCHField,
                           args.Entity.GetPropertyValue(backupCHField));

                var finalPropertyValue =
                    !string.IsNullOrEmpty(mainPropertyValue) ? mainPropertyValue : backupPropertyValue;
                if (!string.IsNullOrEmpty(finalPropertyValue))
                {
                    var fieldID = settingItem[Sitecore.Connector.CMP.Constants.FieldMappingSitecoreFieldNameFieldId];
                    if (fieldID != null)
                    {
                        args.Item[fieldID] =
                            _sitecoreFieldMappingHelper.PerformFieldMapAlteration(finalPropertyValue, args.Item.Fields[fieldID]);
                    }
                }
            }
        } 
    }
}