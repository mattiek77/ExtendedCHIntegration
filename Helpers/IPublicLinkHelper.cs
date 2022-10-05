using System.Threading.Tasks;

namespace ExtendedCHIntegration.Foundation.DAM.Helpers
{
    public interface IPublicLinkHelper
    {
        Task<string> GetPublicLink(string source, string host);
    }
}