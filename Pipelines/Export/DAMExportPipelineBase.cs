using Sitecore.Data;

namespace WKSkunkWorks.Foundation.DAM.Pipelines.Export
{
    public abstract class DAMExportPipelineBase
    {
        internal DAMNotificationEventArgs Args { get; set; }
        internal Database RunningDb = Database.GetDatabase(Constants.DbName);

        public virtual void Process(DAMNotificationEventArgs args)
        {
            Args = args;
        }
    }
}