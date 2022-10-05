using System;
using System.Linq;
using System.Threading.Tasks;
using Stylelabs.M.Base.Querying;
using Stylelabs.M.Framework.Essentials.LoadConfigurations;
using Stylelabs.M.Sdk.Contracts.Base;
using Stylelabs.M.Sdk.WebClient;

namespace ExtendedCHIntegration.Foundation.DAM.Helpers
{
    public class CMPWorkflowHelper : ICMPWorkflowHelper
    {
        private readonly WebClientFactory _webClientFactory;
        private IWebMClient Client => _webClientFactory.GetClient;

        public CMPWorkflowHelper(WebClientFactory clientFactory)
        {
            _webClientFactory = clientFactory;
        }
       
        public async Task SetItemWorkflowState(string cmpIdentifier, string targetWorkflowIdentifier)
        {
            var query = ByIdentity(cmpIdentifier);
            var workflowQuery = ByIdentity(targetWorkflowIdentifier);
            var workFlowItemTask = Client.Querying.SingleIdAsync(workflowQuery);
            var cmpItem = await Client.Querying.SingleAsync(query, EntityLoadConfiguration.Full)
                .ConfigureAwait(false);
            var relationship = await cmpItem.GetRelationAsync<IParentToManyChildrenRelation>(Constants.WorkflowRelationshipName).ConfigureAwait(false);

            var workFlowItemId = await workFlowItemTask.ConfigureAwait(false);

            if (workFlowItemId.HasValue)
            {
                relationship.Children.Clear();
                relationship.Children.Add(workFlowItemId.Value);
                await Client.Entities.SaveAsync(cmpItem).ConfigureAwait(false);
            }
        }

        public async Task SendNotes(string cmpIdentifier, string notes, string fieldName)
        {
            var query = ByIdentity(cmpIdentifier);
            var cmpItem = await Client.Querying.SingleAsync(query, EntityLoadConfiguration.Full)
                .ConfigureAwait(false);
            var currentNotes = await cmpItem.GetPropertyValueAsync<string>(fieldName).ConfigureAwait(false)??string.Empty;
            if (currentNotes.Length > 0)
            {
                currentNotes += Environment.NewLine;
            }

            currentNotes += notes;
            cmpItem.SetPropertyValue(fieldName, currentNotes);
            await Client.Entities.SaveAsync(cmpItem).ConfigureAwait(false);
        }

        private Query ByIdentity(string entityIdentifier)
        {
            return Query.CreateEntitiesQuery(entities =>
                from entity in entities
                where entity.Identifier == entityIdentifier
                select entity
            );
        }
    }
}