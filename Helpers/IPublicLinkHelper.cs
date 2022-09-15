using System.Threading.Tasks;

namespace WKSkunkWorks.Foundation.DAM.Helpers
{
    public interface IPublicLinkHelper
    {
        Task<string> GetPublicLink(string source, string host);
    }
}