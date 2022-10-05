using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Pipelines;
using Sitecore.Data.Items;
namespace ExtendedCHIntegration.Foundation.DAM.Pipelines.Export
{
    public class DAMNotificationEventArgs : PipelineArgs
    {
        public Item Item { get; set; }
        public Item MappingRootItem { get; set; }
        public UpdateType UpdateType { get; set; }
        public string DAMItemID { get; set; }
        public string ContentHubIdentifier { get; set; }
        public string EntitySchema { get; set; }
        public long? DamID { get; set; }
    }
}