using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.UI;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.DependencyInjection;
using Sitecore.Mvc.Extensions;
using Sitecore.Shell.Applications.ContentManager;
using Sitecore.Web.UI.HtmlControls;
using Control = System.Web.UI.Control;
using Page = Sitecore.Web.UI.HtmlControls.Page;

namespace WKSkunkWorks.Foundation.DAM.Pipelines.Rendering
{
    public class RenderingFormatter : Sitecore.Shell.Applications.ContentEditor.EditorFormatter
    {
        private ImportMappingService ImportMappingService =>
            ServiceLocator.ServiceProvider.GetRequiredService<ImportMappingService>();

        public override void RenderLabel(Control parent, Editor.Field field, Item fieldType, bool readOnly)
        {
            var page = field.ItemField.Item;
            var importReadOnly = ImportMappingService.IsReadOnly(field, page);
            base.RenderLabel(parent, field, fieldType, readOnly||importReadOnly);
        }

        public override void RenderField(Control parent, Editor.Field field, Item fieldType, bool readOnly)
        {
            var page = field.ItemField.Item;
            var importReadOnly = ImportMappingService.IsReadOnly(field, page);
            base.RenderField(parent, field, fieldType, readOnly|| importReadOnly);
        }


        public override void RenderField(Control parent, Editor.Field field, bool readOnly)
        {
            var page = field.ItemField.Item;
            var importReadOnly = ImportMappingService.IsReadOnly(field, page);
            base.RenderField(parent, field, readOnly|| importReadOnly);
        }

        public override void RenderField(Control parent, Editor.Field field, Item fieldType, bool readOnly, string value)
        {
            var page = field.ItemField.Item;
            var importReadOnly = ImportMappingService.IsReadOnly(field, page);
            base.RenderField(parent, field, fieldType, readOnly || importReadOnly, value);
        }


        
    }
}