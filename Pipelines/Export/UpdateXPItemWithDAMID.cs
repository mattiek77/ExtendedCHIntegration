using ExtendedCHIntegration.Foundation.DAM.Helpers;
using Sitecore.Data.Events;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using MConstants = Sitecore.Connector.CMP.Constants;
namespace ExtendedCHIntegration.Foundation.DAM.Pipelines.Export
{
    public class UpdateXPItemWithDamID : DAMExportPipelineBase
    {
        private readonly ICMPExporter _cmpExporter;
        public UpdateXPItemWithDamID(ICMPExporter cmpExporter)
        {
            _cmpExporter = cmpExporter;
        }

        public override void Process(DAMNotificationEventArgs args)
        {
            base.Process(args);
            var createdIdentifier = _cmpExporter.GetCHIdentifier(Args.DamID.Value).Result;
            using (new SecurityDisabler())
            using (new EditContext(Args.Item))
            using (new EventDisabler())
            {
                Args.Item[MConstants.EntityIdentifierFieldId] = createdIdentifier;
            }
        }
    }
}