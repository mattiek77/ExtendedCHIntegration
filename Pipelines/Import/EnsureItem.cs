using WKSkunkWorks.Foundation.DAM.Helpers;
using Sitecore.Connector.CMP;
using Sitecore.Abstractions;
using Sitecore.Connector.CMP.Helpers;
using Sitecore.Connector.CMP.Pipelines.ImportEntity;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.SecurityModel;
using Sitecore.Data.Fields;


namespace WKSkunkWorks.Foundation.DAM.Pipelines.Import
{
    public class EnsureItem : Sitecore.Connector.CMP.Pipelines.ImportEntity.EnsureItem
    {
        private readonly BaseFactory _factory;
        private readonly CmpSettings _settings;
        private readonly CmpHelper _cmpHelper;

        public EnsureItem(
            BaseFactory factory,
            BaseLog logger,
            BaseCacheManager cacheManager,
            CmpSettings settings,
            CmpSettings CMPsettings, CmpHelper cmpHelper, IOneWebCMPSitecoreHelper cmpSitecoreHelper)
            : base(factory, logger, settings, cmpHelper)
        {
            _factory = factory;
            _settings = CMPsettings;
            _cmpHelper = cmpHelper;
        }

        public override void Process(ImportEntityPipelineArgs args, BaseLog logger)
        {
            if (args.EntityMappingItem == null)
                args.EntityMappingItem = this._cmpHelper.GetEntityMappingItem(args);

            var active = ((CheckboxField) args.EntityMappingItem.Fields[Constants.EntityMapping.Fields.ActiveStateField])
                .Checked;

            if (!active)
            {
                logger.Info($"Item from CMP {args.Entity.Id} - imported - configuration {args.EntityMappingItem.Name} not enabled - aborting", this);
                args.AbortPipeline();
                return;
            }

            if (args.Item != null)
            {
                SetVersionSpecificEntityIdentifier(args.Item, args.Entity.Identifier);
                return;
            }
           
            Assert.IsNotNull((object)args.EntityMappingItem,
                "Could not find any Entity Mapping item for the Entity Type (Schema): " + args.ContentTypeIdentifier);


            var template = _factory.GetDatabase(_settings.DatabaseName).GetItem(
                new ID(args.EntityMappingItem[Sitecore.Connector.CMP.Constants.EntityMappingTemplateFieldId]),
                args.Language);
            if (IsTemplate(template))
            {
                base.Process(args, logger);
            }
            else
            {
                if (IsBranchTemplate(template, out var brTemplate))
                {
                    using (new SecurityDisabler())
                    using (new LanguageSwitcher(args.Language))
                    {
                        Database database = this._factory.GetDatabase(_settings.DatabaseName);
                        Assert.IsNotNull((object) database, "Could not get the master database.");
                        Item cmpItemBucket = this._cmpHelper.GetCmpItemBucket(args, database);
                        Assert.IsNotNull((object) cmpItemBucket,
                            "Could not find the item bucket. Check this field value in the configuration item.");
                        string propertyValue = args.Entity.GetPropertyValue<string>(
                            args.EntityMappingItem[Constants.EntityMappingItemNamePropertyField]);
                        Assert.IsNotNullOrEmpty(propertyValue,
                            "Could not get the property value from Content Hub as Sitecore item name. Check this field isn't valid and the value should not be null in Content Hub.");
                        string name = ItemUtil.ProposeValidItemName(propertyValue);
                        Assert.IsNotNullOrEmpty(name, "Could not proposed the valid item name as Sitecore Item Name.");
                        args.Item = cmpItemBucket.Add(name, brTemplate);
                    }
                }
            }
            SetVersionSpecificEntityIdentifier(args.Item, args.EntityIdentifier);
        }

        private static void SetVersionSpecificEntityIdentifier(Item i, string identifier)
        {
            using (new SecurityDisabler())
            using (new EditContext(i))
            {
                if (i != null)
                    i[Constants.ContentHubEntity.Fields.VersionSpecificIdField] = identifier;
            }
        }

        private static bool IsBranchTemplate(Item i, out BranchItem br)
        {
            try
            {
                br = (BranchItem)i;
                return true;
            }
            catch
            {
                br = null;
                return false;
            }
        }

        private static bool IsTemplate(Item i)
        {
            try
            {
                var tem = (TemplateItem)i;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}