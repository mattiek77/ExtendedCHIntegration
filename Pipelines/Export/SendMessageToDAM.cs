using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ExtendedCHIntegration.Foundation.DAM.Helpers;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
namespace ExtendedCHIntegration.Foundation.DAM.Pipelines.Export
{
    public class SendMessageToDAM : DAMExportPipelineBase
    {
        private readonly ICMPExporter _cmpExporter;

        public SendMessageToDAM(ICMPExporter cmpExporter)
        {
            _cmpExporter = cmpExporter;
        }

        public override void Process(DAMNotificationEventArgs args)
        {
            base.Process(args);
            if (Args.UpdateType == UpdateType.Update)
            {
                UpdateInDAM();
            }
            else
            {
                if (((CheckboxField)Args.MappingRootItem.Fields[Constants.ExportTemplate.Fields.ProcessDeletes])
                    .Checked && !string.IsNullOrEmpty(Args.DAMItemID))
                {
                    DeleteInDAM();
                }
            }
        }

        private void DeleteInDAM()
        {
            _cmpExporter.DeleteFromDAM(Args.ContentHubIdentifier);
            Args.AbortPipeline();
        }

        private void UpdateInDAM()
        {
            Args.DamID = _cmpExporter.AddToDam(Args.EntitySchema, GetItemName(), GetDAMNameFieldName(), Args.DAMItemID, GetExportFields(true), GetExportFields(false), GetCulture(Args.Item)).Result;
            if (Args.DamID == null)
            {
                Sitecore.Diagnostics.Log.Error($"Failed to add item to DAM aborting pipeline {Args.Item.Name}", this);
            }
        }

        private string GetDAMNameFieldName()
        {
            return Args.MappingRootItem[Constants.ExportTemplate.Fields.NameInDamField];
        }

        private string GetItemName()
        {
            var nameField = Args.MappingRootItem[Constants.ExportTemplate.Fields.NameField];

            if (string.IsNullOrEmpty(nameField))
            {
                return Args.Item.Name;
            }

            var langField = Args.MappingRootItem[Constants.ExportTemplate.Fields.LangVersionForName];
            Item language = null;
            string valueFromItemOfName = string.Empty;
            if (!string.IsNullOrEmpty(langField) && ID.TryParse(langField, out var langId))
            {
                language = Args.Item.Database.GetItem(langId);
                var langVersion = Args.Item.Versions.GetVersions(true).Where(x => x.Language.Name.Equals(language.Name))
                    .OrderByDescending(i => i.Version.Number).FirstOrDefault();
                if (!string.IsNullOrEmpty(langVersion?[nameField]))
                {
                    valueFromItemOfName = langVersion[nameField];
                }
            }
            else
            {
                valueFromItemOfName = Args.Item[nameField];
            }
            return string.IsNullOrEmpty(valueFromItemOfName) ? Args.Item.Name : valueFromItemOfName;
        }

        private CultureInfo GetCulture(Item i)
        {
            var mappingList = (NameValueListField)Args.MappingRootItem.Fields[Constants.ExportTemplate.Fields.LanguageMapping];
            if (mappingList.NameValues.AllKeys.Contains(i.Language.Name))
            {
                var mapping = mappingList.NameValues[i.Language.Name];
                return new CultureInfo(mapping);
            }

            return new CultureInfo(i.Language.Name);
        }

        private IEnumerable<Tuple<string, string, bool>> GetExportFields(bool required)
        {
            return from field in Args.MappingRootItem.Children
                   where field.TemplateID == Constants.ExportFieldTemplate.TemplateID
                   where ((CheckboxField)field.Fields[Constants.ExportFieldTemplate.Fields.Required]).Checked == required
                   select new Tuple<string, string, bool>(
                       !string.IsNullOrEmpty(Args.Item[field[Constants.ExportFieldTemplate.Fields.SitecoreField]])
                           ? Args.Item[field[Constants.ExportFieldTemplate.Fields.SitecoreField]]
                           : Args.Item[field[Constants.ExportFieldTemplate.Fields.DefaultIfEmpty]], field[
                           Constants.ExportFieldTemplate.Fields.DAMField],
                       ((CheckboxField)field.Fields[Constants.ExportFieldTemplate.Fields.IsLocalisable]).Checked);
        }
    }
}