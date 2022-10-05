namespace ExtendedCHIntegration.Foundation.DAM.Pipelines.Rendering
{
    public class ContentEditorImportRendering
    {
        public void Process(
            Sitecore.Shell.Applications.ContentEditor.Pipelines.RenderContentEditor.RenderContentEditorArgs args)
        {
            args.EditorFormatter = new RenderingFormatter
            {
                Arguments = args
            };
        }
    }
}