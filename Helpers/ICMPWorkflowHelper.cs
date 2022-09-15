using System.Threading.Tasks;

namespace WKSkunkWorks.Foundation.DAM.Helpers
{
    public interface ICMPWorkflowHelper
    {
        Task SetItemWorkflowState(string cmpIdentifier, string targetWorkflowIdentifier);
        Task SendNotes(string cmpIdentifier, string notes, string fieldName);
    }
}