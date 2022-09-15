using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Abstractions;
using Sitecore.Connector.CMP;
using Sitecore.Connector.CMP.Pipelines.ImportEntity;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;

namespace WKSkunkWorks.Foundation.DAM.Pipelines.Import
{
    public class SetCmpWorkflowAndState : ImportEntityProcessor
    {

        public SetCmpWorkflowAndState(BaseLog logger, CmpSettings settings) : base(logger, settings)
        {
        }

        public override void Process(ImportEntityPipelineArgs args, BaseLog logger)
        {
            if ((args.CustomData.ContainsKey(Constants.EXISTING_ITEM) &&
                  (bool)args.CustomData[Constants.EXISTING_ITEM])) return;

            var targetWorkFlow = args.EntityMappingItem[Constants.EntityMapping.Fields.CmpItemWorkflow];
            var startingState = args.EntityMappingItem[Constants.EntityMapping.Fields.ImportedStartingState];

            if (string.IsNullOrEmpty(targetWorkFlow) || string.IsNullOrEmpty(startingState)) return;

            var mainItem = args.Item;
            foreach (var item in LocalDataSources(mainItem))
            {
                if (item != null)
                {
                    using (new SecurityDisabler())
                    using (new EditContext(item))
                    {
                        item[Constants.Workflows.WorkflowFieldID] = targetWorkFlow;
                        item[Constants.Workflows.WorkflowStateFieldID] = startingState;
                    }
                }
            }
        }

        private IEnumerable<Item> LocalDataSources(Item parent)
        {
            if (parent == null)
            {
                yield break;
            }
            yield return parent;
            var localData = parent.Children.FirstOrDefault(i => i.TemplateID == Constants.LocalDsId);

            if (localData != null)
            {
                foreach (Item child in localData.Children)
                {
                    yield return child;
                }
            }
        }
    }

}