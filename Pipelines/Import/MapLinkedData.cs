using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WKSkunkWorks.Foundation.DAM.Helpers;
using Sitecore.Abstractions;
using Sitecore.Connector.CMP;
using Sitecore.Connector.CMP.Helpers;
using Sitecore.Connector.CMP.Pipelines.ImportEntity;
using Sitecore.Connector.CMP.Models;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.SecurityModel;
using Stylelabs.M.Sdk.Contracts.Base;

namespace WKSkunkWorks.Foundation.DAM.Pipelines.Import
{
    public class MapLinkedData : ImportEntityProcessor
    {
        private readonly CmpSettings _settings;
        private readonly BaseFactory _factory;
        private readonly CmpHelper _cmpHelper;
        private readonly IOneWebCMPSitecoreHelper _sitecoreHelper;

        public MapLinkedData(
            BaseFactory factory,
            CmpHelper cmpHelper,
            IOneWebCMPSitecoreHelper sitecoreHelper,
            BaseLog logger,
            CmpSettings settings)
            : base(logger, settings)
        {
            _factory = factory;
            _settings = settings;
            _cmpHelper = cmpHelper;
            _sitecoreHelper = sitecoreHelper;
        }

        public override void Process(ImportEntityPipelineArgs args, BaseLog logger)
        {
            Assert.IsNotNull((object)args.Item, "The item is null.");
            Assert.IsNotNull((object)args.Language, "The language is null.");
            using (new SecurityDisabler())
            using (new LanguageSwitcher(args.Language))
            {
                try
                {
                    using (new EditContext(args.Item))
                    {
                        args.Item[Sitecore.Connector.CMP.Constants.EntityIdentifierFieldId] = args.EntityIdentifier;
                        TryMapItemRelation(args);
                    }
                }
                catch(Exception ex)
                {
                    logger.Error("Error mapping related items", ex, this);
                }
            }
        }


        internal void TryMapItemRelation(ImportEntityPipelineArgs args)
        {
            if (args.EntityMappingItem == null)
                args.EntityMappingItem = this._cmpHelper.GetEntityMappingItem(args);
            Assert.IsNotNull((object)args.EntityMappingItem, "Could not find any Entity Mapping item for the Entity Type (Schema): " + args.ContentTypeIdentifier);
            foreach (Item obj1 in args.EntityMappingItem.Children.Where(i => i.TemplateID.Equals(Constants.LinkedDataItemTemplateID)))
            {
                string fieldName1 = obj1[Sitecore.Connector.CMP.Constants.FieldMappingSitecoreFieldNameFieldId];
                string cmpRelationName = obj1[Sitecore.Connector.CMP.Constants.RelationFieldMappingCmpRelationFieldNameFieldId];
                if (!(string.IsNullOrEmpty(fieldName1) || string.IsNullOrEmpty(cmpRelationName)))
                {

                    try
                    {
                        Database database = this._factory.GetDatabase(_settings.DatabaseName);
                        Assert.IsNotNull((object) database, "Could not get the master database.");
                        var relationEntities = this._cmpHelper.GetRelationEntities(args, cmpRelationName);
                        Item obj2 = database.GetItem(Sitecore.Connector.CMP.Constants.ConfigItemId, args.Language);
                        Assert.IsNotNull((object) obj2, "Could not get the CMP Config item.");
                        List<string> source = new List<string>();
                        foreach (CmpEntityModel cmpEntityModel in relationEntities)
                        {

                            ID itemId;
                            if (this._sitecoreHelper.SearchItem(Constants.EntityIdentifier,
                                cmpEntityModel.EntityIdentifier, obj2, out itemId))
                            {
                                source.Add(itemId?.ToString());
                            }

                        }

                        args.Item[fieldName1] = source.Count != 0
                            ? string.Join("|", source.Distinct<string>().ToList<string>())
                            : string.Empty;
                    }
                    catch (Exception ex)
                    {
                        this.Logger.Error(
                            BaseHelper.GetLogMessageText(_settings.LogMessageTitle,
                                string.Format(
                                    "An error occured during mapping related entity of CMP Relation '{0}' to '{1}' item. Field mapping ID: '{2}'.",
                                    (object) cmpRelationName, (object) args.Item.Name, (object) obj1.ID)), ex,
                            (object) this);
                    }

                }
                else
                {
                    this.Logger.Error(
                        BaseHelper.GetLogMessageText(_settings.LogMessageTitle,
                            string.Format(
                                "Configuration of the field mapping '{0}' is incorrect. Required fields are not specified.",
                                (object) obj1.ID)), (object) this);
                }
            }
        }
    }
}