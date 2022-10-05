using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using Sitecore.Pipelines.RenderField;
using Sitecore.Data.Items;

namespace ExtendedCHIntegration.Foundation.DAM.Pipelines.Rendering
{
    public class ExperienceEditorImportReadOnlyOverride
    {
        private ImportMappingService ImportMappingService =>
            ServiceLocator.ServiceProvider.GetRequiredService<ImportMappingService>();

        public void Process(RenderFieldArgs args)
        {
            if (args.DisableWebEdit)
                return;

            args.DisableWebEdit = ImportMappingService.IsReadOnly(args.FieldName, args.Item);

        }
    }
}