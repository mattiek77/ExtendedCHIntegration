using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DocumentFormat.OpenXml.Math;
using Sitecore.Abstractions;
using Sitecore.Connector.CMP;
using Sitecore.Connector.CMP.Pipelines.ImportEntity;
using Sitecore.Data.Fields;
using Sitecore.Globalization;
using Sitecore.SecurityModel;
using Sitecore.Data.Items;
using ItemEventHandler = Sitecore.Globalization.ItemEventHandler;

namespace ExtendedCHIntegration.Foundation.DAM.Pipelines.Import
{
    public class EnsureItemVersion : ImportEntityProcessor
    {

        public EnsureItemVersion(BaseLog logger, CmpSettings settings) : base(logger, settings)
        {
        }



        public override void Process(ImportEntityPipelineArgs args, BaseLog logger)
        {

            var workFlow = args.Item[Constants.Workflows.WorkflowStateFieldID];
            var intitial = args.EntityMappingItem[Constants.EntityMapping.Fields.ImportedStartingState];
            if (workFlow == intitial) return;

            using (new SecurityDisabler())
            using (new LanguageSwitcher(args.Language))
            {
                ProcessItemAndLocalDataSources(args.Item, args.EntityMappingItem, args);
                args.Item = args.Item.Database.GetItem(args.Item.ID);
            }
        }



        public void ProcessItemAndLocalDataSources(Item item, Item entityMappingItem, ImportEntityPipelineArgs args)
        {

            var createNewIfNotInitial =
                (CheckboxField) args.EntityMappingItem.Fields[
                    Constants.EntityMapping.Fields.CreateNewVersionIfNotInInitialState];
            if (createNewIfNotInitial.Checked)
            {
                if (!string.IsNullOrEmpty(item[Constants.Workflows.WorkflowStateFieldID]) &&
                    item[Constants.Workflows.WorkflowStateFieldID] !=
                    args.EntityMappingItem[Constants.EntityMapping.Fields.ImportedStartingState])
                {
                    var ni = item.Versions.AddVersion();
                    using (new EditContext(ni))
                    {
                        if (!string.IsNullOrEmpty(
                            entityMappingItem[Constants.EntityMapping.Fields.ImportedStartingState]))
                        {
                            ni[Constants.Workflows.WorkflowStateFieldID] =
                                entityMappingItem[Constants.EntityMapping.Fields.ImportedStartingState];
                        }
                    }
                }
            }
            
            var localDataItems = item.Children.FirstOrDefault(i => i.TemplateID == Constants.LocalDsId)?.Children;
            if (localDataItems == null) return;
            foreach (Item localDataItem in localDataItems)
            {
                ProcessItemAndLocalDataSources(localDataItem, entityMappingItem, args);
            }
        }
    }
}