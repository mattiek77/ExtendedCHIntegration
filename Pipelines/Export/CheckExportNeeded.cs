using System;
using System.Linq;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Managers;
using Sitecore.Data.Validators;
using MConstants = Sitecore.Connector.CMP.Constants;
namespace WKSkunkWorks.Foundation.DAM.Pipelines.Export
{
    public class CheckExportNeeded : DAMExportPipelineBase
    {
      public override void Process(DAMNotificationEventArgs args)
        {
            base.Process(args);
            if (!CheckExportConfig())
            {
                args.AbortPipeline();
            }
        }

        private bool CheckExportConfig()
        {
            var settingRoot = RunningDb.GetItem(Sitecore.Connector.CMP.Constants.ConfigItemId);
            if (settingRoot == null)
            {
                Sitecore.Diagnostics.Log.Info("No CMP Setting Item Found", this);
                return false;
            }

            var savedItemTemplate = TemplateManager.GetTemplate(Args.Item);

            if (savedItemTemplate == null)
            {
                return false;
            }

            if (!savedItemTemplate.InheritsFrom(Constants.ContentHubEntryTemplateID))
            {
                return false;
            }

            if (!IsValid() && Args.UpdateType != UpdateType.Delete)
            {
                return false;
            }

            var exportConfigs = from settingItem in settingRoot.Children
                                where settingItem.TemplateID == Constants.ExportTemplate.TemplateID
                                where settingItem[Constants.ExportTemplate.Fields.SitecoreTemplate] == Args.Item.TemplateID.ToString()
                                select settingItem;
            var export = exportConfigs.FirstOrDefault();
            if (export == null)
            {
                return false;
            }

            var exportTemplateValue = export[Constants.ExportTemplate.Fields.ContentHubSchema];
            if (string.IsNullOrEmpty(exportTemplateValue))
            {
                Sitecore.Diagnostics.Log.Info($"Export Template exists - but has no Content hub schema {export.Name}", this);
            }

            if (Args.UpdateType == UpdateType.Delete &&
                !((CheckboxField)export.Fields[Constants.ExportTemplate.Fields.ProcessDeletes]).Checked)
            {
                return false;
            }

            if (Args.UpdateType == UpdateType.Update &&
                !string.IsNullOrEmpty(Args.Item[MConstants.EntityIdentifierFieldId]) &&
                !((CheckboxField)export.Fields[Constants.ExportTemplate.Fields.ProcessUpdates]).Checked)
            {
                return false;
            }

            var exportLangs = export[Constants.ExportTemplate.Fields.LanguageToExport]
                .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(id => Args.Item.Database.GetItem(new ID(id))?.Name);

            if (!exportLangs.Contains(Args.Item.Language.Name))
            {
                return false;
            }


            Args.MappingRootItem = export;
            Args.DAMItemID = Args.Item[MConstants.EntityIdentifierFieldId];
            Args.EntitySchema = exportTemplateValue;
            return true;
        }

        private bool IsValid()
        {
            var validators = ValidatorManager.BuildValidators(ValidatorsMode.ValidateButton, Args.Item);
            var options = new ValidatorOptions(true);
            ValidatorManager.Validate(validators, options);

            bool valid = true;
            foreach (BaseValidator val in validators)
            {
                if (val.Result == ValidatorResult.CriticalError || val.Result == ValidatorResult.FatalError)
                {
                    valid = false;
                    break;
                }
            }
            return valid;
        }
    }
}