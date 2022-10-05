using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExtendedCHIntegration.Foundation.DAM.Helpers;
using Sitecore.Abstractions;
using Sitecore.Connector.CMP;
using Sitecore.Connector.CMP.Conversion;
using Sitecore.Connector.CMP.Helpers;
using Sitecore.Connector.CMP.Pipelines.ImportEntity;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.SecurityModel;

namespace ExtendedCHIntegration.Foundation.DAM.Pipelines.Import
{
    public class TextMapping : ImportEntityProcessor
    {
        private readonly SitecoreFieldMappingHelper _sitecoreFieldMappingHelper;
        private readonly CmpHelper _cmpHelper;
        private readonly CmpSettings _settings;
        private readonly ICmpConverterMapper _mapper;

        public TextMapping(ICmpConverterMapper mapper, BaseLog logger, CmpHelper helper, CmpSettings settings,
            SitecoreFieldMappingHelper sitecoreFieldMappingHelper) : base(
            logger, settings)
        {
            _sitecoreFieldMappingHelper = sitecoreFieldMappingHelper;
            _cmpHelper = helper;
            _settings = settings;
            _mapper = mapper;
        }

        public override void Process(ImportEntityPipelineArgs args, BaseLog logger)
        {
            using (new SecurityDisabler())
            {
                using (new LanguageSwitcher(args.Language))
                {
                    using (new EditContext(args.Item))
                    {
                        args.Item[Sitecore.Connector.CMP.Constants.EntityIdentifierFieldId] = args.EntityIdentifier;
                        TryMapConfiguredFields(args);
                    }
                }
            }
        }

        private void TryMapConfiguredFields(ImportEntityPipelineArgs args)
        {
            if (args.EntityMappingItem == null)
                args.EntityMappingItem = _cmpHelper.GetEntityMappingItem(args);
            if (args.EntityMappingItem == null)
                return;
            foreach (var setting in args.EntityMappingItem.Children.Where(i =>
                i.TemplateID == Sitecore.Connector.CMP.Constants.FieldMappingTemplateId ||
                i.TemplateID == Sitecore.Connector.CMP.Constants.RelationFieldMappingTemplateId))
            {
                var fieldName = setting[Sitecore.Connector.CMP.Constants.FieldMappingSitecoreFieldNameFieldId];
                var cmpFieldName = setting[Sitecore.Connector.CMP.Constants.FieldMappingCmpFieldNameFieldId];
                if (!string.IsNullOrEmpty(fieldName) && !string.IsNullOrEmpty(cmpFieldName))
                {
                    try
                    {
                        if (setting.TemplateID == Sitecore.Connector.CMP.Constants.RelationFieldMappingTemplateId)
                        {
                            string cmpRelationName =
                                setting[
                                    Sitecore.Connector.CMP.Constants
                                        .RelationFieldMappingCmpRelationFieldNameFieldId];
                            if (string.IsNullOrEmpty(cmpRelationName))
                            {
                                continue;
                            }

                            List<string> stringList =
                                this._cmpHelper.TryMapRelationPropertyValues(args, cmpRelationName, cmpFieldName);
                            args.Item[fieldName] = stringList.Count != 0
                                ? string.Join(_settings.RelationFieldMappingSeparator,
                                    (IEnumerable<string>) stringList)
                                : string.Empty;
                            continue;
                        }

                        var baseData = _mapper.Convert(args.EntityDefinition, cmpFieldName,
                            args.Entity.GetPropertyValue(cmpFieldName));
                        args.Item[fieldName] =
                            _sitecoreFieldMappingHelper.PerformFieldMapAlteration(baseData, args.Item.Fields[fieldName]);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(
                            BaseHelper.GetLogMessageText(_settings.LogMessageTitle,
                                string.Format(
                                    "An error occured during converting '{0}' field to '{1}' field. Field mapping ID: '{2}'.",
                                    cmpFieldName, fieldName, setting.ID)), ex, this);
                    }
                }
            }
        }
    }
}