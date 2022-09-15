using System.Linq;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace WKSkunkWorks.Foundation.DAM.Helpers
{
    public class OneWebCMPSitecoreHelper : IOneWebCMPSitecoreHelper
    {

        public bool SearchItem(string fieldName, string fieldValue, Item item, out ID itemId)
        {

            Assert.IsNotNull((object)fieldValue, "Could not get the " + fieldName + ".");
            using (IProviderSearchContext searchContext = ContentSearchManager.GetIndex(new SitecoreIndexableItem(item)).CreateSearchContext())
            {
                SearchResultItem searchResultItem1 = searchContext.GetQueryable<SearchResultItem>()
                    .FirstOrDefault(searchItem =>
                        searchItem[fieldName] == fieldValue);
                if (searchResultItem1 == null)
                {
                    itemId = ID.Null;
                    return false;
                }
                itemId = searchResultItem1.ItemId;
                return true;
            }
        }

    }
}