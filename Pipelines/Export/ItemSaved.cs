using System;
using System.Security.AccessControl;
using Sitecore.Data.Items;
using Sitecore.Events;
using Sitecore.Pipelines;

namespace WKSkunkWorks.Foundation.DAM.Pipelines.Export
{
    public class ItemSaved
    {
        public void OnItemSaved(object sender, EventArgs ea)
        {
            var item = Event.ExtractParameter<Item>(ea, 0);
            var eventArgs = new DAMNotificationEventArgs()
            {
                Item = item,
                UpdateType = UpdateType.Update
            };
            CorePipeline.Run("wk.exportTaxonomy", eventArgs);
        }

        public void OnItemDeleted(object sender, EventArgs ea)
        {
            var item = Event.ExtractParameter<Item>(ea, 0);
            var eventArgs = new DAMNotificationEventArgs()
            {
                Item = item,
                UpdateType = UpdateType.Delete
            };
            CorePipeline.Run("wk.exportTaxonomy", eventArgs);
        }
    }
}