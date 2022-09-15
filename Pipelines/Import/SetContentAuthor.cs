using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Abstractions;
using Sitecore.Connector.CMP;
using Sitecore.Connector.CMP.Helpers;
using Sitecore.Connector.CMP.Pipelines.ImportEntity;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.SecurityModel;

namespace WKSkunkWorks.Foundation.DAM.Pipelines.Import
{
    public class SetContentAuthor : ImportEntityProcessor
    {
        private readonly CmpHelper _cmpHelper;
        public SetContentAuthor(BaseLog logger, CmpSettings settings, CmpHelper cmpHelper) : base(logger, settings)
        {
            _cmpHelper = cmpHelper;
        }

        public override void Process(ImportEntityPipelineArgs args, BaseLog logger)
        {
            var source = args.Entity;
            if (!source.CreatedBy.HasValue)
            {
                return;
            }
            var authorEntity = _cmpHelper.GetEntity(source.CreatedBy.Value);
            var damUserNameTask = authorEntity.GetPropertyValueAsync<string>("Username").ConfigureAwait(false);
            var damUserName = damUserNameTask.GetAwaiter().GetResult();
            if (!string.IsNullOrWhiteSpace(damUserName))
            {
                using (new SecurityDisabler())
                using (new LanguageSwitcher(args.Language))
                using (new EditContext(args.Item))
                {
                    args.Item[Constants.ContentHubEntity.Fields.CmpAuthorFieldId] = damUserName;
                    args.Item[Constants.CreatedByField] = args.Item[Constants.ItemOwnerField] = damUserName;
                }
            }
        }
    }
}