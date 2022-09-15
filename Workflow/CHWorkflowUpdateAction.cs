using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using WKSkunkWorks.Foundation.DAM.Helpers;
using Sitecore.Diagnostics;
using Sitecore.Data.Items;
using Sitecore.DependencyInjection;
using Sitecore.Workflows.Simple;
using Stylelabs.M.Sdk.WebClient;
using MConstants = Sitecore.Connector.CMP.Constants;

namespace WKSkunkWorks.Foundation.DAM.Workflow
{
    public class CHWorkflowUpdateAction
    {
        private readonly ICMPWorkflowHelper _workflowHelper;

        public CHWorkflowUpdateAction()
        {
            _workflowHelper = ServiceLocator.ServiceProvider.GetService<ICMPWorkflowHelper>();
        }

        public void Process(WorkflowPipelineArgs args)
        {
            var updatedItem = args.DataItem;
            var workflowActionItem = args.ProcessorItem?.InnerItem;
            if (workflowActionItem == null) return;
            var targetIdentifier = workflowActionItem[Constants.Workflows.WorkflowAction.StateIdentifier];
            if (string.IsNullOrEmpty(targetIdentifier)) return;
            var contentHubIdentifier = updatedItem[Constants.ContentHubEntity.Fields.VersionSpecificIdField];
            if (string.IsNullOrEmpty(contentHubIdentifier)) return;
            _workflowHelper.SetItemWorkflowState(contentHubIdentifier, targetIdentifier).GetAwaiter().GetResult();
            var comments = string.Join(Environment.NewLine, args.CommentFields.Where(x => x.Key.ToLower().Contains("comments") && !string.IsNullOrEmpty(x.Value)).Select(x=>$"{DateTime.Now.ToShortDateString()} -- {x.Value}"));
            if (comments.Length>0)
            {
                _workflowHelper.SendNotes(contentHubIdentifier, comments,
                    workflowActionItem[Constants.Workflows.WorkflowCommentsFieldName]).GetAwaiter().GetResult();
            }
        }

    }
}