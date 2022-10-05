using Sitecore.Data;
using Sitecore.Data.Items;

namespace ExtendedCHIntegration.Foundation.DAM.Helpers
{
    public interface IOneWebCMPSitecoreHelper
    {
        bool SearchItem(string fieldName, string fieldValue, Item item, out ID itemId);
    }
}